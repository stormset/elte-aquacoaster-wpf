using System;
using System.Collections.Generic;
using System.Text;

namespace AquaCoaster.Model.Entities
{
    class WaterSlide : WaterGame
    {
        private static Dictionary<String, Object> CONFIG = new Dictionary<String, Object>
        {
            { "Name",              "Water Slide" },
            { "Price",             7000 },
            { "Size",              new Utilities.Point(4, 3) },
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

        public WaterSlide() : this(CONFIG) { }

        public WaterSlide(Dictionary<String, Object> config) : base(config) { }
    }
}
