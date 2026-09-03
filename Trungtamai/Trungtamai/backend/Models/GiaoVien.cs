using System.ComponentModel.DataAnnotations;

namespace Trungtamai.Models
{
    public class GiaoVien
    {
        [Key]
        public int Id { get; set; }

        public string MaGiaoVien { get; set; } = string.Empty;

        public string HoTen { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string SoDienThoai { get; set; } = string.Empty;

        public string NgonNgu { get; set; } = string.Empty;

        public string ChuyenMon { get; set; } = string.Empty;

        public int KinhNghiem { get; set; }
    }
}