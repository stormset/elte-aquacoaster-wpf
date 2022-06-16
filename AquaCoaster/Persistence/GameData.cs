using System;
using AquaCoaster.Model.Enums;


namespace AquaCoaster.Persistence
{
    public class GameData
    {

        public String Name { get; set; }

        public Int32 Rows { get; set; }

        public Int32 Columns { get; set; }

        public Int32 Budget { get; set; }

        public Int32 GameTime { get; set; }

        public Int32 TimeInterval { get; set; }

        public ParkStatus ParkStatus { get; set; }

        public PersonList People { get; set; }

        public InfrastructureGraph Infrastructure { get; set; }

        public GameData(String name, Int32 rows, Int32 columns, Int32 budget, Int32 gameTime, Int32 timeInterval, ParkStatus parkStatus)
        {
            this.Name = name;
            this.Rows = rows;
            this.Columns = columns;
            this.GameTime = gameTime;
            this.TimeInterval = timeInterval;
            this.Budget = budget;
            this.ParkStatus = parkStatus;
            this.People = new PersonList();
            this.Infrastructure = new InfrastructureGraph();
        }

    }
}
