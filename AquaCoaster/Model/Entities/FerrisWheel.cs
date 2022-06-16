using System;
using System.Collections.Generic;
using System.Text;

namespace AquaCoaster.Model.Entities
{
    class FerrisWheel : LandGame
    {
        private static Dictionary<String, Object> CONFIG = new Dictionary<String, Object>
        {
            { "Name",              nameof(FerrisWheel) },
            { "Price",             2000 },
            { "Size",              new Utilities.Point(4, 4) },
            { "Capacity",          20 },
            { "MinimumCapacity",   4 },
            { "UseFee",            100 },
            { "MoodFactor",        0.2f },
            { "FoodFactor",        0f },
            { "RoundTime",         10 },
            { "ContinuousExpense", 5 },
            { "PerUseExpense",     1 },
        };

        public static Dictionary<String, Object> Config { get => new Dictionary<string, object>(CONFIG); }

        public FerrisWheel() : this(CONFIG) { }

        public FerrisWheel(Dictionary<String, Object> config) : base(config) { }
    }
}
