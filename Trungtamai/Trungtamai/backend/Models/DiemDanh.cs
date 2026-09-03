using System.ComponentModel.DataAnnotations;

namespace Trungtamai.Models
{
    public class DiemDanh
    {
        [Key]
        public int Id { get; set; }

        public int LopHocId { get; set; }          // Id của lớp
        public string MaLop { get; set; } = "";     // để dễ tra cứu
        public string TenLop { get; set; } = "";

        public int HocVienId { get; set; }
        public string MaHocVien { get; set; } = "";
        public string HoTenHocVien { get; set; } = "";

        public DateTime NgayHoc { get; set; }       // Ngày điểm danh

        /// <summary>
        /// Có mặt | Vắng | Muộn | Có phép
        /// </summary>
        public string TrangThai { get; set; } = "Có mặt";

        public string? GhiChu { get; set; }

        public string GiaoVienDiemDanh { get; set; } = "";
        public DateTime ThoiGianDiemDanh { get; set; } = DateTime.Now;
    }
}