using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trungtamai.Models
{
    public class DiemSo
    {
        [Key]
        public int Id { get; set; }

        public int HocVienId { get; set; }
        public int LopHocId { get; set; }

        public string MaHocVien { get; set; } = "";
        public string HoTenHocVien { get; set; } = "";
        public string MaLop { get; set; } = "";
        public string TenLop { get; set; } = "";

        /// <summary>Điểm kiểm tra nhỏ 1 (0 - 10)</summary>
        [Column(TypeName = "decimal(4,1)")]
        public decimal? DiemKT1 { get; set; }

        /// <summary>Điểm kiểm tra nhỏ 2</summary>
        [Column(TypeName = "decimal(4,1)")]
        public decimal? DiemKT2 { get; set; }

        /// <summary>Điểm kiểm tra nhỏ 3</summary>
        [Column(TypeName = "decimal(4,1)")]
        public decimal? DiemKT3 { get; set; }

        /// <summary>Điểm kiểm tra lớn (đánh giá năng lực hoàn thành khóa học)</summary>
        [Column(TypeName = "decimal(4,1)")]
        public decimal? DiemKiemTraLon { get; set; }

        /// <summary>Điểm trung bình = (KT1 + KT2 + KT3 + KTL*2) / 5</summary>
        [NotMapped]
        public decimal? DiemTrungBinh
        {
            get
            {
                if (DiemKT1 == null && DiemKT2 == null && DiemKT3 == null && DiemKiemTraLon == null)
                    return null;

                decimal tong = 0;
                int heSo = 0;

                if (DiemKT1.HasValue) { tong += DiemKT1.Value; heSo += 1; }
                if (DiemKT2.HasValue) { tong += DiemKT2.Value; heSo += 1; }
                if (DiemKT3.HasValue) { tong += DiemKT3.Value; heSo += 1; }
                if (DiemKiemTraLon.HasValue) { tong += DiemKiemTraLon.Value * 2; heSo += 2; } // trọng số x2

                return heSo > 0 ? Math.Round(tong / heSo, 1) : null;
            }
        }

        /// <summary>
/// Kết quả: Đạt nếu điểm kiểm tra lớn >= 8.0
/// 3 bài KT nhỏ chỉ để theo dõi mức độ nắm kiến thức
/// </summary>
[NotMapped]
public string KetQua
{
    get
    {
        if (DiemKiemTraLon == null) return "Chưa thi";
        return DiemKiemTraLon >= 8.0m ? "Đạt" : "Không đạt";
    }
}

        public string? GhiChu { get; set; }
        public string GiaoVienNhap { get; set; } = "";
        public DateTime NgayCapNhat { get; set; } = DateTime.Now;
    }
}