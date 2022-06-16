using System;
using System.Collections.Generic;
using System.Linq;
using AquaCoaster.Model.Entities;
using AquaCoaster.Utilities;


namespace AquaCoaster.Persistence
{
    public struct PersonLocator
    {

        public Person Person { get; set; }

        public Point Coords { get; set; }

        public Infrastructure Infrastructure { get; set; }

        public PersonLocator(Person person, Point coords, Infrastructure infrastructure = null)
        {
            Person = person;
            Coords = coords;
            Infrastructure = infrastructure;
        }

    }

    public class PersonList
    {
        private List<PersonLocator> _people;

        public PersonList()
        {
            _people = new List<PersonLocator>();
        }

        public IEnumerable<PersonLocator> PersonLocators { get { return _people.Select(o => o); } }

        public Int32 Count { get => _people.Count; }

        /// <summary>
        /// Returns the people at a given coordinate.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public IEnumerable<Person> this[Int32 x, Int32 y] => _people.FindAll(p => p.Coords.Equals(new Point(x, y))).Select(p => p.Person);

        /// <summary></summary>
        /// <param name="person"></param>
        public Point CoordinatesOf(Person person)
        {
            Int32 ind = _people.FindIndex(p => Equals(p.Person, person));

            if (ind == -1)
            {
                throw new InvalidOperationException("The passed person object isn't in the list.");
            }

            return _people[ind].Coords;
        }

        /// <summary></summary>
        /// <param name="person"></param>
        public Point? CoordinatesOfOrNull(Person person)
        {
            Int32 ind = _people.FindIndex(p => Equals(p.Person, person));

            if (ind == -1)
            {
                return null;
            }

            return _people[ind].Coords;
        }

        /// <summary></summary>
        /// <param name="person"></param>
        /// <param name="coords"></param>
        public void AddPerson(Person person, Point coords)
        {
            _people.Add(new PersonLocator(person, coords));
        }

        /// <summary></summary>
        /// <param name="person"></param>
        public void RemovePerson(Person person)
        {
            _people.RemoveAll(p => Equals(p.Person, person));
        }

        public Infrastructure InfrastructureOfPerson(Person person)
        {
            return _people.Find(p => Equals(p.Person, person)).Infrastructure;
        }

        public void SetCoordinates(Person person, Point coords)
        {
            Int32 ind = _people.FindIndex(p => Equals(p.Person, person));

            if (ind == -1)
            {
                throw new InvalidOperationException("The passed person object isn't in the list.");
            }

            _people[ind] = new PersonLocator(person, coords);
        }

        public void SetInfrastructure(Person person, Infrastructure infrastructure)
        {
            Int32 ind = _people.FindIndex(p => Equals(p.Person, person));

            if (ind != -1)
            {
                _people[ind] = new PersonLocator(person, CoordinatesOf(person), infrastructure);
            }
        }
    }
}
