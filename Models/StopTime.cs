using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitFeeds.Models
{
    [Table("StopTimes")]
    public class StopTime
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("trip_id")]
        [Display(Name = "Trip")]
        public int TripId { get; set; } // FK to Trip.Id

        [Column("stop_id")]
        [Display(Name = "Stop")]
        public int StopId { get; set; } // FK to Stop.Id

        [Column("stop_sequence")]
        [Display(Name = "Stop Sequence")]
        public int StopSequence { get; set; }

        [Column("arrival_time")]
        [Display(Name = "Arrival Time")]
        public TimeSpan? ArrivalTime { get; set; }

        [Column("departure_time")]
        [Display(Name = "Departure Time")]
        public TimeSpan? DepartureTime { get; set; }

        [Column("stop_headsign")]
        [StringLength(255)]
        [Display(Name = "Stop Headsign")]
        public string? StopHeadsign { get; set; }

        [Column("pickup_type")]
        [Display(Name = "Pickup Type")]
        public PickupDropOffType? PickupType { get; set; }

        [Column("drop_off_type")]
        [Display(Name = "Drop Off Type")]
        public PickupDropOffType? DropOffType { get; set; }

        [Column("shape_dist_traveled")]
        [Display(Name = "Shape Distance Traveled")]
        public double? ShapeDistTraveled { get; set; }

        [Column("timepoint")]
        [Display(Name = "Timepoint")]
        public TimepointType? Timepoint { get; set; }

        [Column("continuous_pickup")]
        [Display(Name = "Continuous Pickup")]
        public ContinuousStopping? ContinuousPickup { get; set; }

        [Column("continuous_drop_off")]
        [Display(Name = "Continuous Drop-off")]
        public ContinuousStopping? ContinuousDropOff { get; set; }

        // Navigation properties
        [ForeignKey("TripId")]
        public virtual Trip? Trip { get; set; }

        [ForeignKey("StopId")]
        public virtual Stop? Stop { get; set; }
    }
}
