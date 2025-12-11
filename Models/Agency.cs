using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitFeeds.Models
{
    [Table("Agencies")]
    public class Agency
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("gtfs_agency_id")]
        [Display(Name = "GTFS Agency ID")]
        public string? GtfsAgencyId { get; set; }

        [Required]
        [Column("agency_name")]
        [StringLength(255)]
        [Display(Name = "Name")]
        public string AgencyName { get; set; } = null!;

        [Required]
        [Column("agency_url")]
        [StringLength(255)]
        [Display(Name = "URL")]
        public string AgencyUrl { get; set; } = null!;

        [Required]
        [Column("agency_timezone")]
        [StringLength(100)]
        [Display(Name = "Timezone")]
        public string AgencyTimezone { get; set; } = null!;

        [Column("agency_phone")]
        [StringLength(50)]
        [Display(Name = "Phone")]
        public string? AgencyPhone { get; set; }

        [Column("agency_lang")]
        [StringLength(10)]
        [Display(Name = "Language")]
        public string? AgencyLang { get; set; }

        public virtual ICollection<TransitRoute> TransitRoutes { get; set; } = new HashSet<TransitRoute>();
    }
}
