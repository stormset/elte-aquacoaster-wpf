using System;
using System.Collections.Generic;


namespace AquaCoaster.Model.Entities
{
    public class Pier : Road
    {
        private static Dictionary<String, Object> CONFIG = new Dictionary<String, Object>
        {
            { "Name", nameof(Pier) },
            { "Price", 200 },
            { "Size", new Utilities.Point(1, 1) }
        };

        public static Dictionary<String, Object> Config { get => new Dictionary<string, object>(CONFIG); }

        public Pier() : this(CONFIG) { }

        public Pier(Dictionary<String, Object> config) : base(config) { }
    }
}
