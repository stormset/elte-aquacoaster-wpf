using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AquaCoaster.Model;
using AquaCoaster.Model.Entities;
using AquaCoaster.Model.Enums;
using AquaCoaster.Utilities;
using Point = AquaCoaster.Utilities.Point;


namespace AquaCoaster.ViewModel
{
    public class GameViewModel : ViewModelBase
    {
        public struct PopupMessage
        {
            public String Title { get; set; }
            public String Message { get; set; }
            public WPFUI.Common.Icon Icon { get; set; }
        }

        #region Fields

        private static Uri ENTITY_RESOURCE_DICT_PATH = new Uri("pack://application:,,,/AquaCoaster;component/Resources/EntityResources.xaml");
        private static Uri IMAGE_RESOURCE_DICT_PATH = new Uri("pack://application:,,,/AquaCoaster;component/Resources/ImageResources.xaml");
        private GameModel _model;
        private ResourceDictionary _entityResourceDict;
        private ResourceDictionary _imageResourceDict;

        #endregion

        #region Properties

        public DelegateCommand EscapePressedCommand { get; private set; }

        public DelegateCommand NewGameCommand { get; private set; }

        public DelegateCommand ExitGameCommand { get; private set; }

        public DelegateCommand ToggleHelpCommand { get; private set; }

        public DelegateCommand GridClickCommand { get; private set; }

        public DelegateCommand GridHoverCommand { get; private set; }

        public DelegateCommand GridHoverEndCommand { get; private set; }

        public DelegateCommand OpenParkCommand { get; private set; }

        public DelegateCommand CloseParkCommand { get; private set; }

        public DelegateCommand BuildModeCommand { get; private set; }

        public DelegateCommand DemolishModeCommand { get; private set; }

        public DelegateCommand ExitInfoPanelCommand { get; private set; }

        public DelegateCommand OKInfoPanelCommand { get; private set; }

        public DelegateCommand MoneyChanged { get; private set; }

        public DelegateCommand SlowDownCommand { get; private set; }

        public DelegateCommand SpeedUpCommand { get; private set; }

        public DelegateCommand NameParkCommand { get; private set; }

        public FullyObservableCollection<PlaceableCategoryField> PlaceableCategoryFields { get; set; }

        public FullyObservableCollection<PersonField> PersonFields { get; set; }

        public FullyObservableCollection<InfrastructureField> InfrastructureFields { get; set; }

        public GameMode GameMode { get; set; } = GameMode.BUILD;

        public Int32 Rows { get => _model.Rows; }

        public Int32 Columns { get => _model.Columns; }

        public Int32 Budget { get => _model.Budget; }

        public String GameTime { get { return TimeSpan.FromMilliseconds(_model.GameTime).ToString(@"hh\:mm\:ss"); } }

        public Int32 MechanicCount { get => _model.MechanicCount; set => _model.MechanicCount = value; }

        public Int32 VisitorCount { get => _model.VisitorCount; }

        public Int32 TimeInterval { get => _model.TimeInterval; set => _model.TimeInterval = value; }

        public ParkStatus ParkStatus { get => _model.ParkStatus; }

        public Boolean IsParkOpen { get => ParkStatus == ParkStatus.OPEN; }

        public String Name { get => _model.Name; set => _model.Name = value; }

        private Boolean _triggerPopup;
        public Boolean TriggerPopup
        {
            get => _triggerPopup;
            private set
            {
                _triggerPopup = value;
                OnPropertyChanged();
            }
        }

        public PopupMessage _popupMessage;
        public PopupMessage PopupData
        {
            get => _popupMessage;
            private set
            {
                _popupMessage = value;
                OnPropertyChanged();
            }
        }

        public Int32 MaxUseFee { get => GameModel.MAX_USE_FEE; }

        public Int32 MaxEntryFee { get => GameModel.MAX_ENTRY_FEE; }

        // selected game to place
        private SelectionInfo _currentlySelected = new SelectionInfo();
        public SelectionInfo CurrentlySelected
        {
            get => _currentlySelected;
            private set
            {
                _currentlySelected = value;
                OnPropertyChanged();
            }
        }

        // hovered field
        private SelectionInfo _currentlyHovered = new SelectionInfo();
        public SelectionInfo CurrentlyHovered
        {
            get => _currentlyHovered;
            private set
            {
                _currentlyHovered = value;
                OnPropertyChanged();
            }
        }

        // clicked infrastructure to show info
        public SelectedInfrastructure ClickedInfrastructure { get; set; } = new SelectedInfrastructure();

        // clicked person to show info
        public SelectedPerson ClickedPerson { get; set; } = new SelectedPerson();

        // cursor type on canvas
        Cursor _cursor;
        public Cursor Cursor
        {
            get => _cursor;
            private set
            {
                if (_cursor != value)
                {
                    _cursor = value;
                    OnPropertyChanged();
                }
            }
        }

        private Boolean _demolishModeActive;
        public Boolean DemolishModeActive
        {
            get => _demolishModeActive;
            private set
            {
                if (_demolishModeActive != value)
                {
                    if (value && ParkStatus == ParkStatus.OPEN)
                    {
                        DisplayPopup("Note!", "Close park before starting to demolish.");
                    }
                    else if (value && VisitorCount > 0)
                    {
                        DisplayPopup("Be cautious!", "Wait until all visitors leave.");
                    }
                    else
                    {
                        _demolishModeActive = value;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private Visibility _menuOpen = Visibility.Visible;
        public Visibility MenuOpen
        {
            get => _menuOpen;
            private set
            {
                if (_menuOpen != value)
                {
                    _menuOpen = value;
                    OnPropertyChanged();
                }
            }
        }

        private Visibility _helpOpen = Visibility.Collapsed;
        public Visibility HelpOpen
        {
            get => _helpOpen;
            private set
            {
                if (_helpOpen != value)
                {
                    _helpOpen = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler NewGame;

        public event EventHandler ExitGame;

        #endregion

        #region Constructor
        public GameViewModel(GameModel model)
        {
            // getting resources
            _entityResourceDict = new ResourceDictionary();
            _imageResourceDict = new ResourceDictionary();
            _entityResourceDict.Source = ENTITY_RESOURCE_DICT_PATH;
            _imageResourceDict.Source = IMAGE_RESOURCE_DICT_PATH;

            DemolishModeActive = false;

            // attaching model event handlers
            _model = model;
            _model.GameCreated += Model_GameCreated;
            _model.ParkStatusChanged += Model_ParkStatusChanged;
            _model.BudgetChanged += Model_BudgetChanged;
            _model.GameTimeChanged += Model_GameTimeChanged;
            _model.TimeIntervalChanged += Model_TimeIntervalChanged;
            _model.PersonAdded += Model_PersonAdded;
            _model.PersonRemoved += Model_PersonRemoved;
            _model.PersonMoved += Model_PersonMoved;
            _model.PersonChanged += Model_PersonChanged;
            _model.InfrastructureAdded += Model_InfrastructureAdded;
            _model.InfrastructuresAdded += Model_InfrastructuresAdded;
            _model.InfrastructureRemoved += Model_InfrastructureRemoved;
            _model.InfrastructureChanged += Model_InfrastructureChanged;

            // craeating the commands
            EscapePressedCommand = new DelegateCommand(_ => OnEscapePressed());
            NewGameCommand = new DelegateCommand(_ => OnNewGame());
            ExitGameCommand = new DelegateCommand(_ => OnExitGame());
            ToggleHelpCommand = new DelegateCommand(_ => HelpOpen = (HelpOpen == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible));
            DemolishModeCommand = new DelegateCommand(_ => OnDemolishButtonClicked());
            ExitInfoPanelCommand = new DelegateCommand(_ => OnExitInfoPanel());
            OKInfoPanelCommand = new DelegateCommand(_ => OnOKInfoPanel());
            GridClickCommand = new DelegateCommand(args => OnGridClicked(((Tuple<Point, Object>)args).Item1,
                                                                         ((Tuple<Point, Object>)args).Item2));
            GridHoverCommand = new DelegateCommand(args => OnGridHovered(((Tuple<Point, Object>)args).Item1,
                                                                         ((Tuple<Point, Object>)args).Item2));
            GridHoverEndCommand = new DelegateCommand(_ => OnGridHoverEnded());
            OpenParkCommand = new DelegateCommand(_ => OnParkOpen());
            NameParkCommand = new DelegateCommand(_ => OnNamePark());

            // instantiating the ObservableCollection's
            PlaceableCategoryFields = new FullyObservableCollection<PlaceableCategoryField>();
            InfrastructureFields = new FullyObservableCollection<InfrastructureField>();
            PersonFields = new FullyObservableCollection<PersonField>();

            PopulateTable();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Update table based on model data.
        /// </summary>
        private void PopulateTable()
        {
            PlaceableCategoryFields.Clear();
            InfrastructureFields.Clear();
            PersonFields.Clear();

            Dictionary<String, PlaceableCategoryField> categoryGameDict = new Dictionary<string, PlaceableCategoryField>();
            IEnumerable<Resources.ResourceModel> sorted = _entityResourceDict.Values.Cast<Resources.ResourceModel>().Where(s => s.Visible).OrderBy(s => s.Order);

            Dictionary<String, Tuple<String, Boolean, String, Func<Object, String>>> detailedInfoProps = new Dictionary<String, Tuple<String, Boolean, String, Func<Object, String>>> {
                        // Key                                                                        Display Name        zero treatment     null value treatment    converter
                        { "UseFee",          new Tuple<String, Boolean, String, Func<Object, String>>("Use fee",          false,             null,                   null           ) },
                        { "Capacity",        new Tuple<String, Boolean, String, Func<Object, String>>("Capacity",         false,             "no limit",             null           ) },
                     // { "MinimumCapacity", new Tuple<String, Boolean, String, Func<Object, String>>("Minimum capacity", false,             "no limit",             null           ) },
                        { "MoodFactor",      new Tuple<String, Boolean, String, Func<Object, String>>("Mood factor",      true,              null,                   floatToPercent ) },
                        { "FoodFactor",      new Tuple<String, Boolean, String, Func<Object, String>>("Food factor",      true,              null,                   floatToPercent ) }
                    };

            foreach (Resources.ResourceModel entityInfo in sorted)
            {
                Dictionary<String, object> config = Helpers.GetPropertyValueFromType<Dictionary<String, object>>(entityInfo.TypeReference, "Config");

                if (typeof(IPlaceable).IsAssignableFrom(entityInfo.TypeReference) && entityInfo.CategoryName != null) // the item is placeable
                {
                    String detailedInfo = "";
                    foreach (KeyValuePair<string, Tuple<String, Boolean, String, Func<Object, String>>> entry in detailedInfoProps)
                    {
                        if (config.ContainsKey(entry.Key))
                        {
                            String str = PrettyPrintCardDetail(entry.Value.Item1, config[entry.Key], entry.Value.Item2, entry.Value.Item3, entry.Value.Item4);
                            detailedInfo += str;

                            if (!String.IsNullOrEmpty(str))
                            {
                                detailedInfo += Environment.NewLine;
                            }
                        }
                    }

                    PlaceableField placeableField = new PlaceableField
                    {
                        ElementType = entityInfo.TypeReference,
                        Name = (String)config["Name"],
                        Price = (Int32)config["Price"],
                        Size = config.ContainsKey("Size") ? ((Utilities.Point)config["Size"]).ToString().Replace(",", " x ") : "",
                        DetailedInfo = detailedInfo,
                        Image = entityInfo.Image,
                        ClickCommand = new DelegateCommand(param => PlaceableSelected(entityInfo.TypeReference))
                    };

                    if (!categoryGameDict.ContainsKey(entityInfo.CategoryName))
                    {
                        ObservableCollection<PlaceableField> gamesOfCategory = new ObservableCollection<PlaceableField>();

                        gamesOfCategory.Add(placeableField);

                        categoryGameDict[entityInfo.CategoryName] = new PlaceableCategoryField
                        {
                            CategoryName = entityInfo.CategoryName,
                            CategoryIcon = WPFUI.Common.Icon.RectangleLandscape24,
                            PlaceableFields = gamesOfCategory
                        };
                    }
                    else
                    {
                        categoryGameDict[entityInfo.CategoryName].PlaceableFields.Add(placeableField);
                    }
                }
            }

            PlaceableCategoryFields.AddAll(categoryGameDict.Select(s => s.Value));
        }

        private String PrettyPrintCardDetail(String name, Object value, Boolean treatZeroAsEmpty = false, String nullReplaceValue = null, Func<Object, String> converter = null)
        {
            Int32 padToWidth = 32;
            if ((value == null && nullReplaceValue == null) || (treatZeroAsEmpty && (Single)value == 0))
            {
                return "";
            }

            else if (value == null && nullReplaceValue != null)
            {
                value = converter != null ? converter(nullReplaceValue) : nullReplaceValue;
                return $"{name}: {value}";
            }

            String val = (String)(converter != null ? converter(value).ToString() : value.ToString());
            name = String.Format($"{{0,-{padToWidth - (name.Length + val.Length)}}}", name + ':');

            return $"{name} {val}";
        }

        private String floatToPercent(Object val)
        {
            return String.Format("{0:0%}", val);
        }

        private void PlaceableSelected(Type type)
        {
            Dictionary<String, object> config = Helpers.GetPropertyValueFromType<Dictionary<String, object>>(type, "Config");

            if (CurrentlySelected.SelectedType == type)
            {
                UnselectPlaceables();
                return;
            }

            CurrentlySelected = new SelectionInfo
            {
                AnySelected = true,
                SelectedType = type,
                SelectedName = (String)config["Name"],
                SelectedSize = (Utilities.Point)config["Size"],
                SelectedImage = ((Resources.ResourceModel)_entityResourceDict[type.Name]).Image
            };

            foreach (PlaceableCategoryField field in PlaceableCategoryFields)
            {
                foreach (PlaceableField placeable in field.PlaceableFields)
                {
                    placeable.Selected = false;

                    if (type == placeable.ElementType)
                    {
                        placeable.Selected = true;
                    }

                }
            }
        }

        private void UnselectPlaceables()
        {
            CurrentlySelected.AnySelected = false;
            CurrentlySelected.SelectedType = null;
            CurrentlySelected.SelectionActive = false;

            foreach (PlaceableCategoryField field in PlaceableCategoryFields)
            {
                foreach (PlaceableField placeable in field.PlaceableFields)
                {
                    placeable.Selected = false;
                }
            }
        }

        private void DisplayPopup(String title, String message, WPFUI.Common.Icon icon = WPFUI.Common.Icon.Warning24)
        {
            TriggerPopup = !TriggerPopup;
            PopupData = new PopupMessage
            {
                Title = title,
                Message = message,
                Icon = icon
            };
        }

        private void CloseInfoPanels()
        {
            ClickedInfrastructure.Infrastructure = null;
            ClickedPerson.Person = null;
        }

        #endregion

        #region Game event handlers

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Model_GameCreated(Object sender, EventArgs args)
        {
            // TODO: Implementation
            PopulateTable(); // populate table with fields

            OnPropertyChanged(nameof(Budget));
            OnPropertyChanged(nameof(TimeInterval));
            OnPropertyChanged(nameof(ParkStatus));
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="newParkStatus"></param>
        private void Model_ParkStatusChanged(Object sender, ParkStatus newParkStatus)
        {
            for (int i = 0; i < InfrastructureFields.Count; i++)
            {
                if (InfrastructureFields[i].Infrastructure is Gate g)
                {
                    String resKey = InfrastructureFields[i].Infrastructure.GetType().Name + (ParkStatus == ParkStatus.OPEN ? "_OPEN" : "");
                    InfrastructureFields[i].Image = ((Resources.ResourceModel)_entityResourceDict[resKey]).Image;
                }
            }

            OnPropertyChanged(nameof(ParkStatus));
            OnPropertyChanged(nameof(IsParkOpen));
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="newTime"></param>
        private void Model_GameTimeChanged(Object sender, Int32 newTime)
        {
            OnPropertyChanged(nameof(GameTime));
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="newSpeedFactor"></param>
        private void Model_TimeIntervalChanged(Object sender, Int32 newTimeInterval)
        {
            OnPropertyChanged(nameof(TimeInterval));
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="newBudget"></param>
        private void Model_BudgetChanged(Object sender, Int32 newBudget)
        {
            OnPropertyChanged(nameof(Budget));
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Model_PersonAdded(Object sender, PersonAddRemoveEventArgs args)
        {
            ImageSource img = args.Person is Visitor ? (ImageSource)_imageResourceDict["Woman"] :   // visitor image
                              args.Person is Mechanic ? (ImageSource)_imageResourceDict["Mechanic"] :  // mechanic image
                              (ImageSource)_imageResourceDict["Woman"];  // default image

            // TODO: Might add ClickCommand
            PersonFields.Add(new PersonField
            {
                Person = args.Person,
                Coordinates = (Point)args.Coords,
                Image = img
            });
            OnPropertyChanged(nameof(VisitorCount));
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Model_PersonRemoved(Object sender, PersonAddRemoveEventArgs args)
        {
            for (int i = 0; i < PersonFields.Count; i++)
            {
                if (PersonFields[i].Person.Equals(args.Person))
                {
                    PersonFields.RemoveAt(i);
                }
            }

            if (ClickedPerson.Person == args.Person)
            {
                ClickedPerson.Person = null;
                OnPropertyChanged(nameof(ClickedPerson));
            }

            OnPropertyChanged(nameof(VisitorCount));
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="person"></param>
        private void Model_PersonChanged(Object sender, Person person)
        {
            if (ClickedPerson.Person == person)
            {
                ClickedPerson.Person = person;
                OnPropertyChanged(nameof(ClickedPerson));
            }
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Model_PersonMoved(Object sender, PersonMovedEventArgs args)
        {
            PersonField pf = PersonFields.First(p => p.Person.Equals(args.Person));
            pf.Coordinates = args.To;
            OnPropertyChanged(nameof(ClickedPerson));
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Model_InfrastructureAdded(Object sender, InfrastructureAddRemoveEventArgs args)
        {
            InfrastructureFields.Add(new InfrastructureField
            {
                Infrastructure = args.Infrastructure,
                Coordinates = args.Coords,
                Image = ((Resources.ResourceModel)_entityResourceDict[args.Infrastructure.GetType().Name]).Image
            });

            OnPropertyChanged(nameof(Budget));
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Model_InfrastructuresAdded(Object sender, InfrastructuresAddRemoveEventArgs args)
        {
            List<InfrastructureField> newFields = new List<InfrastructureField>();
            foreach (var inf in args.AddedInfrastructures)
            {
                newFields.Add(new InfrastructureField
                {
                    Infrastructure = inf.Item1,
                    Coordinates = inf.Item2,
                    Image = ((Resources.ResourceModel)_entityResourceDict[inf.Item1.GetType().Name]).Image
                });
            }
            InfrastructureFields.AddAll(newFields);
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Model_InfrastructureRemoved(Object sender, InfrastructureAddRemoveEventArgs args)
        {
            for (int i = 0; i < InfrastructureFields.Count; i++)
            {
                if (InfrastructureFields[i].Infrastructure.GetType().Equals(args.Infrastructure.GetType()) &&
                    InfrastructureFields[i].Coordinates.Equals(args.Coords)
)
                {
                    InfrastructureFields.RemoveAt(i);
                    break;
                }
            }

            if (ClickedInfrastructure.Infrastructure == args.Infrastructure)
            {
                ClickedInfrastructure.Infrastructure = null;
            }
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="infrastructure"></param>
        private void Model_InfrastructureChanged(Object sender, Infrastructure infrastructure)
        {
            if (ClickedInfrastructure.Infrastructure == infrastructure)
            {
                ClickedInfrastructure.Infrastructure = infrastructure;
            }
        }

        #endregion

        #region Private event methods

        private void OnGridClicked(Point coords, Object obj)
        {
            if (DemolishModeActive)
            {
                if (_model.CanDemolish(coords))
                {
                    _model.Demolish(coords);

                    InfrastructureField f = new InfrastructureField
                    {
                        Infrastructure = _model.GetInfrastructureAt(coords),
                        Coordinates = coords
                    };
                    OnGridHovered(coords, f);
                }
                else
                {
                    DisplayPopup("Warning!", "The clicked element can't be demolished.");
                }

            }
            else
            {
                if (CurrentlySelected.SelectionActive)
                {
                    Infrastructure toPlace = (Infrastructure)Activator.CreateInstance(CurrentlySelected.SelectedType);

                    if (_model.CanPlace(toPlace, coords))
                    {
                        _model.Place(toPlace, new Point(coords.X, coords.Y));
                        CurrentlySelected.IsValid = false;
                    }
                    else
                    {
                        DisplayPopup("Warning!", $"Placement of '{CurrentlySelected.SelectedName}' is not allowed to that field.");
                    }
                }
                else
                {
                    // Display info of related entities
                    PersonField pf = obj as PersonField;
                    InfrastructureField inf = obj as InfrastructureField;

                    if (pf != null)
                    {
                        ClickedInfrastructure.Infrastructure = null;
                        ClickedPerson.Person = pf.Person;
                    }
                    else if (inf != null && (inf.Infrastructure is Facility || inf.Infrastructure is Gate))
                    {
                        ClickedPerson.Person = null;
                        ClickedInfrastructure.Infrastructure = inf.Infrastructure;
                    }
                }
            }
        }

        private void OnGridHovered(Point coords, Object obj)
        {
            Cursor = Cursors.Arrow;

            if (obj is InfrastructureField)
            {
                InfrastructureField f = (InfrastructureField)obj;
                Cursor = (f.Infrastructure is Facility || f.Infrastructure is Gate) ? Cursors.Hand : Cursors.Arrow;

                if (f != null)
                {
                    CurrentlyHovered.SelectedInfrastructure = f.Infrastructure;

                    if (f.Infrastructure != null)
                    {
                        CurrentlyHovered.SelectedCoords = f.Coordinates;
                        CurrentlyHovered.SelectedSize = f.Infrastructure.Size;
                    }
                }

                if (!DemolishModeActive)
                {
                    CurrentlySelected.SelectionActive = CurrentlySelected.SelectedType != null;
                    if (CurrentlySelected.SelectionActive)
                    {
                        Cursor = Cursors.Hand;
                        CurrentlySelected.SelectedCoords = coords;
                        CurrentlySelected.IsValid = _model.CanPlaceInfrastructureWithType(CurrentlySelected.SelectedType, CurrentlySelected.SelectedSize, CurrentlySelected.SelectedCoords);
                    }
                }
                else
                {
                    if (f != null && f.Infrastructure != null)
                    {
                        CurrentlyHovered.SelectionActive = true;
                        CurrentlyHovered.IsValid = _model.CanDemolish(f.Coordinates);
                    }
                }
            }
            else if (obj is PersonField)
            {
                Cursor = Cursors.Hand;
            }
        }

        private void OnGridHoverEnded()
        {
            // TODO: Implementation
            // might be usefult to show that the element can be placed to the hovered field or
            // to show a popup about the building.
            CurrentlySelected.SelectionActive = false;
            CurrentlyHovered.SelectionActive = false;
            CurrentlyHovered.HoverActive = false;
        }

        private void OnDemolishButtonClicked()
        {
            DemolishModeActive = !DemolishModeActive;
            UnselectPlaceables();
        }

        private void OnEscapePressed()
        {
            if (!CurrentlySelected.SelectionActive && !DemolishModeActive)
            {
                CloseInfoPanels();
            }

            UnselectPlaceables();
            CurrentlyHovered.SelectionActive = false;
            DemolishModeActive = false;
        }

        public void OnNewGame()
        {
            MenuOpen = Visibility.Visible;
            UnselectPlaceables();
            CloseInfoPanels();
            DemolishModeActive = false;

            OnPropertyChanged(nameof(ParkStatus));
            OnPropertyChanged(nameof(IsParkOpen));
        }

        public void OnExitGame()
        {
            ExitGame?.Invoke(this, EventArgs.Empty);
        }

        public void OnExitInfoPanel()
        {
            ClickedInfrastructure.Infrastructure = ClickedInfrastructure.Infrastructure;
            ClickedInfrastructure.Infrastructure = null;
            ClickedPerson.Person = null;
            OnPropertyChanged(nameof(ClickedInfrastructure));
            OnPropertyChanged(nameof(ClickedPerson));
        }

        public void OnOKInfoPanel()
        {
            ClickedInfrastructure.Infrastructure = ClickedInfrastructure.Infrastructure;
            OnPropertyChanged(nameof(ClickedInfrastructure));
        }

        public void OnParkOpen()
        {
            if (ParkStatus == ParkStatus.CLOSED)
            {
                _model.OpenPark();
            }
            else
            {
                _model.ClosePark();
            }
        }

        public void OnNamePark()
        {
            MenuOpen = Visibility.Collapsed;
            NewGame?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged(nameof(Name));
        }

        #endregion
    }
}
