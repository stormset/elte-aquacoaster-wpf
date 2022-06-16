using System;
using System.Collections.Generic;
using AquaCoaster.Utilities;


namespace AquaCoaster.Model.Entities
{
    public class Gate : Visitable
    {
        private static Dictionary<String, Object> CONFIG = new Dictionary<String, Object>
        {
            { "Name",     nameof(Gate) },
            { "Price",    0 },
            { "Size",     new Point(6, 2) },
            { "UseableArea", new List<Point>{ new Point(2, 0), new Point(3, 0), new Point(2, 1), new Point(3, 1) } },
            { "EntryFee", 2000 }
        };

        public static Dictionary<String, Object> Config { get => new Dictionary<string, object>(CONFIG); }

        public Int32 EntryFee { get; set; }

        public Gate() : this(CONFIG) { }

        public Gate(Dictionary<String, Object> config) : base(config)
        {
            this.EntryFee = (Int32)config["EntryFee"];
        }

        /// <summary>
        /// Handles the acceptation of a <see cref="Person"/> by a <see cref="Gate"/> element.
        /// </summary>
        /// <param name="visitor"></param>
        public override void Accept(Person person)
        {
            person.VisitGate(this);
        }

        /// <summary>
        /// Handles the serving of a customer by a <see cref="Game"/>.
        /// </summary>
        /// <param name="visitor"></param>
        public void EnterPark(Visitor visitor)
        {
            if (visitor.Status == Enums.PersonStatus.IDLE)
            {
                visitor.Money -= EntryFee;
                OnExpenseGenerated(EntryFee);
                OnInteractionEnded(new System.Collections.Generic.List<Person> { visitor });
            }
        }

        public void AddMechanic(Mechanic m)
        {
            if (m.Status == Enums.PersonStatus.IDLE)
            {
                OnExpenseGenerated(m.Price);
                OnInteractionEnded(new System.Collections.Generic.List<Person> { m });
            }
        }
    }
}
