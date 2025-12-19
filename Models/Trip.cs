using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitFeeds.Models
{
    [Table("Trips")]
    public class Trip
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("gtfs_trip_id")]
        [StringLength(50)]
        [Display(Name = "GTFS Trip ID")]
        public string GtfsTripId { get; set; } = null!;

        [Column("transit_route_id")]
        [Display(Name = "Route")]
        public int TransitRouteId { get; set; } // FK to TransitRoute.Id

        [Column("service_id")]
        [Display(Name = "Service")]
        public int ServiceId { get; set; } // FK to TransitCalendar.Id

        [Column("shape_id")]
        [Display(Name = "Shape")]
        public int? ShapeId { get; set; } // FK to ShapesMaster.Id

        [Column("trip_headsign")]
        [StringLength(255)]
        [Display(Name = "Headsign")]
        public string? TripHeadsign { get; set; }

        [Column("trip_short_name")]
        [StringLength(50)]
        [Display(Name = "Short Name")]
        public string? TripShortName { get; set; }

        [Column("direction_id")]
        [Display(Name = "Direction")]
        public DirectionId? DirectionId { get; set; }

        [Column("wheelchair_accessible")]
        [Display(Name = "Wheelchair Accessible")]
        public WheelchairBoarding? WheelchairAccessible { get; set; }

        [Column("bikes_allowed")]
        [Display(Name = "Bikes Allowed")]
        public BikesAllowed? BikesAllowed { get; set; }

        [Column("block_id")]
        [StringLength(50)]
        [Display(Name = "Block ID")]
        public string? BlockId { get; set; }

        // Navigation properties
        [ForeignKey("TransitRouteId")]
        public virtual TransitRoute? TransitRoute { get; set; }

        [ForeignKey("ServiceId")]
        public virtual TransitCalendar? TransitCalendar { get; set; }

        [ForeignKey("ShapeId")]
        public virtual ShapesMaster? ShapesMaster { get; set; }

        public virtual ICollection<StopTime> StopTimes { get; set; } = new HashSet<StopTime>();
    }
}
