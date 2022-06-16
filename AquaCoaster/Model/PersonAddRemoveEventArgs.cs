using System;
using AquaCoaster.Model.Entities;
using AquaCoaster.Utilities;


namespace AquaCoaster.Model
{
    public class PersonAddRemoveEventArgs
    {
        public Person Person { get; }

        public Point? Coords { get; }

        public PersonAddRemoveEventArgs(Person person, Point? coords)
        {
            this.Person = person;
            this.Coords = coords;
        }
    }
}
