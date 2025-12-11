using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitFeeds.Models
{
    [Table("Stops")]
    public class Stop
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("gtfs_stop_id")]
        [StringLength(50)]
        [Display(Name = "GTFS Stop ID")]
        public string GtfsStopId { get; set; } = null!;

        [Column("stop_code")]
        public string? StopCode { get; set; }

        [Column("stop_name")]
        public string StopName { get; set; } = null!;

        [Column("stop_desc")]
        public string? StopDesc { get; set; }

        [Column("stop_lat")]
        public decimal StopLat { get; set; }

        [Column("stop_lon")]
        public decimal StopLon { get; set; }

        [Column("zone_id")]
        public string? ZoneId { get; set; }

        [Column("stop_url")]
        public string? StopUrl { get; set; }

        [Column("location_type")]
        [Display(Name = "Location Type")]
        public byte? LocationType { get; set; }

        [Column("wheelchair_boarding")]
        [Display(Name = "Wheelchair Boarding")]
        public byte? WheelchairBoarding { get; set; }

        [Column("parent_station_id")]
        public int? ParentStationId { get; set; }

        [Column("stop_timezone")]
        public string? StopTimezone { get; set; }

        // Navigation properties
        [ForeignKey("ParentStationId")]
        public virtual Stop? ParentStation { get; set; }

        public virtual ICollection<StopTime> StopTimes { get; set; } = new List<StopTime>();
    }
}
