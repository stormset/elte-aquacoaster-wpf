using System;
using AquaCoaster.Model.Enums;


namespace AquaCoaster.Model.Entities
{
    public class Mechanic : Person, IPlaceable
    {
        public Int32 Price { get; protected set; }

        public Mechanic(Int32 Price = 500, PersonStatus status = PersonStatus.IDLE) : base(status)
        {
            this.Price = Price;
        }

        public override void VisitGame(Game game)
        {
            game.ServeMechanic(this);
        }

        public override void VisitGate(Gate gate)
        {
            gate.AddMechanic(this);
        }
    }
}
