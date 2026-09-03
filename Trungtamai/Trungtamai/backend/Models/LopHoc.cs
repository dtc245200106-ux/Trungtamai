using System.ComponentModel.DataAnnotations;

namespace Trungtamai.Models
{
    public class LopHoc
    {
        [Key]
        public int Id { get; set; }

        public string MaLop { get; set; } = string.Empty;
        public string TenLop { get; set; } = string.Empty;
        public string KhoaHoc { get; set; } = string.Empty;
        public string NgonNgu { get; set; } = string.Empty;
        public string GiaoVien { get; set; } = string.Empty;

        public int SiSoToiDa { get; set; }
        public int SiSoHienTai { get; set; }

        /// <summary>Các thứ học, cách nhau bằng dấu phẩy. Ví dụ: T2,T4,T6</summary>
        public string NgayHoc { get; set; } = string.Empty;

        /// <summary>Giờ bắt đầu HH:mm (ví dụ 18:00)</summary>
        public string GioBatDau { get; set; } = string.Empty;

        /// <summary>Giờ kết thúc HH:mm (ví dụ 19:30)</summary>
        public string GioKetThuc { get; set; } = string.Empty;

        /// <summary>Phòng học: P01 → P20</summary>
        public string PhongHoc { get; set; } = string.Empty;

        /// <summary>Chuỗi hiển thị: T2-T4, 18:00-20:00, P05</summary>
        public string CaHoc { get; set; } = string.Empty;

        public string TrangThai { get; set; } = "Sắp khai giảng";
    }
}