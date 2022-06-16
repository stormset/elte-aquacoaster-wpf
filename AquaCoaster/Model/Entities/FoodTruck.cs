using System;
using System.Collections.Generic;
using System.Text;

namespace AquaCoaster.Model.Entities
{
    class FoodTruck : Restaurant
    {
        private static Dictionary<String, Object> CONFIG = new Dictionary<String, Object>
        {
            { "Name",              "FoodTruck" },
            { "Price",             2300 },
            { "Size",              new Utilities.Point(5, 3) },
            { "Capacity",          5 },
            { "MinimumCapacity",   2 },
            { "UseFee",            1300 },
            { "MoodFactor",        0f },
            { "FoodFactor",        0.3f },
            { "RoundTime",         10 },
            { "ContinuousExpense", 5 },
            { "PerUseExpense",     1 },
        };

        public static Dictionary<String, Object> Config { get => new Dictionary<string, object>(CONFIG); }

        public FoodTruck() : this(CONFIG) { }

        public FoodTruck(Dictionary<String, Object> config) : base(config) { }
    }
}
