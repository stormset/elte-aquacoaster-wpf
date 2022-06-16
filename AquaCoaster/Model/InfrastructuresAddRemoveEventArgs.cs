using System;
using System.Collections.Generic;
using AquaCoaster.Model.Entities;
using AquaCoaster.Utilities;


namespace AquaCoaster.Model
{
    public class InfrastructuresAddRemoveEventArgs
    {
        public List<Tuple<Infrastructure, Point>> AddedInfrastructures;

        public InfrastructuresAddRemoveEventArgs(List<Tuple<Infrastructure, Point>> addedInfrastructures)
        {
            this.AddedInfrastructures = addedInfrastructures;
        }
    }
}
