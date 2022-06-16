using System;


namespace AquaCoaster.Model.Enums
{
    public enum InfrastructureStatus
    {
        OPERATING,
        WAITING,
        UNDER_CONSTRUCTION,
        FAULTY,
        UNDER_REPAIR
    }

    public static class InfrastructureStatusExtensions
    {
        public static string DisplayName(this InfrastructureStatus me)
        {
            switch (me)
            {
                case InfrastructureStatus.OPERATING:
                    return "Operating";
                case InfrastructureStatus.WAITING:
                    return "Waiting";
                case InfrastructureStatus.UNDER_CONSTRUCTION:
                    return "Building";
                case InfrastructureStatus.FAULTY:
                    return "Faulty";
                case InfrastructureStatus.UNDER_REPAIR:
                    return "Repairing";
                default:
                    return "Unknown";
            }
        }
    }
}
