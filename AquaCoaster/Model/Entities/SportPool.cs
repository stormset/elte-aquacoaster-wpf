using System;
using System.Collections.Generic;


namespace AquaCoaster.Model.Entities
{
    public class SportPool : LandGame
    {
        private static Dictionary<String, Object> CONFIG = new Dictionary<String, Object>
        {
            { "Name",              "Sport Pool" },
            { "Price",             6000 },
            { "Size",              new Utilities.Point(6, 3) },
            { "Capacity",          60 },
            { "MinimumCapacity",   5 },
            { "UseFee",            300 },
            { "MoodFactor",        0.15f },
            { "FoodFactor",        0f },
            { "RoundTime",         12 },
            { "ContinuousExpense", 5 },
            { "PerUseExpense",     1 },
        };

        public static Dictionary<String, Object> Config { get => new Dictionary<string, object>(CONFIG); }

        public SportPool() : this(CONFIG) { }

        public SportPool(Dictionary<String, Object> config) : base(config) { }
    }
}
