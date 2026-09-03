using System.ComponentModel.DataAnnotations;

namespace Trungtamai.Models
{
    public class TuVanVien
    {
        [Key]
        public int Id { get; set; }

        public string MaTuVanVien { get; set; } = string.Empty;

        public string HoTen { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string SoDienThoai { get; set; } = string.Empty;

        // Nhóm khóa học / mảng phụ trách tư vấn (VD: "Anh văn giao tiếp, TOEIC")
        public string LinhVucPhuTrach { get; set; } = string.Empty;

        public int KinhNghiem { get; set; }
    }
}
