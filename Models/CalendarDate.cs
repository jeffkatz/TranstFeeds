using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitFeeds.Models
{
    [Table("CalendarDates")]
    public class CalendarDate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("gtfs_service_id")]
        [StringLength(50)]
        [Display(Name = "Service ID")]
        public string GtfsServiceId { get; set; } = null!;

        [Required]
        [Column("date", TypeName = "date")]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [Column("exception_type")]
        [Display(Name = "Exception Type")]
        public ExceptionType ExceptionType { get; set; } // Added or Removed

        // Optional: linking to TransitCalendar internal Id if it exists
        [Column("transit_calendar_id")]
        public int? TransitCalendarId { get; set; }

        [ForeignKey("TransitCalendarId")]
        public virtual TransitCalendar? TransitCalendar { get; set; }
    }
}
