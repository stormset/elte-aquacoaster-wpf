using System;
using System.Collections.Generic;


namespace AquaCoaster.Model.Entities
{
    public class Dirt : Infrastructure
    {
        private static Dictionary<String, Object> CONFIG = new Dictionary<String, Object>
        {
            { "Name", nameof(Dirt) },
            { "Price", 0 },
            { "Size", new Utilities.Point(1, 1) },
            { "StackableInfrastructure",  new List<Type>{ typeof(Entities.Gate), typeof(Entities.Water), typeof(Entities.Pavement), typeof(Entities.LandGame), typeof(Entities.Restaurant), typeof(Entities.Plant), typeof(Tree) } }
        };

        public static Dictionary<String, Object> Config { get => new Dictionary<string, object>(CONFIG); }

        public Dirt() : this(CONFIG) { }

        public Dirt(Dictionary<String, Object> config) : base(config) { }
    }
}
