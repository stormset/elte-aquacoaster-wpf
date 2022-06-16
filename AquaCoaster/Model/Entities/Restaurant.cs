using System;
using System.Collections.Generic;
using AquaCoaster.Model.Enums;
using System.Linq;

namespace AquaCoaster.Model.Entities
{
    public abstract class Restaurant : Facility
    {
        private Int32 _countTime = 0;       // "inner clock"
        private Random rand = new Random();

        protected Restaurant(Dictionary<String, Object> config) : base(config) { }

        /// <summary>
        /// Handles the time based behaviour of a <see cref="Restaurant"/> element.
        /// </summary>
        /// <param name="passedMillis"></param>
        public override void Tick(Int64 passedMillis)
        {
            // Continous expenses
            OnExpenseGenerated(-ContinuousExpense);

            // Finishing the construction
            if (Status == InfrastructureStatus.UNDER_CONSTRUCTION && _countTime == 10)
            {
                Status = InfrastructureStatus.WAITING;
                _countTime = 0;
            }

            // Counting the passing time until the round ends
            if (Status == InfrastructureStatus.OPERATING && _countTime >= RoundTime)
            {
                //TODO: change the status of the visitors and place them on road
                foreach (Visitor v in CurrentUsers)
                {
                    v.Status = PersonStatus.IDLE;
                    v.Money -= UseFee;
                    v.Food += FoodFactor;
                    if (v.Food > 1f)
                    {
                        v.Food = 1f;
                    }
                    OnExpenseGenerated(UseFee);
                }
                OnInteractionEnded(new List<Person>(CurrentUsers));
                OnExpenseGenerated(-PerUseExpense);

                CurrentUsers.Clear();
                Status = InfrastructureStatus.WAITING;
                _countTime = 0;

            }

            // Starting new round if the minimum capacity is reached
            if (Status == InfrastructureStatus.WAITING && WaitingQueue.Count >= MinimumCapacity)
            {
                Status = InfrastructureStatus.OPERATING;
                _countTime = 0;

                List<Person> finishedWaiting = new List<Person>();
                while (WaitingQueue.Count != 0 && CurrentUsers.Count < Capacity)
                {
                    Visitor v = WaitingQueue.Dequeue();
                    CurrentUsers.Add(v);
                    finishedWaiting.Add(v);
                    v.Status = PersonStatus.EATING;
                }

                OnWaitingEnded(finishedWaiting);
            }

            // TODO: Implementing break downs and repairs
            if (Status == InfrastructureStatus.WAITING || Status == InfrastructureStatus.OPERATING)
            {
                if (rand.Next(0, 5000) == rand.Next(0, 5000))
                {
                    Status = InfrastructureStatus.FAULTY;
                    foreach (Visitor v in WaitingQueue)
                    {
                        v.Status = PersonStatus.IDLE;
                    }
                    foreach (Visitor v in CurrentUsers)
                    {
                        v.Status = PersonStatus.IDLE;
                    }
                    OnInteractionEnded(new List<Person>(WaitingQueue));
                    WaitingQueue.Clear();
                    OnInteractionEnded(new List<Person>(CurrentUsers));
                    CurrentUsers.Clear();
                }
            }
            _countTime++;
            OnAttributesChanged();
        }

        /// <summary>
        /// Handles the acceptation of a <see cref="Person"/> by a <see cref="Game"/> element.
        /// </summary>
        /// <param name="visitor"></param>
        public override void Accept(Person person)
        {
            if (Status is InfrastructureStatus.FAULTY)
            {
                person.Status = PersonStatus.IDLE;
            }
            else
            {
                person.VisitRestaurant(this);
            }
        }

        /// <summary>
        /// Handles the serving of a customer by a <see cref="Game"/>.
        /// </summary>
        /// <param name="visitor"></param>
        public override void ServeVisitor(Visitor visitor)
        {
            WaitingQueue.Enqueue(visitor);
            visitor.Status = PersonStatus.WAITING;
        }

        public override void LeaveFacility(Visitor visitor)
        {
            WaitingQueue = new Queue<Visitor>(WaitingQueue.Where(x => x != visitor));
            CurrentUsers = new List<Visitor>(CurrentUsers.Where(x => x != visitor));

            OnInteractionEnded(new List<Person> { visitor });
        }
    }

}
