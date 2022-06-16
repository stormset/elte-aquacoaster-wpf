using System;
using System.Collections.Generic;


namespace AquaCoaster.Model.Entities
{
    public abstract class Visitable : Infrastructure
    {
        protected Visitable(Dictionary<String, Object> config) : base(config) { }

        public abstract void Accept(Person person);
    }
}
