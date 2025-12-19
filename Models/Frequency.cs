using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitFeeds.Models
{
    [Table("Frequencies")]
    public class Frequency
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("trip_id")]
        public int TripId { get; set; }

        [Required]
        [Column("start_time")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Column("end_time")]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Column("headway_secs")]
        public int HeadwaySecs { get; set; }

        [Column("exact_times")]
        public byte? ExactTimes { get; set; } // 0: frequency-based, 1: schedule-based

        [ForeignKey("TripId")]
        public virtual Trip? Trip { get; set; }
    }
}
