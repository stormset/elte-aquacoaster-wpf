using AquaCoaster.Model.Entities;
using AquaCoaster.Model.Enums;
using AquaCoaster.Persistence;
using AquaCoaster.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;


namespace AquaCoaster.Model
{
    public class GameModel
    {

        #region Fields

        public const Int32 MAX_USE_FEE = 20000;
        public const Int32 MAX_ENTRY_FEE = 60000;
        public static Int32 DEFAULT_ROWS = 26;
        public static Int32 DEFAULT_COLUMNS = 26;

        private const Int32 DEFAULT_BUDGET = 100000;
        private const Int32 DEFAULT_TIME_INTERVAL = 1000;
        private const Int32 DEFAULT_SPAWN_INTERVAL = 10 * 1000; // in milliseconds
        private const Int32 DEFAULT_GAME_TIME = 0; // start time

        private static Point GATE_COORDS = new Point(10, 24);

        private readonly Dictionary<Person, Tuple<Queue<Point>, Action>> _movementStates = new Dictionary<Person, Tuple<Queue<Point>, Action>>();
        private readonly Queue<Tuple<Int32, Action>> _callbackQueue = new Queue<Tuple<Int32, Action>>(); // callbacks to call in AdvanceTime after n call of AdvanceTime (n is the first item of the tuple)
        private Random random = new Random(Guid.NewGuid().GetHashCode());
        private Int32 SpawnIn; // how long till the next spawn

        #endregion

        #region Properties

        private GameData GameData { get; set; }

        public Int32 Rows { get => GameData.Rows; }

        public Int32 Columns { get => GameData.Columns; }

        public Int32 MechanicCount { get; set; }

        public ParkStatus ParkStatus
        {
            get => GameData.ParkStatus;
            set
            {
                if (GameData.ParkStatus != value)
                {
                    GameData.ParkStatus = value;
                    OnParkStatusChanged(GameData.ParkStatus);
                }
            }
        }

        public Int32 Budget
        {
            get => GameData.Budget;
            set
            {
                if (GameData.Budget != value)
                {
                    GameData.Budget = value;
                    OnBudgetChanged(GameData.Budget);
                }
            }
        }

        public Int32 GameTime
        {
            get => GameData.GameTime;
        }

        public Int32 VisitorCount
        {
            get => GameData.People.PersonLocators.Where(p => p.Person is Visitor).Count();
        }

        public Int32 TimeInterval
        {
            get => GameData.TimeInterval;
            set
            {
                if (GameData.TimeInterval != value)
                {
                    GameData.TimeInterval = value;
                    OnTimeIntervalChanged(GameData.TimeInterval);
                }
            }
        }

        public String Name
        {
            get => GameData.Name;
            set
            {
                if (GameData.Name != value)
                {
                    GameData.Name = value;
                }

            }
        }

        #endregion

        #region Events

        public event EventHandler GameCreated;

        public event EventHandler GameOver;

        public event EventHandler<ParkStatus> ParkStatusChanged;

        public event EventHandler<Int32> BudgetChanged;

        public event EventHandler<Int32> GameTimeChanged;

        public event EventHandler<Int32> TimeIntervalChanged;

        public event EventHandler<PersonAddRemoveEventArgs> PersonAdded;

        public event EventHandler<PersonAddRemoveEventArgs> PersonRemoved;

        public event EventHandler<PersonMovedEventArgs> PersonMoved;

        public event EventHandler<Person> PersonChanged;

        public event EventHandler<InfrastructureAddRemoveEventArgs> InfrastructureAdded;

        public event EventHandler<InfrastructuresAddRemoveEventArgs> InfrastructuresAdded;

        public event EventHandler<InfrastructureAddRemoveEventArgs> InfrastructureRemoved;

        public event EventHandler<Infrastructure> InfrastructureChanged;

        //public event EventHandler<Facility> CurrentSelectedInfrastructureChanged;

        #endregion

        #region Constructor

        public GameModel(String name)
        {
            GameData = new GameData(name: name,
                         rows: DEFAULT_ROWS,
                         columns: DEFAULT_COLUMNS,
                         budget: DEFAULT_BUDGET,
                         gameTime: DEFAULT_GAME_TIME,
                         timeInterval: DEFAULT_TIME_INTERVAL,
                         parkStatus: ParkStatus.CLOSED);
        }

        #endregion

        #region Private game methods

        /// <summary></summary>
        /// <param name="count"></param>
        private void SpawnVisitors(Int32 count)
        {
            if (ParkStatus != ParkStatus.OPEN)
                return;

            // TODO: Review Implementation
            // TODO: Attribute tendency to enter...
            Gate gate = GameData.Infrastructure.GetInfrastructureWithType(typeof(Gate)) as Gate;

            if (gate != null)
            {
                for (int i = 0; i < count; i++)
                {
                    int tendency = GameData.Infrastructure.AvailableFacilityElements.Where(f => f.Status == InfrastructureStatus.OPERATING || f.Status == InfrastructureStatus.WAITING).Any() ? 50 : 3;
                    tendency -= VisitorCount < 10 ? 0 : (VisitorCount / 60) * 100;
                    tendency -= gate.EntryFee / 1000;

                    if (random.Next(1, 100) > (100 - tendency))
                    {
                        Visitor v = new Visitor();
                        Point target = GetAvailablePointOnInfrastructure(gate);
                        Place(v, target);
                        gate.Accept(v);
                    }
                }
            }
        }

        private void SpawnMechanic()
        {
            if (ParkStatus != ParkStatus.OPEN)
                return;

            Gate gate = GameData.Infrastructure.GetInfrastructureWithType(typeof(Gate)) as Gate;
            if (gate != null)
            {
                Mechanic m = new Mechanic();
                Point target = GetAvailablePointOnInfrastructure(gate);
                Place(m, target);
                gate.Accept(m);
            }
        }

        /// <summary>
        /// Selects an accessible (there is an path from it's current position) <see cref="Facility"/> for the person.
        /// </summary>
        /// <param name="person"></param>
        private Facility GetTargetFacilityFor(Person person)
        {
            // TODO: Weight choice by price.
            Infrastructure infOfPerson = GameData.People.InfrastructureOfPerson(person);
            List<Facility> availableFacilities = GameData.Infrastructure.AvailableFacilityElements
                                                                        .Where(f => !f.Equals(infOfPerson) && (f.Status == InfrastructureStatus.OPERATING || f.Status == InfrastructureStatus.WAITING))
                                                                        .ToList();

            if (availableFacilities.Any())
            {
                int ind = random.Next(availableFacilities.Count());
                return availableFacilities[ind];
            }

            return null;
        }

        private Person NearestMechanic(Infrastructure infrastructure)
        {
            List<PersonLocator> mechanics = GameData.People.PersonLocators.Where(x => x.Person is Mechanic && (x.Person.Status == PersonStatus.IDLE || x.Person.Status == PersonStatus.WALKING) && GameData.Infrastructure[x.Coords.X,x.Coords.Y].GetType() != typeof(Gate)).ToList();
            // If the mechanic go to repair an infrastructure, remove from the enable mechanics
            if (mechanics.Any())
            {
                for (Int32 i = mechanics.Count - 1; i >= 0; i--)
                {
                    PersonLocator pl = mechanics[i];
                    if (_movementStates.ContainsKey(pl.Person))
                    {
                        Queue<Point> path = _movementStates[pl.Person].Item1;
                        if (path.Any())
                        {
                            if (pl.Person.Status == PersonStatus.WALKING && !(GameData.Infrastructure[path.Last().X, path.Last().Y] is Road))
                            {
                                mechanics.Remove(pl);
                            }
                        }
                        else
                        {
                            mechanics.Remove(pl);
                        }
                    }
                }

                if (mechanics.Any())
                {
                    PersonLocator nearestMechanic = mechanics.First();
                    Int32 nearestDistance = GameData.Infrastructure.GetShortestPath(nearestMechanic.Coords, infrastructure).Count;
                    if (nearestDistance <= 0)
                        return null;

                    foreach (PersonLocator pl in mechanics)
                    {
                        Int32 distance = GameData.Infrastructure.GetShortestPath(pl.Coords, infrastructure).Count;
                        if (distance < nearestDistance)
                        {
                            nearestMechanic = pl;
                            nearestDistance = distance;
                        }
                    }
                    infrastructure.Status = InfrastructureStatus.UNDER_REPAIR;
                    return nearestMechanic.Person;
                }
            }

            return null;
        }

        /// <summary>
        /// Event handler that gets called when people finished interacting with an <see cref="Infrastructure"/> .
        /// </summary>
        /// <param name="person"></param>
        private void InteractionEnded(object sender, List<Person> e)
        {
            foreach (Person p in e)
            {
                GameData.People.SetInfrastructure(p, sender as Infrastructure);

                if (p.Status != PersonStatus.LEAVING)
                {
                    p.Status = PersonStatus.IDLE;
                }
            }
        }

        /// <summary>
        /// Event handler that gets called when people finished waiting at an <see cref="Infrastructure"/> .
        /// </summary>
        /// <param name="person"></param>
        private void WaitingEnded(object sender, List<Person> e)
        {
            /*foreach (Person p in e)
            {
                Point target = GameData.Infrastructure.CoordinatesOf(sender as Infrastructure);
                MovePersonByOne(p, target);
            }*/
        }

        private void Infrastructure_ExpenseGenerated(Object sender, int amount)
        {
            Budget += amount;
        }

        private void Infrastructure_AttributesChanged(object sender, EventArgs e)
        {
            OnInfrastructureChanged(sender as Infrastructure);
        }

        private void Person_AttributesChanged(object sender, EventArgs e)
        {
            OnPersonChanged(sender as Person);
        }

        private void InitTable()
        {
            List<Tuple<Infrastructure, Point>> toPlace = new List<Tuple<Infrastructure, Point>>();
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    toPlace.Add(new Tuple<Infrastructure, Point>(new Dirt(), new Point(i, j)));
                }
            }
            PlaceAll(toPlace);
            Place(new Gate(), GATE_COORDS);
        }

        private void LeavePark(Person person)
        {
            Infrastructure gate = GameData.Infrastructure.GetInfrastructureWithType(typeof(Gate));
            Point p = GameData.People.CoordinatesOf(person);

            if (GameData.Infrastructure[p.X, p.Y] is Facility f && person is Visitor v)
            {
                f.LeaveFacility(v);
            }

            if (GameData.Infrastructure[p.X, p.Y] is Game g && person is Mechanic m)
            {
                g.LeaveGame(m);
            }

            if (GameData.Infrastructure[p.X, p.Y].GetType() != typeof(Game) && person is Mechanic)
            {
                if (_movementStates.ContainsKey(person))
                {
                    Queue<Point> path = _movementStates[person].Item1;
                    if (path.Any())
                    {
                        if (GameData.Infrastructure[path.Last().X, path.Last().Y] is Game game)
                        {
                            game.Status = InfrastructureStatus.FAULTY;
                        }
                    }
                }
            }

            if (gate != null)
            {
                person.Status = PersonStatus.LEAVING;
                MovePersonTo(person, gate, GameData.People.InfrastructureOfPerson(person), () => RemovePerson(person));
            }
        }

        private void RemoveMechanicFromPark()
        {
            Person person = null;
            List<PersonLocator> mechanics = GameData.People.PersonLocators.Where(p => p.Person is Mechanic && p.Person.Status != PersonStatus.LEAVING).ToList();
            if (mechanics.Count() > 0)
            {
                person = mechanics.First().Person;
                if (_movementStates.ContainsKey(person))
                {
                    Queue<Point> path = _movementStates[person].Item1;
                    if (path.Any())
                    {
                        if (GameData.Infrastructure[path.Last().X, path.Last().Y] is Game g)
                        {
                            g.Status = InfrastructureStatus.FAULTY;
                        }
                        LeavePark(person);
                    }
                }
                else
                {
                    LeavePark(person);
                }

            }
        }

        private Point GetAvailablePointOnInfrastructure(Infrastructure infrastructure)
        {
            Point coords = GameData.Infrastructure.CoordinatesOf(infrastructure);
            Point size = infrastructure.Size;

            if (infrastructure.UseableArea == null)
            {
                int edgeDistX = (size.X > 2 ? ((size.X > 3) ? 2 : 1) : 0), edgeDistY = (size.Y > 2 ? ((size.Y > 3) ? 2 : 1) : 0);

                Int32 x = random.Next(coords.X + edgeDistX, coords.X + size.X - edgeDistX);
                Int32 y = random.Next(coords.Y + edgeDistY, coords.Y + size.Y - edgeDistY);

                return new Point(x, y);
            }

            foreach (Point point in infrastructure.UseableArea)
            {
                Point offsPoint = point.Offset(coords.X, coords.Y);
                if (GameData.People[offsPoint.X, offsPoint.Y].Count() == 0)
                {
                    // if there is an empty field, return that
                    return offsPoint;
                }
            }

            // else return a random field
            return infrastructure.UseableArea[random.Next(infrastructure.UseableArea.Count)].Offset(coords.X, coords.Y);
        }

        #endregion

        #region Public game methods

        /// <summary>
        /// Start a new game.
        /// </summary>
        public void NewGame(String name)
        {
            GameData = new GameData(name: name,
                                     rows: DEFAULT_ROWS,
                                     columns: DEFAULT_COLUMNS,
                                     budget: DEFAULT_BUDGET,
                                     gameTime: DEFAULT_GAME_TIME,
                                     timeInterval: DEFAULT_TIME_INTERVAL,
                                     parkStatus: ParkStatus.CLOSED);

            _movementStates.Clear();
            _callbackQueue.Clear();
            SpawnIn = 0;
            OnGameCreated();
            OnParkStatusChanged(ParkStatus);
            InitTable();
            OnTimeIntervalChanged(TimeInterval);
        }

        /// <summary>
        /// Start a new game.
        /// </summary>
        public void NewGameButDoNotStart(String name)
        {
            GameData = new GameData(name: name,
                                     rows: DEFAULT_ROWS,
                                     columns: DEFAULT_COLUMNS,
                                     budget: DEFAULT_BUDGET,
                                     gameTime: DEFAULT_GAME_TIME,
                                     timeInterval: DEFAULT_TIME_INTERVAL,
                                     parkStatus: ParkStatus.CLOSED);

            OnGameCreated();
            OnParkStatusChanged(ParkStatus);
            InitTable();
        }


        /// <summary>
        /// Open park.
        /// </summary>
        public void OpenPark()
        {
            ParkStatus = ParkStatus.OPEN;
        }

        /// <summary>
        /// Open park.
        /// </summary>
        public void ClosePark()
        {
            PersonLocator[] arr = GameData.People.PersonLocators.ToArray();

            foreach (PersonLocator p in arr)
            {
                if (p.Person is Visitor)
                    LeavePark(p.Person);

                if (p.Person is Mechanic)
                    LeavePark(p.Person);
            }
            ParkStatus = ParkStatus.CLOSED;
        }

        /// <summary></summary>
        /// <param name="passedMillis"></param>
        public void AdvanceTime()
        {
            GameData.GameTime += 1000;
            OnGameTimeChanged(GameData.GameTime);

            foreach (Infrastructure inf in GameData.Infrastructure.InfrastructueElements)
            {
                inf.Tick(TimeInterval);
            }

            SpawnIn -= TimeInterval;
            if (SpawnIn <= 0)
            {
                SpawnVisitors(random.Next(1, 100) / 100 % 3 + 1);
                SpawnIn = DEFAULT_SPAWN_INTERVAL;
            }

            // spawn and remove mechnaic
            if (MechanicCount >= 0)
            {
                Int32 x = GameData.People.PersonLocators.Where(p => p.Person is Mechanic && p.Person.Status != PersonStatus.LEAVING).Count();

                Int32 mechanic = MechanicCount - x;

                if (mechanic > 0)
                {
                    for (Int32 i = 0; i < mechanic; i++)
                    {
                        SpawnMechanic();
                    }
                }

                x = GameData.People.PersonLocators.Where(p => p.Person is Mechanic && p.Person.Status != PersonStatus.LEAVING && GameData.Infrastructure[p.Coords.X, p.Coords.Y].GetType() != typeof(Gate)).Count();
                mechanic = MechanicCount - x;

                if (mechanic < 0)
                {
                    for (Int32 i = 0; i > mechanic; i--)
                    {
                        RemoveMechanicFromPark();
                    }
                }
            }

            // Search faulty infrastructure
            foreach (Infrastructure infrastructure in GameData.Infrastructure.AvailableInfrastructureElements)
            {
                if (infrastructure is Game && infrastructure.Status == InfrastructureStatus.FAULTY)
                {
                    Person person = NearestMechanic(infrastructure);
                    if (person != null)
                    {
                        MovePersonTo(person, infrastructure, GameData.Infrastructure[GATE_COORDS.X, GATE_COORDS.Y]);
                    }

                }
            }

            // Handle moving persons step-by-step
            List<Person> reachedTarget = new List<Person>();
            foreach (Person person in _movementStates.Keys)
            {
                Queue<Point> path = _movementStates[person].Item1;
                if (path.Count == 0)
                {
                    // target reached
                    reachedTarget.Add(person);
                }
                else
                {
                    Point goToCoords = path.Dequeue();
                    MovePersonByOne(person, goToCoords);
                }
            }
            foreach (Person person in reachedTarget)
            {
                Point reachedCoords = GameData.People.CoordinatesOf(person);
                Action callback = _movementStates[person].Item2;

                if (person.Status != PersonStatus.LEAVING)
                {
                    person.Status = PersonStatus.IDLE;
                }
                _movementStates.Remove(person);

                Visitable v = GameData.Infrastructure[reachedCoords.X, reachedCoords.Y] as Visitable;

                if (v != null)
                {
                    if (callback != null)
                        MovePersonByOne(person, GetAvailablePointOnInfrastructure(v), callback);
                    v.Accept(person);
                }
            }

            // Handle callbacks
            for (int i = 0; i < _callbackQueue.Count; i++)
            {
                Tuple<Int32, Action> cbTuple = _callbackQueue.Dequeue();
                if (cbTuple.Item1 > 0)
                {
                    _callbackQueue.Enqueue(new Tuple<int, Action>(cbTuple.Item1 - 1, cbTuple.Item2));
                }
                else
                {
                    cbTuple.Item2?.Invoke();
                }
            }

            // Handle target choice for IDLE visitors
            foreach (PersonLocator pl in GameData.People.PersonLocators.ToList())
            {
                if (pl.Person is Visitor v)
                {
                    if (v.Mood > 0 && GameData.GameTime % 30000 == 0)
                    {
                        // Plants raising the mood of the visitors
                        for (Int32 i = pl.Coords.X - 2; i < pl.Coords.X + 2; i++)
                        {
                            for (Int32 j = pl.Coords.Y - 2; j < pl.Coords.Y + 2; j++)
                            {
                                if ((i >= 0 && j >= 0 && i < GameData.Rows && j < GameData.Columns) && GameData.Infrastructure[i, j] is Plant)
                                {
                                    v.Mood = v.Mood + 0.05f >= 1.0f ? 1.0f : v.Mood + 0.05f;

                                }
                            }
                        }
                    }

                    switch (v.Status)
                    {
                        case PersonStatus.IDLE:
                            v.Mood -= v.Food > 0.5 ? (float)Helpers.GetRandomDouble(random, 0.001, 0.005) : (float)Helpers.GetRandomDouble(random, 0.005, 0.01);
                            v.Food -= (float)Helpers.GetRandomDouble(random, 0.001, 0.005);
                            v.Food = v.Food < 0 ? 0 : v.Food;
                            v.Mood = v.Mood < 0 ? 0 : v.Mood;

                            if (v.Mood > 0 && v.Food > 0)
                            {
                                Facility facility = GetTargetFacilityFor(pl.Person);
                                if (facility != null)
                                {
                                    MovePersonTo(pl.Person, facility, pl.Infrastructure);
                                }
                                else
                                {
                                    // choose a random road for the person
                                    /* Infrastructure infOfPerson = GameData.People.InfrastructureOfPerson(pl.Person);
                                     List<Infrastructure> availableRoads = GameData.Infrastructure.AvailableInfrastructureElements
                                                                                                 .Where(e => e is Road)
                                                                                                 .Where(f => !f.Equals(infOfPerson))
                                                                                                 .ToList();
                                     if (availableRoads.Any())
                                     {
                                         int ind = random.Next(availableRoads.Count());
                                         MovePersonTo(pl.Person, availableRoads[ind], pl.Infrastructure);
                                     }
                                     else
                                     {
                                         LeavePark(pl.Person);
                                     } */
                                    LeavePark(pl.Person);

                                }
                            }
                            break;

                        case PersonStatus.WAITING:
                            v.Mood -= v.Food > 0.5 ? (float)Helpers.GetRandomDouble(random, 0.005, 0.01) : (float)Helpers.GetRandomDouble(random, 0.01, 0.05);
                            v.Food -= (float)Helpers.GetRandomDouble(random, 0.001, 0.005);
                            v.Food = v.Food < 0 ? 0 : v.Food;
                            v.Mood = v.Mood < 0 ? 0 : v.Mood;
                            break;

                        case PersonStatus.WALKING:
                            v.Mood -= v.Food > 0.5 ? (float)Helpers.GetRandomDouble(random, 0.001, 0.005) : (float)Helpers.GetRandomDouble(random, 0.005, 0.01);
                            v.Food -= (float)Helpers.GetRandomDouble(random, 0.001, 0.005);
                            v.Food = v.Food < 0 ? 0 : v.Food;
                            v.Mood = v.Mood < 0 ? 0 : v.Mood;
                            break;

                        default:
                            break;
                    }

                    if ((v.Mood <= 0 || v.Money <= 0) && v.Status != PersonStatus.LEAVING)
                    {
                        LeavePark(v);
                    }
                }
                else if (pl.Person is Mechanic)
                {
                    if (pl.Person.Status is PersonStatus.IDLE)
                    {
                        Infrastructure infOfPerson = GameData.People.InfrastructureOfPerson(pl.Person);
                        List<Infrastructure> availableRoads = GameData.Infrastructure.AvailableInfrastructureElements
                                                                                                 .Where(e => e is Road)
                                                                                                 .Where(f => !f.Equals(infOfPerson))
                                                                                                 .ToList();
                        if (availableRoads.Any())
                        {
                            int ind = random.Next(availableRoads.Count());
                            MovePersonTo(pl.Person, availableRoads[ind], pl.Infrastructure);
                        }
                    }
                }
            }

            //Losing game
            if (Budget < 0)
            {
                OnGameOver();
            }
        }

        /// <summary></summary>
        /// <param name="coords"></param>
        public Infrastructure GetInfrastructureAt(Point coords)
        {
            return GameData.Infrastructure[coords.X, coords.Y];
        }

        /// <summary>Checks whether a given person can stand on a given field.</summary>
        /// <param name="person"></param>
        /// <param name="coords"></param>
        public Boolean CanPlace(Person person, Point coords)
        {
            if (GameData.Infrastructure[coords.X, coords.Y] is Road || GameData.Infrastructure[coords.X, coords.Y] is Visitable)
            {
                return true;
            }

            return false;
        }

        /// <summary>Initial placement of a <see cref="Person"/> object.</summary>
        /// <param name="person"></param>
        /// <param name="coords"></param>
        public void Place(Person person, Point coords)
        {
            if (CanPlace(person, coords))
            {
                GameData.People.AddPerson(person, coords);
                person.AttributesChanged += new EventHandler(Person_AttributesChanged);
                OnPersonAdded(person, coords);
            }
        }

        /// <summary></summary>
        /// <param name="person"></param>
        public void RemovePerson(Person person)
        {
            person.AttributesChanged -= new EventHandler(Person_AttributesChanged);

            if (_movementStates.ContainsKey(person))
            {
                _movementStates.Remove(person);
            }

            Point? coords = GameData.People.CoordinatesOfOrNull(person);
            GameData.People.RemovePerson(person);
            OnPersonRemoved(person, coords);
        }

        /// <summary>Moving a <see cref="Person"/> object by one step.</summary>
        /// <remarks>Even into infrastructures.</remarks>
        /// <param name="person"></param>
        /// <param name="toCoords"></param>
        public void MovePersonByOne(Person person, Point toCoords, Action callback = null)
        {
            Point currCoords = GameData.People.CoordinatesOf(person);

            if (Math.Abs(currCoords.X - toCoords.X) > 1 || Math.Abs(currCoords.Y - toCoords.Y) > 1)
            {
                // throw new InvalidOperationException("Supports only moving by one field.");
            }

            if (!CanPlace(person, toCoords))
            {
                _movementStates.Remove(person);
                person.Status = PersonStatus.IDLE;
            }

            Point fromCoords = GameData.People.CoordinatesOf(person);

            GameData.People.SetCoordinates(person, toCoords);
            OnPersonMoved(person, fromCoords, toCoords);

            _callbackQueue.Enqueue(new Tuple<int, Action>(1, callback));
        }

        /// <summary>
        /// Moving a <see cref="Person"/> object to a given location, on the shortest path.
        /// </summary>
        /// <remarks>Even into infrastructures.</remarks>
        /// <param name="person"></param>
        /// <param name="toCoords"></param>
        public void MovePersonTo(Person person, Infrastructure targetInfrastructure, Infrastructure currentInfrastructure, Action callback = null)
        {
            if (targetInfrastructure == null)
                return;

            if (_movementStates.ContainsKey(person))
            {
                _movementStates.Remove(person);
            }

            Point dest = GameData.Infrastructure.CoordinatesOf(targetInfrastructure);
            dest = targetInfrastructure is Gate ? GetAvailablePointOnInfrastructure(targetInfrastructure) : dest;

            if (!CanPlace(person, dest))
            {
                throw new InvalidOperationException("The given person can't be mooved to that field.");
            }

            Point currCoords = GameData.People.CoordinatesOf(person);
            Queue<Point> path = GameData.Infrastructure.GetShortestPath(currCoords, targetInfrastructure, currentInfrastructure);

            if (person.Status != PersonStatus.LEAVING)
                person.Status = PersonStatus.WALKING;

            _movementStates.Add(person, new Tuple<Queue<Point>, Action>(path, callback));
        }

        /// <summary>
        /// Returns that the the passed <see cref="Infrastructure"/> can be placed on top of an existing one.
        /// </summary>
        /// <param name="infrastrucure"></param>
        /// <param name="coords"></param>
        public Boolean IsValidStacking(Infrastructure infrastructure, Point coord)
        {
            if (infrastructure.GetType() != typeof(Dirt))
            {
                for (int i = coord.X; i < coord.X + infrastructure.Size.X; i++)
                {
                    for (int j = coord.Y; j < coord.Y + infrastructure.Size.Y; j++)
                    {
                        if (!GameData.Infrastructure[i, j].StackableInfrastructure.Any(s => infrastructure.GetType() == s || infrastructure.GetType().IsSubclassOf(s)))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary></summary>
        /// <param name="infrastrucure"></param>
        /// <param name="coords"></param>
        public Boolean CanPlace(Infrastructure infrastructure, Point coords)
        {
            if (coords.X + infrastructure.Size.X - 1 >= DEFAULT_ROWS || coords.Y + infrastructure.Size.Y - 1 >= DEFAULT_COLUMNS ||
                GameData.Budget < infrastructure.Price)
                return false;

            return IsValidStacking(infrastructure, coords);
        }

        /// <summary>Checks whether an infrastructure with given type and size can fit. Price won't be checked.</summary>
        /// <param name="infrastrucure"></param>
        /// <param name="coords"></param>
        public Boolean CanPlaceInfrastructureWithType(Type type, Point infrastructureSize, Point coords)
        {
            if (coords.X + infrastructureSize.X - 1 >= DEFAULT_ROWS || coords.Y + infrastructureSize.Y - 1 >= DEFAULT_COLUMNS)
                return false;

            // Check if stacking is valid
            if (type != typeof(Dirt))
            {
                for (int i = coords.X; i < coords.X + infrastructureSize.X; i++)
                {
                    for (int j = coords.Y; j < coords.Y + infrastructureSize.Y; j++)
                    {
                        if (!GameData.Infrastructure[i, j].StackableInfrastructure.Any(s => type == s || type.IsSubclassOf(s)))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary></summary>
        /// <param name="infrastrucure"></param>
        /// <param name="coords"></param>
        public void Place(Infrastructure infrastructure, Point coords)
        {
            if (CanPlace(infrastructure, coords))
            {
                GameData.Budget -= infrastructure.Price;
                GameData.Infrastructure.AddInfrastructure(infrastructure, coords);
                infrastructure.InteractionEnded += new EventHandler<List<Person>>(InteractionEnded);
                infrastructure.WaitingEnded += new EventHandler<List<Person>>(WaitingEnded);
                infrastructure.ExpenseGenerated += new EventHandler<int>(Infrastructure_ExpenseGenerated);
                infrastructure.AttributesChanged += new EventHandler(Infrastructure_AttributesChanged);
                OnInfrastructureAdded(infrastructure, coords);
            }
        }

        /// <summary>Places multiple infrastructures on the table.</summary>
        /// <param name="infrastructures"></param>
        public void PlaceAll(List<Tuple<Infrastructure, Point>> infrastructures)
        {
            List<Tuple<Infrastructure, Point>> successfullyAdded = new List<Tuple<Infrastructure, Point>>();

            foreach (var infTuple in infrastructures)
            {
                Infrastructure infrastructure = infTuple.Item1;
                Point coords = infTuple.Item2;

                if (CanPlace(infrastructure, coords))
                {
                    GameData.Infrastructure.AddInfrastructure(infrastructure, coords);
                    infrastructure.InteractionEnded += new EventHandler<List<Person>>(InteractionEnded);
                    infrastructure.WaitingEnded += new EventHandler<List<Person>>(WaitingEnded);
                    infrastructure.ExpenseGenerated += new EventHandler<int>(Infrastructure_ExpenseGenerated);
                    infrastructure.AttributesChanged += new EventHandler(Infrastructure_AttributesChanged);
                    successfullyAdded.Add(infTuple);
                }
            }

            OnInfrastructuresAdded(successfullyAdded);
        }

        /// <summary></summary>
        /// <param name="person"></param>
        /// <param name="coords"></param>
        public Boolean CanDemolish(Point coords)
        {
            Infrastructure inf = GameData.Infrastructure[coords.X, coords.Y];

            if (inf is Dirt || inf is Gate)
                return false;

            return true;
        }

        /// <summary></summary>
        /// <param name="infrastructure"></param>
        public void Demolish(Point coords)
        {
            if (CanDemolish(coords))
            {
                Infrastructure infrastructure = GameData.Infrastructure[coords.X, coords.Y];
                Point coord = GameData.Infrastructure.CoordinatesOf(infrastructure);
                infrastructure.InteractionEnded -= new EventHandler<List<Person>>(InteractionEnded);
                infrastructure.WaitingEnded -= new EventHandler<List<Person>>(WaitingEnded);
                infrastructure.ExpenseGenerated -= new EventHandler<int>(Infrastructure_ExpenseGenerated);
                infrastructure.AttributesChanged -= new EventHandler(Infrastructure_AttributesChanged);
                GameData.Infrastructure.RemoveInfrastructure(infrastructure);
                OnInfrastructureRemoved(infrastructure, coord);
            }
        }

        #endregion

        #region Private event methods

        private void OnGameCreated()
        {
            GameCreated?.Invoke(this, EventArgs.Empty);
        }

        private void OnGameOver()
        {
            GameOver?.Invoke(this, EventArgs.Empty);
        }

        /// <summary></summary>
        /// <param name="newParkStatus"></param>
        private void OnParkStatusChanged(ParkStatus newParkStatus)
        {
            ParkStatusChanged?.Invoke(this, newParkStatus);
        }

        /// <summary></summary>
        /// <param name="newBudget"></param>
        private void OnBudgetChanged(Int32 newBudget)
        {
            BudgetChanged?.Invoke(this, newBudget);
        }

        /// <summary></summary>	
        /// <param name="newGameTime"></param>	
        private void OnGameTimeChanged(Int32 newGameTime)
        {
            GameTimeChanged?.Invoke(this, newGameTime);
        }

        /// <summary></summary>
        /// <param name="newTimeInterval"></param>
        private void OnTimeIntervalChanged(Int32 newTimeInterval)
        {
            TimeIntervalChanged?.Invoke(this, newTimeInterval);
        }

        /// <summary></summary>
        /// <param name="person"></param>
        private void OnPersonAdded(Person person, Point coords)
        {
            PersonAdded?.Invoke(this, new PersonAddRemoveEventArgs(person, coords));
        }

        /// <summary></summary>
        /// <param name="person"></param>
        private void OnPersonRemoved(Person person, Point? coords)
        {
            PersonRemoved?.Invoke(this, new PersonAddRemoveEventArgs(person, coords));
        }

        /// <summary></summary>
        /// <param name="person"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void OnPersonMoved(Person person, Point from, Point to)
        {
            PersonMoved?.Invoke(this, new PersonMovedEventArgs(person, from, to));
        }

        /// <summary></summary>
        /// <param name="person"></param>
        private void OnPersonChanged(Person person)
        {
            PersonChanged?.Invoke(this, person);
        }

        /// <summary></summary>
        /// <param name="infrastructure"></param>
        private void OnInfrastructureAdded(Infrastructure infrastructure, Point coords)
        {
            InfrastructureAdded?.Invoke(this, new InfrastructureAddRemoveEventArgs(infrastructure, coords));
        }

        /// <summary></summary>
        /// <param name="infrastructure"></param>
        private void OnInfrastructuresAdded(List<Tuple<Infrastructure, Point>> infrastructures)
        {
            InfrastructuresAdded?.Invoke(this, new InfrastructuresAddRemoveEventArgs(infrastructures));
        }

        /// <summary></summary>
        /// <param name="infrastructure"></param>
        private void OnInfrastructureRemoved(Infrastructure infrastructure, Point coords)
        {
            InfrastructureRemoved?.Invoke(this, new InfrastructureAddRemoveEventArgs(infrastructure, coords));
        }

        /// <summary></summary>
        /// <param name="infrastructure"></param>
        private void OnInfrastructureChanged(Infrastructure infrastructure)
        {
            InfrastructureChanged?.Invoke(this, infrastructure);
        }

        #endregion

    }
}
