using System;
using System.Collections.Generic;


namespace AquaCoaster.Model.Entities
{
    public class FishNChipsRestaurant : Restaurant
    {
        private static Dictionary<String, Object> CONFIG = new Dictionary<String, Object>
        {
            { "Name",              "Fish&Chips" },
            { "Price",             2300 },
            { "Size",              new Utilities.Point(5, 4) },
            { "Capacity",          1 },
            { "MinimumCapacity",   2 },
            { "UseFee",            1300 },
            { "MoodFactor",        0f },
            { "FoodFactor",        0.3f },
            { "RoundTime",         10 },
            { "ContinuousExpense", 5 },
            { "PerUseExpense",     1 },
        };

        public static Dictionary<String, Object> Config { get => new Dictionary<string, object>(CONFIG); }

        public FishNChipsRestaurant() : this(CONFIG) { }

        public FishNChipsRestaurant(Dictionary<String, Object> config) : base(config) { }
    }
}
