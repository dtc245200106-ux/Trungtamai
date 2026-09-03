using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trungtamai.Models
{
    public class KhoaHoc
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string TenKhoaHoc { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string NgonNgu { get; set; } = "";

        public int SoBuoi { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Gia { get; set; }

        public string MoTa { get; set; } = "";
    }
}