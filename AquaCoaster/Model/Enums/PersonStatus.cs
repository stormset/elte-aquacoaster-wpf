using System;


namespace AquaCoaster.Model.Enums
{
    public enum PersonStatus
    {
        IDLE,
        WALKING,
        PLAYING,
        EATING,
        CLEANING,
        REPAIRING,
        WAITING,
        LEAVING
    }

    public static class PersonStatusExtensions
    {
        public static string DisplayName(this PersonStatus me)
        {
            switch (me)
            {
                case PersonStatus.IDLE:
                    return "Idle";
                case PersonStatus.WALKING:
                    return "Walking";
                case PersonStatus.PLAYING:
                    return "Playing";
                case PersonStatus.EATING:
                    return "Eating";
                case PersonStatus.CLEANING:
                    return "Cleaning";
                case PersonStatus.REPAIRING:
                    return "Repairing";
                case PersonStatus.WAITING:
                    return "Waiting";
                case PersonStatus.LEAVING:
                    return "Leaving";
                default:
                    return "Unknown";
            }
        }
    }
}
