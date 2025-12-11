using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitFeeds.Models
{
    [Table("ShapesMaster")]
    public class ShapesMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("gtfs_shape_id")]
        [StringLength(50)]
        [Display(Name = "GTFS Shape ID")]
        public string GtfsShapeId { get; set; } = null!;

        // Navigation: multiple points for this shape
        public virtual ICollection<Shape> Shapes { get; set; } = new HashSet<Shape>();

        // Optional: Trips using this shape
        public virtual ICollection<Trip> Trips { get; set; } = new HashSet<Trip>();
    }
}
