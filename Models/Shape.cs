using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitFeeds.Models
{
    [Table("Shapes")]
    public class Shape
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("shape_id")]
        [Required(ErrorMessage = "Please select a Shape ID")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Shape ID")]
        public int ShapeId { get; set; } // FK to ShapesMaster.Id

        [Column("shape_pt_sequence")]
        [Display(Name = "Sequence")]
        [Required(ErrorMessage = "Sequence is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Sequence must be 0 or greater")]
        public int ShapePtSequence { get; set; }

        [Column("shape_pt_lat")]
        [Display(Name = "Latitude")]
        [Required(ErrorMessage = "Latitude is required")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public decimal ShapePtLat { get; set; }

        [Column("shape_pt_lon")]
        [Display(Name = "Longitude")]
        [Required(ErrorMessage = "Longitude is required")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public decimal ShapePtLon { get; set; }

        [Column("shape_dist_traveled")]
        [Display(Name = "Distance Traveled")]
        public decimal? ShapeDistTraveled { get; set; }

        // Navigation: reference to ShapesMaster
        [ForeignKey(nameof(ShapeId))]
        public virtual ShapesMaster? ShapesMaster { get; set; }
    }
}
