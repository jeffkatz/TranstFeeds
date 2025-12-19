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

        [Column("tts_stop_name")]
        [Display(Name = "TTS Stop Name")]
        public string? TtsStopName { get; set; }

        [Column("platform_code")]
        [Display(Name = "Platform Code")]
        public string? PlatformCode { get; set; }

        [Column("stop_lat")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public decimal StopLat { get; set; }

        [Column("stop_lon")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public decimal StopLon { get; set; }

        [Column("zone_id")]
        public string? ZoneId { get; set; }

        [Column("stop_url")]
        public string? StopUrl { get; set; }

        [Column("location_type")]
        [Display(Name = "Location Type")]
        public LocationType? LocationType { get; set; }

        [Column("wheelchair_boarding")]
        [Display(Name = "Wheelchair Boarding")]
        public WheelchairBoarding? WheelchairBoarding { get; set; }

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
