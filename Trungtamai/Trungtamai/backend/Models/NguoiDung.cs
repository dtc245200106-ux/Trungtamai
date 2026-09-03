using System.ComponentModel.DataAnnotations;

namespace Trungtamai.Models
{
    public class NguoiDung
    {
        public int Id { get; set; }

        [Required]
        public string TaiKhoan { get; set; } = "";

        [Required]
        public string MatKhau { get; set; } = "";

        [Required]
        public string HoTen { get; set; } = "";

        [Required]
        public string VaiTro { get; set; } = "";
    }
}