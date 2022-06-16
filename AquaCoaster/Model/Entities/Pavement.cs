using System;
using System.Collections.Generic;


namespace AquaCoaster.Model.Entities
{
    public class Pavement : Road
    {
        private static Dictionary<String, Object> CONFIG = new Dictionary<String, Object>
        {
            { "Name", nameof(Pavement) },
            { "Price", 100 },
            { "Size", new Utilities.Point(1, 1) }
        };

        public static Dictionary<String, Object> Config { get => new Dictionary<string, object>(CONFIG); }

        public Pavement() : this(CONFIG) { }

        public Pavement(Dictionary<String, Object> config) : base(config) { }
    }
}
