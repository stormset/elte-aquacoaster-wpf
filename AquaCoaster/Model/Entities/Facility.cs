using System;
using System.Collections.Generic;
using AquaCoaster.Model.Enums;


namespace AquaCoaster.Model.Entities
{
    public abstract class Facility : Visitable
    {
        private Int32 _capacity;
        /// <summary>
        /// Number of people that a facility can serve at the same time.
        /// </summary>
        /// <remarks>
        /// Invariant: 1. _currentUsers.Count <= Capacity
        ///            2. if a new customer arrives and _currentUsers.Count > Capacity 
        ///               it should be added to the _waitingQueue
        /// </remarks>
        public Int32 Capacity
        {
            get => _capacity;
            set
            {
                if (_capacity != value)
                {
                    _capacity = value;
                    OnAttributesChanged();
                }
            }
        }

        private Int32? _minimumCapacity;
        /// <summary>
        /// Minimum Capacity of the facility, should be reached before
        /// switching the status to <see cref="Enums.InfrastructureStatus.OPERATING"/>.
        /// <para>The value should be null, if there is no minimum capacity required.</para> 
        /// </summary>
        public Int32? MinimumCapacity
        {
            get => _minimumCapacity;
            set
            {
                if (_minimumCapacity != value)
                {
                    _minimumCapacity = value;
                    OnAttributesChanged();
                }
            }
        }


        private Int32 _useFee;
        /// <summary>
        /// Fee of using a <see cref="Facility"/>, that a <see cref="Visitor"/> should pay.
        /// </summary>
        public Int32 UseFee
        {
            get => _useFee;
            set
            {
                if (_useFee != value)
                {
                    _useFee = value;
                    OnAttributesChanged();
                }
            }
        }

        public float MoodFactor { get; protected set; }

        public float FoodFactor { get; protected set; }

        public Int32 RoundTime { get; protected set; }

        public Int32 ContinuousExpense { get; protected set; }

        public Int32 PerUseExpense { get; protected set; }

        private List<Visitor> _currentUsers;
        public List<Visitor> CurrentUsers { 
            get => _currentUsers;
            protected set
            {
                if (_currentUsers != value)
                {
                    _currentUsers = value;
                    OnAttributesChanged();
                }
            }
        }

        private Queue<Visitor> _waitingQueue;
        public Queue<Visitor> WaitingQueue
        {
            get => _waitingQueue;
            protected set
            {
                if (_waitingQueue != value)
                {
                    _waitingQueue = value;
                    OnAttributesChanged();
                }
            }
        }

        public virtual void LeaveFacility(Visitor visitor) { }

        protected Facility(Dictionary<String, Object> config) : base(config)
        {
            if (!config.ContainsKey("Status"))
            {
                this.Status = InfrastructureStatus.UNDER_CONSTRUCTION;
            }

            if (config.ContainsKey("MinimumCapacity"))
            {
                this.MinimumCapacity = (Int32?)config["MinimumCapacity"];
            }
            else
            {
                this.MinimumCapacity = null;
            }

            /* Non-default config params */
            this.Capacity = (Int32)config["Capacity"];
            this.UseFee = (Int32)config["UseFee"];
            this.MoodFactor = (float)config["MoodFactor"];
            this.FoodFactor = (float)config["FoodFactor"];
            this.RoundTime = (Int32)config["RoundTime"];
            this.ContinuousExpense = (Int32)config["ContinuousExpense"];
            this.PerUseExpense = (Int32)config["PerUseExpense"];

            _currentUsers = new List<Visitor>();
            _waitingQueue = new Queue<Visitor>();
        }

        public abstract void ServeVisitor(Visitor v);

    }
}
