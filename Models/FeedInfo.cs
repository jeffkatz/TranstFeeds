using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitFeeds.Models
{
    [Table("FeedInfo")]
    public class FeedInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("feed_publisher_name")]
        [StringLength(255)]
        [Display(Name = "Publisher Name")]
        public string FeedPublisherName { get; set; } = null!;

        [Required]
        [Column("feed_publisher_url")]
        [StringLength(255)]
        [Display(Name = "Publisher URL")]
        public string FeedPublisherUrl { get; set; } = null!;

        [Required]
        [Column("feed_lang")]
        [StringLength(50)]
        [Display(Name = "Language")]
        public string FeedLang { get; set; } = null!;

        [Column("feed_start_date", TypeName = "date")]
        [Display(Name = "Start Date")]
        public DateTime? FeedStartDate { get; set; }

        [Column("feed_end_date", TypeName = "date")]
        [Display(Name = "End Date")]
        public DateTime? FeedEndDate { get; set; }

        [Column("feed_version")]
        [StringLength(100)]
        [Display(Name = "Version")]
        public string? FeedVersion { get; set; }

        [Column("feed_contact_email")]
        [StringLength(255)]
        [Display(Name = "Contact Email")]
        public string? FeedContactEmail { get; set; }

        [Column("feed_contact_url")]
        [StringLength(255)]
        [Display(Name = "Contact URL")]
        public string? FeedContactUrl { get; set; }
    }
}
