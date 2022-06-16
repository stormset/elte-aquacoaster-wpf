using System;
using System.Collections.Generic;
using System.Text;

namespace AquaCoaster.Model.Entities
{
    class RollerCoaster : LandGame
    {
        private static Dictionary<String, Object> CONFIG = new Dictionary<String, Object>
        {
            { "Name",              nameof(RollerCoaster) },
            { "Price",             1000 },
            { "Size",              new Utilities.Point(5, 4) },
            { "Capacity",          10 },
            { "MinimumCapacity",   2 },
            { "UseFee",            100 },
            { "MoodFactor",        0.2f },
            { "FoodFactor",        0f },
            { "RoundTime",         10 },
            { "ContinuousExpense", 5 },
            { "PerUseExpense",     1 },
        };

        public static Dictionary<String, Object> Config { get => new Dictionary<string, object>(CONFIG); }

        public RollerCoaster() : this(CONFIG) { }

        public RollerCoaster(Dictionary<String, Object> config) : base(config) { }
    }
}
