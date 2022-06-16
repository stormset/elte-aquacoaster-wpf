using System;
using System.Collections.Generic;


namespace AquaCoaster.Model.Entities
{
    public class Plant : Infrastructure
    {
        private static Dictionary<String, Object> CONFIG = new Dictionary<String, Object>
        {
            { "Name", nameof(Plant) },
            { "Price", 10 },
            { "Size", new Utilities.Point(1, 1) }
        };

        public static Dictionary<String, Object> Config { get => new Dictionary<string, object>(CONFIG); }

        public Plant() : this(CONFIG) { }
        public Plant(Dictionary<String, Object> config) : base(config) { }
    }
}
