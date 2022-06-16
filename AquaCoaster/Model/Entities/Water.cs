using System;
using System.Collections.Generic;


namespace AquaCoaster.Model.Entities
{
    public class Water : Infrastructure
    {
        private static Dictionary<String, Object> CONFIG = new Dictionary<String, Object>
        {
            { "Name", nameof(Water) },
            { "Price", 0 },
            { "Size", new Utilities.Point(1, 1) },
            { "StackableInfrastructure",  new List<Type>{typeof(Entities.Pier), typeof(Entities.WaterGame)} }
        };

        public static Dictionary<String, Object> Config { get => new Dictionary<string, object>(CONFIG); }

        public Water() : this(CONFIG) { }

        public Water(Dictionary<String, Object> config) : base(config) { }
    }
}
