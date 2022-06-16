using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using AquaCoaster.ViewModel;
using AquaCoaster.Utilities;
using Point = System.Windows.Point;

namespace AquaCoaster.View
{
    public static class CompositionTargetEx
    {
        private static TimeSpan _last = TimeSpan.Zero;
        private static event EventHandler<RenderingEventArgs> _FrameUpdating;
        public static event EventHandler<RenderingEventArgs> Rendering
        {
            add
            {
                if (_FrameUpdating == null)
                    CompositionTarget.Rendering += CompositionTarget_Rendering;
                _FrameUpdating += value;
            }
            remove
            {
                _FrameUpdating -= value;
                if (_FrameUpdating == null)
                    CompositionTarget.Rendering -= CompositionTarget_Rendering;
            }
        }
        static void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            RenderingEventArgs args = (RenderingEventArgs)e;
            if (args.RenderingTime == _last)
                return;
            _last = args.RenderingTime; _FrameUpdating(sender, args);
        }
    }

    public class VisualHost : UIElement
    {
        public Visual Visual { get; set; }

        protected override int VisualChildrenCount
        {
            get { return Visual != null ? 1 : 0; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return Visual;
        }
    }

    public class GameGridView : Canvas
    {
        private class AnimationState
        {
            public double CurrentX, CurrentY;
            public double StartX, StartY;
            public double TargetX, TargetY;
            public double elapsed;
        }

        private static Single PERSON_SCALE_X = 0.5f;
        private static Single PERSON_SCALE_Y = 1.7f;
        private static readonly Random random = new Random(Guid.NewGuid().GetHashCode());
        private static readonly object syncLock = new object();
        private WriteableBitmap gridBitmap;
        private readonly DrawingGroup infrastructureBackingStore;
        private readonly DrawingImage infrastructureDrawingImageSource;
        private readonly DrawingGroup peopleBackingStore;
        private readonly DrawingImage peopleDrawingImageSource;
        private readonly Dictionary<ImageDrawing, PersonField> imageToPersonMapping;
        private TimeSpan prevFrameTime = TimeSpan.Zero;
        private DateTime prevClickTime = DateTime.Now;
        private readonly Dictionary<PersonField, AnimationState> movingPeople;

        #region Bindable properties / ItemSources

        // GridClick command property
        public DelegateCommand GridClick
        {
            get { return (DelegateCommand)GetValue(GridClickProperty); }
            set { SetValue(GridClickProperty, value); }
        }

        public static readonly DependencyProperty GridClickProperty =
            DependencyProperty.Register(nameof(GridClick), typeof(DelegateCommand), typeof(GameGridView));

        // GridHover command property
        public DelegateCommand GridHover
        {
            get { return (DelegateCommand)GetValue(GridHoverProperty); }
            set { SetValue(GridHoverProperty, value); }
        }

        // MouseLeave command property
        public DelegateCommand GridHoverEnd
        {
            get { return (DelegateCommand)GetValue(GridHoverEndProperty); }
            set { SetValue(GridHoverEndProperty, value); }
        }

        public static readonly DependencyProperty GridHoverEndProperty =
            DependencyProperty.Register(nameof(GridHoverEnd), typeof(DelegateCommand), typeof(GameGridView));

        // PixelPerUnit Property
        public Int32 PixelPerUnit
        {
            get { return (Int32)GetValue(PixelPerUnitProperty); }
            set { SetValue(PixelPerUnitProperty, value); }
        }

        public static readonly DependencyProperty PixelPerUnitProperty =
            DependencyProperty.Register(nameof(PixelPerUnit), typeof(Int32), typeof(GameGridView), new PropertyMetadata(100, PixelPerUnitPropertyChanged));

        private static void PixelPerUnitPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GameGridView)d).PixelPerUnitPropertyChanged((Int32)e.NewValue);
        }

        private void PixelPerUnitPropertyChanged(Int32 pixelPerUnit)
        {
            InvalidateVisual();
        }

        public static readonly DependencyProperty GridHoverProperty =
            DependencyProperty.Register(nameof(GridHover), typeof(DelegateCommand), typeof(GameGridView));

        // GridVisible property
        public Boolean GridVisible
        {
            get { return (Boolean)GetValue(GridVisibleProperty); }
            set { SetValue(GridVisibleProperty, value); }
        }

        public static readonly DependencyProperty GridVisibleProperty =
            DependencyProperty.Register(nameof(GridVisible), typeof(Boolean), typeof(GameGridView), new PropertyMetadata(true, GridVisiblePropertyChanged));

        private static void GridVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GameGridView)d).GridVisiblePropertyChanged((Boolean)e.NewValue);
        }

        private void GridVisiblePropertyChanged(Boolean isVisible)
        {
            this.GridVisible = isVisible;
            InvalidateVisual();
        }

        // Rows property
        public Int32 Rows
        {
            get { return (Int32)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register(nameof(Rows), typeof(Int32), typeof(GameGridView), new PropertyMetadata(0, RowsPropertyChanged));

        private static void RowsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GameGridView)d).RowsPropertyChanged((Int32)e.NewValue);
        }

        private void RowsPropertyChanged(Int32 rows)
        {
            InvalidateVisual();
            this.Rows = rows;
            this.Width = PixelPerUnit * Columns;
            this.Height = PixelPerUnit * Rows;
        }

        // Columns property
        public Int32 Columns
        {
            get { return (Int32)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(nameof(Columns), typeof(Int32), typeof(GameGridView), new PropertyMetadata(0, ColumnsPropertyChanged));

        private static void ColumnsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GameGridView)d).ColumnsPropertyChanged((Int32)e.NewValue);
        }

        private void ColumnsPropertyChanged(Int32 columns)
        {
            InvalidateVisual();
            this.Columns = columns;
            this.Width = PixelPerUnit * Columns;
            this.Height = PixelPerUnit * Rows;
        }

        // TimeInterval property
        public Int32 TimeInterval
        {
            get { return (Int32)GetValue(TimeIntervalProperty); }
            set { SetValue(TimeIntervalProperty, value); }
        }

        public static readonly DependencyProperty TimeIntervalProperty =
            DependencyProperty.Register(nameof(TimeInterval), typeof(Int32), typeof(GameGridView), new PropertyMetadata(1000));


        // InfrastructureElements ItemSources
        public IEnumerable InfrastructureElements
        {
            get { return (IEnumerable)GetValue(InfrastructureElementsProperty); }
            set { SetValue(InfrastructureElementsProperty, value); }
        }

        public static readonly DependencyProperty InfrastructureElementsProperty =
            DependencyProperty.Register(nameof(InfrastructureElements), typeof(IEnumerable), typeof(GameGridView), new PropertyMetadata(new PropertyChangedCallback(OnInfrastructureElementsPropertyChanged)));

        private static void OnInfrastructureElementsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            GameGridView control = sender as GameGridView;
            if (control != null)
                control.OnInfrastructureElementsChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
        }

        private void OnInfrastructureElementsChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            // remove handler for oldValue.CollectionChanged
            var oldValueINotifyCollectionChanged = oldValue as INotifyItemPropertyChanged;

            if (null != oldValueINotifyCollectionChanged)
            {
                oldValueINotifyCollectionChanged.CollectionChanged -= new NotifyCollectionChangedEventHandler(InfrastructureCollectionChanged);
                oldValueINotifyCollectionChanged.ItemPropertyChanged -= InfrastructureChildPropertyChanged;
            }
            // add handler for newValue.CollectionChanged (if possible)
            var newValueINotifyCollectionChanged = newValue as INotifyItemPropertyChanged;
            if (null != newValueINotifyCollectionChanged)
            {
                newValueINotifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(InfrastructureCollectionChanged);
                newValueINotifyCollectionChanged.ItemPropertyChanged += InfrastructureChildPropertyChanged;
            }

        }

        private void InfrastructureChildPropertyChanged(object sender, ItemPropertyChangedEventArgs e)
        {
            DrawInfrastructures();
        }

        // event handler for collection change
        private void InfrastructureCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DrawInfrastructures();
        }


        // PeopleElements ItemSources
        public IEnumerable PersonElements
        {
            get { return (IEnumerable)GetValue(PersonElementsProperty); }
            set { SetValue(PersonElementsProperty, value); }
        }

        public static readonly DependencyProperty PersonElementsProperty =
            DependencyProperty.Register(nameof(PersonElements), typeof(IEnumerable), typeof(GameGridView), new PropertyMetadata(new PropertyChangedCallback(OnPersonElementsPropertyChanged)));

        private static void OnPersonElementsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            GameGridView control = sender as GameGridView;
            if (control != null)
                control.OnPersonElementsPropertyChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
        }

        private void OnPersonElementsPropertyChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            // remove handler for oldValue.CollectionChanged
            var oldValueINotifyCollectionChanged = oldValue as INotifyItemPropertyChanged;

            if (null != oldValueINotifyCollectionChanged)
            {
                oldValueINotifyCollectionChanged.CollectionChanged -= new NotifyCollectionChangedEventHandler(PersonCollectionChanged);
                oldValueINotifyCollectionChanged.ItemPropertyChanged -= PersonChildPropertyChanged;
            }
            // add handler for newValue.CollectionChanged (if possible)
            var newValueINotifyCollectionChanged = newValue as INotifyItemPropertyChanged;
            if (null != newValueINotifyCollectionChanged)
            {
                newValueINotifyCollectionChanged.ItemPropertyChanged += PersonChildPropertyChanged;
                newValueINotifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(PersonCollectionChanged);
            }

        }

        private void PersonChildPropertyChanged(object sender, ItemPropertyChangedEventArgs e)
        {
            FullyObservableCollection<PersonField> coll = sender as FullyObservableCollection<PersonField>;

            if (e.PropertyName == nameof(PersonField.Coordinates))
            {
                PersonField element = coll[e.CollectionIndex];
                if (movingPeople.ContainsKey(element))
                {
                    AnimationState s = movingPeople[element];

                    Int32 x = element.Coordinates.X * PixelPerUnit, y = element.Coordinates.Y * PixelPerUnit;

                    Point nonOverlapping = GetNonOverlappingPositionOnInfrastructure(x, y);

                    s.StartX = s.CurrentX = s.TargetX;
                    s.StartY = s.CurrentY = s.TargetY;
                    s.TargetX = nonOverlapping.X;
                    s.TargetY = nonOverlapping.Y;
                    s.elapsed = 0;
                }
            }
        }

        // event handler for collection change
        private void PersonCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (PersonField pf in e.NewItems)
                {
                    double x = pf.Coordinates.X * PixelPerUnit;
                    double y = pf.Coordinates.Y * PixelPerUnit;
                    System.Windows.Point nonOverlapping = GetNonOverlappingPositionOnInfrastructure(x, y);
                    x = nonOverlapping.X;
                    y = nonOverlapping.Y;

                    movingPeople.Add(pf, new AnimationState
                    {
                        CurrentX = x,
                        CurrentY = y,
                        StartX = x,
                        StartY = y,
                        TargetX = x,
                        TargetY = y
                    });
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (PersonField pf in e.OldItems)
                {
                    movingPeople.Remove(pf);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                movingPeople.Clear();
            }

            // Force to re-renderer the UIElement again, to reflect the newly added visual elements
            InvalidateVisual();
        }

        #endregion

        public GameGridView()
        {
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.Linear);
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);


            this.Loaded += OnLoad;
            this.MouseMove += OnHover;
            this.MouseLeave += OnHoverEnd;
            this.MouseDown += OnMouseDownClick;
            this.MouseUp += OnMouseUpClick;

            CompositionTargetEx.Rendering += CompositionTarget_Rendering;

            movingPeople = new Dictionary<PersonField, AnimationState>();

            /* buffer for infrastructure elements */
            infrastructureBackingStore = new DrawingGroup();
            infrastructureDrawingImageSource = new DrawingImage(infrastructureBackingStore);

            /* buffer for person elements */
            peopleBackingStore = new DrawingGroup();
            imageToPersonMapping = new Dictionary<ImageDrawing, PersonField>();
            peopleDrawingImageSource = new DrawingImage(peopleBackingStore);
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            RenderingEventArgs args = (RenderingEventArgs)e;
            double elapsed = (args.RenderingTime - prevFrameTime).TotalMilliseconds;
            prevFrameTime = args.RenderingTime;
            Render(elapsed);
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            InvalidateVisual();
            this.Width = PixelPerUnit * Columns;
            this.Height = PixelPerUnit * Rows;

            // create grid bitmap, and freeze it, to increase performance.
            gridBitmap = DrawGrid();
            gridBitmap.Freeze();

            // draw infrastructures
            DrawInfrastructures();
        }

        private void OnMouseDownClick(object sender, MouseButtonEventArgs e)
        {
            prevClickTime = DateTime.Now;
        }

        private void OnMouseUpClick(object sender, MouseButtonEventArgs e)
        {
            // To prevent click when dragging
            if (DateTime.Now.Subtract(prevClickTime).Milliseconds < 180)
            {
                double x = e.GetPosition(this).X;
                double y = e.GetPosition(this).Y;

                if (0 <= x && x <= this.Width && 0 <= y && y <= this.Height)
                {
                    if (GridClick != null)
                    {
                        Utilities.Point coords = new Utilities.Point((Int32)x / PixelPerUnit, (Int32)y / PixelPerUnit);
                        PersonField p = GetPersonAtPosition(x, y);
                        if (p != null)
                        {
                            GridClick.Execute(new Tuple<Utilities.Point, Object>(coords, p));
                        }
                        else
                        {
                            InfrastructureField i = GetInfrastructureFieldAtPosition(x, y);
                            GridClick.Execute(new Tuple<Utilities.Point, Object>(coords, i));
                        }
                    }
                }
            }
        }

        private void OnHover(object sender, MouseEventArgs e)
        {
            double x = e.GetPosition(this).X;
            double y = e.GetPosition(this).Y;

            if (0 <= x && x <= this.Width && 0 <= y && y <= this.Height)
            {
                if (GridHover != null)
                {
                    Utilities.Point coords = new Utilities.Point((Int32)x / PixelPerUnit, (Int32)y / PixelPerUnit);
                    PersonField p = GetPersonAtPosition(x, y);
                    if (p != null)
                    {
                        GridHover.Execute(new Tuple<Utilities.Point, Object>(coords, p));
                    }
                    else
                    {
                        InfrastructureField i = GetInfrastructureFieldAtPosition(x, y);
                        GridHover.Execute(new Tuple<Utilities.Point, Object>(coords, i));
                    }
                }
            }
        }

        private void OnHoverEnd(object sender, MouseEventArgs e)
        {
            if (GridHoverEnd != null)
            {
                GridHoverEnd.Execute(null);
            }
        }

        private WriteableBitmap DrawGrid()
        {
            WriteableBitmap writeableBmp = BitmapFactory.New(Rows * PixelPerUnit, Columns * PixelPerUnit);
            using (writeableBmp.GetBitmapContext())
            {
                writeableBmp.Clear();

                for (int i = 0; i < Rows + 1; i++)
                {
                    writeableBmp.DrawLineAa(0, i * PixelPerUnit, Columns * PixelPerUnit, i * PixelPerUnit, Colors.Black, 6);
                }

                for (int i = 0; i < Columns + 1; i++)
                {
                    writeableBmp.DrawLineAa(i * PixelPerUnit, 0, i * PixelPerUnit, Rows * PixelPerUnit, Colors.Black, 6);
                }

                return writeableBmp;
            }
        }

        private InfrastructureField GetInfrastructureFieldAtPosition(double x, double y)
        {
            foreach (Object obj in ((IEnumerable<InfrastructureField>)InfrastructureElements).Reverse())
            {
                if (!(obj is InfrastructureField))
                {
                    throw new InvalidDataException($"{nameof(InfrastructureElements)} should only contain {nameof(InfrastructureField)} elements.");
                }

                InfrastructureField inf = obj as InfrastructureField;
                Utilities.Point infSize = inf.Infrastructure.Size;

                Rect rect = new Rect(inf.Coordinates.X * PixelPerUnit, inf.Coordinates.Y * PixelPerUnit, infSize.X * PixelPerUnit, infSize.Y * PixelPerUnit);
                if (rect.Contains(x + 1, y + 1))
                {
                    return inf;
                }
            }

            return null;
        }

        private PersonField GetPersonAtPosition(double x, double y)
        {
            foreach (ImageDrawing p in peopleBackingStore.Children)
            {
                if (p.Bounds.Contains(x, y))
                {
                    return imageToPersonMapping[p];
                }
            }

            return null;
        }

        private Point GetNonOverlappingPositionOnInfrastructure(double x, double y)
        {
            Int32 tryCount = 0;

            InfrastructureField i = GetInfrastructureFieldAtPosition(x, y);

            double useableX = 1, useableY = 1;

            Rect? infBounds = null;
            if (i != null && i.Infrastructure.UseableArea != null)
            {
                double minX = i.Infrastructure.UseableArea.Min(p => p.X);
                double maxX = i.Infrastructure.UseableArea.Max(p => p.X);
                double minY = i.Infrastructure.UseableArea.Min(p => p.Y);
                double maxY = i.Infrastructure.UseableArea.Max(p => p.Y);

                useableX = maxX - minX + 1;
                useableY = maxY - minY + 1;

                infBounds = new Rect(i.Coordinates.X * PixelPerUnit,
                                     i.Coordinates.Y * PixelPerUnit,
                                     i.Infrastructure.Size.X * PixelPerUnit,
                                     i.Infrastructure.Size.Y * PixelPerUnit);
            }

            double offsetX = 0, offsetY = 0;
            do
            {
                offsetX = PixelPerUnit * GetRandomNumber(-0.2 * useableX, 0.2 * useableX);
                offsetY = PixelPerUnit * GetRandomNumber(-0.2 * useableY, 0.2 * useableY);

                tryCount++;
            } while (GetPersonAtPosition(x + offsetX, y + offsetY) != null && tryCount < 100);


            Point res = new Point(x, y);
            res.Offset(offsetX, offsetY);

            if (infBounds != null)
            {
                Rect infDims = (Rect)(infBounds);
                if (res.X < infDims.X)
                    res.X = infDims.X;

                else if (res.X > infDims.X + infDims.Width)
                    res.X = infDims.X;

                if (res.Y < infDims.Y)
                    res.Y = infDims.Y;

                else if (res.Y > infDims.Y + infDims.Height)
                    res.Y = infDims.Y;
            }

            return res;
        }


        public static double GetRandomNumber(double minimum, double maximum)
        {
            lock (syncLock)
            {
                return random.NextDouble() * (maximum - minimum) + minimum;
            }
        }

        private void DrawInfrastructures()
        {
            infrastructureBackingStore.Children.Clear();

            foreach (Object obj in InfrastructureElements)
            {
                if (!(obj is InfrastructureField))
                {
                    throw new InvalidDataException($"{nameof(InfrastructureElements)} should only contain {nameof(InfrastructureField)} elements.");
                }

                InfrastructureField inf = obj as InfrastructureField;
                Utilities.Point infSize = inf.Infrastructure.Size;

                ImageDrawing imageDrawing = new ImageDrawing
                {
                    Rect = new Rect(inf.Coordinates.X * PixelPerUnit, inf.Coordinates.Y * PixelPerUnit, infSize.X * PixelPerUnit, infSize.Y * PixelPerUnit),
                    ImageSource = inf.Image
                };

                infrastructureBackingStore.Children.Add(imageDrawing);
            }
        }

        private void DrawPeople(Double elapsedMillis)
        {
            if (PersonElements == null)
                return;

            peopleBackingStore.Children.Clear();
            imageToPersonMapping.Clear();

            foreach (KeyValuePair<PersonField, AnimationState> kvp in movingPeople)
            {
                PersonField person = kvp.Key;
                AnimationState anim = kvp.Value;

                if (person.IsVisible) {
                    bool finishedXAnim = (anim.StartX >= anim.TargetX && anim.CurrentX < anim.TargetX) ||
                     (anim.StartX < anim.TargetX && anim.CurrentX >= anim.TargetX);
                    bool finishedYAnim = (anim.StartY <= anim.TargetY && anim.CurrentY >= anim.TargetY) ||
                                         (anim.StartY > anim.TargetY && anim.CurrentY <= anim.TargetY);

                    if (!(finishedXAnim && finishedYAnim))
                    {
                        if (!finishedXAnim)
                            anim.CurrentX += (anim.TargetX - anim.CurrentX) / (TimeInterval - anim.elapsed) * elapsedMillis;
                        if (!finishedYAnim)
                            anim.CurrentY += (anim.TargetY - anim.CurrentY) / (TimeInterval - anim.elapsed) * elapsedMillis;

                        anim.elapsed += elapsedMillis;
                    }

                    double width = PERSON_SCALE_X * PixelPerUnit;
                    double height = PERSON_SCALE_Y * PixelPerUnit;
                    double x = anim.CurrentX;
                    double y = anim.CurrentY;

                    y -= height / PERSON_SCALE_Y;
                    x += width / 2;

                    ImageDrawing imageDrawing = new ImageDrawing
                    {
                        Rect = new Rect(x, y, width, height),
                        ImageSource = person.Image
                    };

                    peopleBackingStore.Children.Add(imageDrawing);
                    imageToPersonMapping.Add(imageDrawing, person);
                }
            }
            InvalidateVisual();
        }

        /// <summary>
        /// Draws the lines and squares, found in the VisualElements collection.
        /// </summary>
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            /* DRAW GRID */
            if (GridVisible)
                dc.DrawImage(gridBitmap, new Rect(0, 0, Rows * PixelPerUnit, Columns * PixelPerUnit));

            /* DRAW INFRASTRUCTURE BUFFER CONTENT */
            if (infrastructureBackingStore.Children.Count > 0)
            {
                var minX = infrastructureBackingStore.Children.Min(c => (c as ImageDrawing).Rect.X);
                var minY = infrastructureBackingStore.Children.Min(c => (c as ImageDrawing).Rect.Y);
                dc.DrawImage(infrastructureDrawingImageSource, new Rect(minX, minY, infrastructureDrawingImageSource.Width, infrastructureDrawingImageSource.Height));
            }

            /* DRAW PERSON BUFFER CONTENT */
            if (peopleBackingStore.Children.Count > 0)
            {
                var minX = peopleBackingStore.Children.Min(c => (c as ImageDrawing).Rect.X);
                var minY = peopleBackingStore.Children.Min(c => (c as ImageDrawing).Rect.Y);
                dc.DrawImage(peopleDrawingImageSource, new Rect(minX, minY, peopleDrawingImageSource.Width, peopleDrawingImageSource.Height));
            }
        }

        private void Render(Double elapsedMillis)
        {
            DrawPeople(elapsedMillis);
        }
    }
}
