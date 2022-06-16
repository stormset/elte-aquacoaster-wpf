using System;
using AquaCoaster.Model.Enums;


namespace AquaCoaster.Model.Entities
{
    public abstract class Person
    {
        private PersonStatus _status;
        public PersonStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnAttributesChanged();
                }
            }
        }

        public event EventHandler AttributesChanged;

        protected Person(PersonStatus status)
        {
            this.Status = status;
        }

        #region Visiting of infrastructures

        public virtual void VisitGame(Game game) { }

        public virtual void VisitRestaurant(Restaurant restaurant) { }

        public virtual void VisitGate(Gate gate) { }

        public virtual void VisitRoad(Road road) { }

        #endregion

        protected void OnAttributesChanged()
        {
            AttributesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
