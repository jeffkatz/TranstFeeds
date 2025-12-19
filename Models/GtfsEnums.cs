using System.ComponentModel.DataAnnotations;

namespace TransitFeeds.Models
{
    public enum RouteType
    {
        [Display(Name = "Tram, Streetcar, Light rail")] Tram = 0,
        [Display(Name = "Subway, Metro")] Subway = 1,
        [Display(Name = "Rail")] Rail = 2,
        [Display(Name = "Bus")] Bus = 3,
        [Display(Name = "Ferry")] Ferry = 4,
        [Display(Name = "Cable Tram")] CableTram = 5,
        [Display(Name = "Aerial Lift")] AerialLift = 6,
        [Display(Name = "Funicular")] Funicular = 7,
        [Display(Name = "Trolleybus")] Trolleybus = 11,
        [Display(Name = "Monorail")] Monorail = 12
    }

    public enum LocationType
    {
        [Display(Name = "Stop/Platform")] Stop = 0,
        [Display(Name = "Station")] Station = 1,
        [Display(Name = "Entrance/Exit")] EntranceExit = 2,
        [Display(Name = "Generic Node")] GenericNode = 3,
        [Display(Name = "Boarding Area")] BoardingArea = 4
    }

    public enum WheelchairBoarding
    {
        [Display(Name = "No Information")] NoInfo = 0,
        [Display(Name = "Possible")] Possible = 1,
        [Display(Name = "Not Possible")] NotPossible = 2
    }

    public enum PickupDropOffType
    {
        [Display(Name = "Regularly Scheduled")] Regular = 0,
        [Display(Name = "None Available")] None = 1,
        [Display(Name = "Must Phone Agency")] Phone = 2,
        [Display(Name = "Must Coordinate with Driver")] Coordinate = 3
    }

    public enum DirectionId
    {
        [Display(Name = "Outbound (0)")] Outbound = 0,
        [Display(Name = "Inbound (1)")] Inbound = 1
    }

    public enum ContinuousStopping
    {
        [Display(Name = "Continuous")] Continuous = 0,
        [Display(Name = "No Continuous")] NoContinuous = 1,
        [Display(Name = "Must Phone")] Phone = 2,
        [Display(Name = "Must Coordinate")] Coordinate = 3
    }

    public enum TimepointType
    {
        [Display(Name = "Approximate")] Approximate = 0,
        [Display(Name = "Exact")] Exact = 1
    }

    public enum BikesAllowed
    {
        [Display(Name = "No Information")] NoInfo = 0,
        [Display(Name = "Allowed")] Allowed = 1,
        [Display(Name = "Not Allowed")] NotAllowed = 2
    }

    public enum ExceptionType
    {
        [Display(Name = "Added Service")] Added = 1,
        [Display(Name = "Removed Service")] Removed = 2
    }
}
