using System;
using System.Collections.Generic;


namespace AquaCoaster.Model.Entities
{
    public abstract class Road : Visitable
    {
        protected Road(Dictionary<String, Object> config) : base(config) { }

        /// <summary>
        /// Handles the acceptation of a <see cref="Person"/> by a <see cref="Road"/> element.
        /// </summary>
        /// <param name="visitor"></param>
        public override void Accept(Person person)
        {
            person.VisitRoad(this);
        }
    }
}
