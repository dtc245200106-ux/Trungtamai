using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trungtamai.Models
{
    public class HocVien
    {
        [Key]
        public int Id { get; set; }

        public string MaHocVien { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string KhoaHoc { get; set; } = string.Empty;
        public string LopHoc { get; set; } = string.Empty;
        public string NgonNgu { get; set; } = string.Empty;
        public string TrangThai { get; set; } = "Đang học";

        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTienPhaiDong { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTienDaDong { get; set; }

        public DateTime? NgayDongCuoi { get; set; }

        [NotMapped]
        public decimal ConNo => Math.Max(0, SoTienPhaiDong - SoTienDaDong);

                [NotMapped]
        public string TrangThaiHocPhi
        {
            get
            {
                if (SoTienDaDong >= SoTienPhaiDong && SoTienPhaiDong > 0)
                    return "Đã đóng";
                if (SoTienDaDong > 0)
                    return "Chưa đủ";
                return "Chưa đóng";
            }
        }
    }
}