using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitFeeds.Models
{
    [Table("TransitRoutes")]
    public class TransitRoute
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("gtfs_route_id")]
        [StringLength(50)]
        [Display(Name = "GTFS Route ID")]
        public string GtfsRouteId { get; set; } = null!;

        [Column("agency_id")]
        [Display(Name = "Agency")]
        public int? AgencyId { get; set; }

        [Column("route_short_name")]
        [StringLength(50)]
        [Display(Name = "Short Name")]
        public string? RouteShortName { get; set; }

        [Column("route_long_name")]
        [StringLength(255)]
        [Display(Name = "Long Name")]
        public string? RouteLongName { get; set; }

        [Column("route_type")]
        [Display(Name = "Type")]
        public int? RouteType { get; set; }

        [Column("route_text_color")]
        [StringLength(20)]
        [Display(Name = "Text Color")]
        public string? RouteTextColor { get; set; }

        [Column("route_color")]
        [StringLength(20)]
        [Display(Name = "Route Color")]
        public string? RouteColor { get; set; }

        [Column("route_url")]
        [StringLength(255)]
        [Display(Name = "URL")]
        public string? RouteUrl { get; set; }

        [Column("route_desc")]
        [StringLength(500)]
        [Display(Name = "Description")]
        public string? RouteDesc { get; set; }

        // Navigation: Agency
        [ForeignKey("AgencyId")]
        [InverseProperty("TransitRoutes")]
        public virtual Agency? Agency { get; set; }

        // Navigation: Trips
        public virtual ICollection<Trip> Trips { get; set; } = new HashSet<Trip>();
    }
}
