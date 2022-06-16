using System;
using System.Collections.Generic;


namespace AquaCoaster.Model.Entities
{
    public class Carousel : LandGame
    {
        private static Dictionary<String, Object> CONFIG = new Dictionary<String, Object>
        {
            { "Name",              nameof(Carousel) },
            { "Price",             1000 },
            { "Size",              new Utilities.Point(5, 5) },
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

        public Carousel() : this(CONFIG) { }

        public Carousel(Dictionary<String, Object> config) : base(config) { }
    }
}
