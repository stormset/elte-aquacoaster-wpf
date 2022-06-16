using System;
using System.Collections.Generic;


namespace AquaCoaster.Model.Entities
{
    public abstract class LandGame : Game
    {
        protected LandGame(Dictionary<String, Object> config) : base(config) { }
    }
}
