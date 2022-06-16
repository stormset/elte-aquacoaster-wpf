using System;
using AquaCoaster.Model.Enums;


namespace AquaCoaster.Model.Entities
{
    public class Visitor : Person
    {
        private Int32 _money;
        public Int32 Money
        {
            get => _money;
            set
            {
                if (_money != value)
                {
                    _money = value;
                    OnAttributesChanged();
                }
            }
        }

        private float _mood;
        public float Mood
        {
            get => _mood;
            set
            {
                if (_mood != value)
                {
                    _mood = value;
                    OnAttributesChanged();
                }
            }
        }

        private float _food;
        public float Food
        {
            get => _food;
            set
            {
                if (_food != value)
                {
                    _food = value;
                    OnAttributesChanged();
                }
            }
        }

        public Visitor(Int32 money = 10000, float mood = 1f, float food = 1f, PersonStatus status = PersonStatus.IDLE) : base(status)
        {
            Money = money;
            Mood = mood;
            Food = food;
        }

        #region Visiting of infrastructures

        /// <summary>
        /// Handles the interaction logic of a <see cref="Visitor"/> with a <see cref="Game"/> element.
        /// </summary>
        /// <param name="game"></param>
        public override void VisitGame(Game game) {
            game.ServeVisitor(this);
        }

        /// <summary>
        /// Handles the interaction logic of a <see cref="Visitor"/> with a <see cref="Restaurant"/> element.
        /// </summary>
        /// <param name="restaurant"></param>
        public override void VisitRestaurant(Restaurant restaurant) {
            restaurant.ServeVisitor(this);
        }

        /// <summary>
        /// Handles the interaction logic of a <see cref="Visitor"/> with a <see cref="Gate"/> element.
        /// </summary>
        /// <param name="gate"></param>
        public override void VisitGate(Gate gate) {
            gate.EnterPark(this);
        }

        #endregion
    }
}
