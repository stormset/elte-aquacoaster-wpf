using System;
using AquaCoaster.Model.Entities;
using AquaCoaster.Utilities;


namespace AquaCoaster.Model
{
    public class PersonMovedEventArgs
    {
        public Person Person { get; }

        public Point From { get; }

        public Point To { get; }

        public PersonMovedEventArgs(Person person, Point from, Point to)
        {
            this.Person = person;
            this.From = from;
            this.To = to;
        }
    }
}
