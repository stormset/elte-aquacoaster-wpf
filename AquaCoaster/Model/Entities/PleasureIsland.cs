using System;
using System.Collections.Generic;


namespace AquaCoaster.Model.Entities
{
    public class PleasureIsland : WaterGame
    {
        private static Dictionary<String, Object> CONFIG = new Dictionary<String, Object>
        {
            { "Name",              "Pleasure Island" },
            { "Price",             7000 },
            { "Size",              new Utilities.Point(4, 4) },
            { "Capacity",          60 },
            { "MinimumCapacity",   2 },
            { "UseFee",            500 },
            { "MoodFactor",        0.35f },
            { "FoodFactor",        0f },
            { "RoundTime",         8 },
            { "ContinuousExpense", 10 },
            { "PerUseExpense",     5 },
        };

        public static Dictionary<String, Object> Config { get => new Dictionary<string, object>(CONFIG); }

        public PleasureIsland() : this(CONFIG) { }

        public PleasureIsland(Dictionary<String, Object> config) : base(config) { }
    }
}
