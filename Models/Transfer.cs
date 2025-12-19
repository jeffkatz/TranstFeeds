using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitFeeds.Models
{
    [Table("Transfers")]
    public class Transfer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("from_stop_id")]
        public int FromStopId { get; set; }

        [Required]
        [Column("to_stop_id")]
        public int ToStopId { get; set; }

        [Required]
        [Column("transfer_type")]
        public byte TransferType { get; set; } // 0: recommended, 1: timed, 2: min time, 3: not possible

        [Column("min_transfer_time")]
        public int? MinTransferTime { get; set; }

        [ForeignKey("FromStopId")]
        public virtual Stop? FromStop { get; set; }

        [ForeignKey("ToStopId")]
        public virtual Stop? ToStop { get; set; }
    }
}
