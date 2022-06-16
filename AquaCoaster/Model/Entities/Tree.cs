using System;
using System.Collections.Generic;
using System.Text;

namespace AquaCoaster.Model.Entities
{
    class Tree : Infrastructure
    {
        private static Dictionary<String, Object> CONFIG = new Dictionary<String, Object>
        {
            { "Name", nameof(Tree) },
            { "Price", 10 },
            { "Size", new Utilities.Point(2, 2) }
        };

        public static Dictionary<String, Object> Config { get => new Dictionary<string, object>(CONFIG); }

        public Tree() : this(CONFIG) { }
        public Tree(Dictionary<String, Object> config) : base(config) { }
    }
}
