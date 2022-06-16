using System;
using System.Collections.Generic;
using AquaCoaster.Model.Enums;


namespace AquaCoaster.Model.Entities
{
    public abstract class WaterGame : Game
    {
        protected WaterGame(Dictionary<String, Object> config) : base(config) { }
    }
}
