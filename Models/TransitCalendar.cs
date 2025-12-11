using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitFeeds.Models
{
    [Table("TransitCalendar")]
    public class TransitCalendar
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("gtfs_service_id")]
        [StringLength(50)]
        [Display(Name = "GTFS Service ID")]
        public string GtfsServiceId { get; set; } = null!;

        [Column("start_date", TypeName = "date")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Column("end_date", TypeName = "date")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Column("monday")]
        [Display(Name = "Monday")]
        public bool Monday { get; set; }

        [Column("tuesday")]
        [Display(Name = "Tuesday")]
        public bool Tuesday { get; set; }

        [Column("wednesday")]
        [Display(Name = "Wednesday")]
        public bool Wednesday { get; set; }

        [Column("thursday")]
        [Display(Name = "Thursday")]
        public bool Thursday { get; set; }

        [Column("friday")]
        [Display(Name = "Friday")]
        public bool Friday { get; set; }

        [Column("saturday")]
        [Display(Name = "Saturday")]
        public bool Saturday { get; set; }

        [Column("sunday")]
        [Display(Name = "Sunday")]
        public bool Sunday { get; set; }

        // Navigation: trips that use this service
        public virtual ICollection<Trip> Trips { get; set; } = new HashSet<Trip>();
    }
}
