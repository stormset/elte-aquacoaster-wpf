using System;
using AquaCoaster.Model.Entities;
using AquaCoaster.Utilities;


namespace AquaCoaster.Model
{
    public class InfrastructureAddRemoveEventArgs
    {
        public Infrastructure Infrastructure { get; }

        public Point Coords { get; }

        public InfrastructureAddRemoveEventArgs(Infrastructure infrastructure, Point coords)
        {
            this.Infrastructure = infrastructure;
            this.Coords = coords;
        }
    }
}
