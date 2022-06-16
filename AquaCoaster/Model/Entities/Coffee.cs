using System;
using System.Collections.Generic;


namespace AquaCoaster.Model.Entities
{
    public class Coffee : Restaurant
    {
        private static Dictionary<String, Object> CONFIG = new Dictionary<String, Object>
        {
            { "Name",              "Coffee Shop" },
            { "Price",             3000 },
            { "Size",              new Utilities.Point(5, 4) },
            { "Capacity",          1 },
            { "MinimumCapacity",   2 },
            { "UseFee",            400 },
            { "MoodFactor",        0f },
            { "FoodFactor",        0.15f },
            { "RoundTime",         3 },
            { "ContinuousExpense", 2 },
            { "PerUseExpense",     1 },
        };

        public static Dictionary<String, Object> Config { get => new Dictionary<string, object>(CONFIG); }

        public Coffee() : this(CONFIG) { }

        public Coffee(Dictionary<String, Object> config) : base(config) { }
    }
}
