using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trungtamai.Attributes;
using Trungtamai.Data;
using Trungtamai.Models;

namespace Trungtamai.Controllers
{
    [AuthorizeRole("GiaoVien")]
    public class GiaoVienController : Controller
    {
        private readonly AppDbContext _context;

        public GiaoVienController(AppDbContext context)
        {
            _context = context;
        }
// =============================
// ĐIỂM SỐ & KẾT QUẢ
// =============================
public async Task<IActionResult> DiemSoKetQua()
{
    ViewBag.HoTen = HttpContext.Session.GetString("HoTen") ?? "";

    var giaoVien = await LayGiaoVienHienTaiAsync();
    if (giaoVien == null)
    {
        ViewBag.CanhBao = "Không tìm thấy hồ sơ giáo viên.";
        return View(new List<LopHoc>());
    }

    var dsLop = await _context.LopHocs
        .Where(x => x.GiaoVien == giaoVien.HoTen)
        .OrderByDescending(x => x.TrangThai == "Đang học")
        .ThenBy(x => x.TenLop)
        .ToListAsync();

    ViewBag.TongLop = dsLop.Count;
    return View(dsLop);
}

// Lấy danh sách điểm của 1 lớp (JSON)
[HttpGet]
public async Task<IActionResult> LayDiemTheoLop(int lopId)
{
    var giaoVien = await LayGiaoVienHienTaiAsync();
    var lop = await _context.LopHocs.FirstOrDefaultAsync(x => x.Id == lopId);

    if (lop == null || giaoVien == null || lop.GiaoVien != giaoVien.HoTen)
        return NotFound();

    var hocViens = await _context.HocViens
        .Where(x => x.LopHoc == lop.TenLop)
        .OrderBy(x => x.HoTen)
        .ToListAsync();

    var diemSos = await _context.DiemSos
        .Where(x => x.LopHocId == lopId)
        .ToListAsync();

    var ketQua = hocViens.Select(hv =>
    {
        var ds = diemSos.FirstOrDefault(x => x.HocVienId == hv.Id);
        return new
        {
            hocVienId = hv.Id,
            maHocVien = hv.MaHocVien,
            hoTen = hv.HoTen,
            diemKT1 = ds?.DiemKT1,
            diemKT2 = ds?.DiemKT2,
            diemKT3 = ds?.DiemKT3,
            diemKiemTraLon = ds?.DiemKiemTraLon,
            diemTrungBinh = ds?.DiemTrungBinh,
            ketQua = ds?.KetQua ?? "Chưa thi",
            ghiChu = ds?.GhiChu ?? ""
        };
    }).ToList();

    return Json(new
    {
        lop = new { lop.Id, lop.MaLop, lop.TenLop },
        danhSach = ketQua
    });
}

// Lưu điểm
[HttpPost]
public async Task<IActionResult> LuuDiemSo([FromBody] LuuDiemSoRequest request)
{
    var giaoVien = await LayGiaoVienHienTaiAsync();
    var lop = await _context.LopHocs.FirstOrDefaultAsync(x => x.Id == request.LopId);

    if (lop == null || giaoVien == null || lop.GiaoVien != giaoVien.HoTen)
        return NotFound();

    foreach (var item in request.DanhSach)
    {
        var existing = await _context.DiemSos
            .FirstOrDefaultAsync(x => x.LopHocId == request.LopId && x.HocVienId == item.HocVienId);

        if (existing != null)
        {
            existing.DiemKT1 = item.DiemKT1;
            existing.DiemKT2 = item.DiemKT2;
            existing.DiemKT3 = item.DiemKT3;
            existing.DiemKiemTraLon = item.DiemKiemTraLon;
            existing.GhiChu = item.GhiChu;
            existing.GiaoVienNhap = giaoVien.HoTen;
            existing.NgayCapNhat = DateTime.Now;
        }
        else
        {
            var hv = await _context.HocViens.FindAsync(item.HocVienId);
            if (hv == null) continue;

            _context.DiemSos.Add(new DiemSo
            {
                HocVienId = item.HocVienId,
                LopHocId = request.LopId,
                MaHocVien = hv.MaHocVien,
                HoTenHocVien = hv.HoTen,
                MaLop = lop.MaLop,
                TenLop = lop.TenLop,
                DiemKT1 = item.DiemKT1,
                DiemKT2 = item.DiemKT2,
                DiemKT3 = item.DiemKT3,
                DiemKiemTraLon = item.DiemKiemTraLon,
                GhiChu = item.GhiChu,
                GiaoVienNhap = giaoVien.HoTen,
                NgayCapNhat = DateTime.Now
            });
        }

        // ===== TỰ ĐỘNG CẬP NHẬT TRẠNG THÁI HỌC VIÊN =====
        if (item.DiemKiemTraLon.HasValue && item.DiemKiemTraLon.Value >= 8.0m)
        {
            var hocVien = await _context.HocViens.FindAsync(item.HocVienId);
            if (hocVien != null && hocVien.TrangThai != "Đã kết thúc")
            {
                hocVien.TrangThai = "Đã kết thúc";
            }
        }
    }

    await _context.SaveChangesAsync();
    return Ok(new { success = true, message = "Đã lưu điểm thành công" });
}
        // =============================
        // XÁC ĐỊNH GIÁO VIÊN ĐANG ĐĂNG NHẬP
        // =============================
        private async Task<GiaoVien?> LayGiaoVienHienTaiAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return null;

            var nguoiDung = await _context.NguoiDungs.FindAsync(userId);
            if (nguoiDung == null)
                return null;

            var giaoVien = await _context.GiaoViens
                .FirstOrDefaultAsync(x => x.Email == nguoiDung.TaiKhoan);

            giaoVien ??= await _context.GiaoViens
                .FirstOrDefaultAsync(x => x.MaGiaoVien == nguoiDung.TaiKhoan);

            giaoVien ??= await _context.GiaoViens
                .FirstOrDefaultAsync(x => x.HoTen == nguoiDung.HoTen);

            return giaoVien;
        }

        // =============================
        // TRANG TỔNG QUAN
        // =============================
        public async Task<IActionResult> Index()
        {
            var hoTen = HttpContext.Session.GetString("HoTen") ?? "";
            ViewBag.HoTen = hoTen;

            var giaoVien = await LayGiaoVienHienTaiAsync();
            List<LopHoc> lopCuaToi = new();

            if (giaoVien != null)
            {
                lopCuaToi = await _context.LopHocs
                    .Where(x => x.GiaoVien == giaoVien.HoTen)
                    .OrderBy(x => x.MaLop)
                    .ToListAsync();
            }

            ViewBag.SoLop = lopCuaToi.Count;
            ViewBag.TongHocVien = lopCuaToi.Sum(x => x.SiSoHienTai);

            var thuHomNay = DateTime.Now.DayOfWeek switch
            {
                DayOfWeek.Monday => "T2",
                DayOfWeek.Tuesday => "T3",
                DayOfWeek.Wednesday => "T4",
                DayOfWeek.Thursday => "T5",
                DayOfWeek.Friday => "T6",
                DayOfWeek.Saturday => "T7",
                _ => "CN"
            };

            var lichHomNay = lopCuaToi
    .Where(x =>
        x.TrangThai != "Đã kết thúc"
        && !string.IsNullOrWhiteSpace(x.NgayHoc)
        && x.NgayHoc
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Any(thu => thu.Trim() == thuHomNay)
    )
    .OrderBy(x => x.GioBatDau)
    .ToList();

            ViewBag.LichHomNay = lichHomNay;
            ViewBag.SoBuoiHomNay = lichHomNay.Count;
            var thongKeBuoi = await LayThongKeBuoiHocAsync(lopCuaToi);
            ViewBag.SoBuoiDaHoc = thongKeBuoi.DaHoc;
            ViewBag.TongSoBuoi = thongKeBuoi.Tong;

            return View(lopCuaToi);
        }
        
// =============================
// LỊCH DẠY CỦA TÔI
// =============================
public async Task<IActionResult> LichDay()
{
    ViewBag.HoTen = HttpContext.Session.GetString("HoTen") ?? "";

    var giaoVien = await LayGiaoVienHienTaiAsync();

    if (giaoVien == null)
    {
        ViewBag.CanhBao =
            "Không tìm thấy hồ sơ giáo viên gắn với tài khoản này.";

        return View(new List<LopHoc>());
    }

    // Chỉ lấy các lớp mà giáo viên hiện tại phụ trách
    var dsLop = await _context.LopHocs
        .Where(x => x.GiaoVien == giaoVien.HoTen)
        .OrderBy(x => x.GioBatDau)
        .ThenBy(x => x.TenLop)
        .ToListAsync();

    ViewBag.TongLop = dsLop.Count;
    var thongKeBuoi = await LayThongKeBuoiHocAsync(dsLop);
    ViewBag.SoBuoiDaHoc = thongKeBuoi.DaHoc;
    ViewBag.TongSoBuoi = thongKeBuoi.Tong;

    return View(dsLop);
}
        // =============================
        // LỚP HỌC CỦA TÔI
        // =============================
        public async Task<IActionResult> LopHocCuaToi()
        {
            ViewBag.HoTen = HttpContext.Session.GetString("HoTen");

            var giaoVien = await LayGiaoVienHienTaiAsync();
            List<LopHoc> dsLop;

            if (giaoVien == null)
            {
                dsLop = new List<LopHoc>();
                ViewBag.CanhBao = "Không tìm thấy hồ sơ giáo viên gắn với tài khoản này. Vui lòng liên hệ quản lý.";
            }
            else
            {
                dsLop = await _context.LopHocs
                    .Where(x => x.GiaoVien == giaoVien.HoTen)
                    .OrderByDescending(x => x.TrangThai == "Đang học")
                    .ThenBy(x => x.TenLop)
                    .ToListAsync();
            }

            ViewBag.TongLop = dsLop.Count;
            ViewBag.DangHoc = dsLop.Count(x => x.TrangThai == "Đang học");
            ViewBag.SapKhaiGiang = dsLop.Count(x => x.TrangThai == "Sắp khai giảng");
            ViewBag.TongHocVien = dsLop.Sum(x => x.SiSoHienTai);
            var thongKeBuoi = await LayThongKeBuoiHocAsync(dsLop);
            ViewBag.SoBuoiDaHoc = thongKeBuoi.DaHoc;
            ViewBag.TongSoBuoi = thongKeBuoi.Tong;

            return View(dsLop);
        }

        private async Task<(Dictionary<int, int> DaHoc, Dictionary<int, int> Tong)> LayThongKeBuoiHocAsync(List<LopHoc> dsLop)
        {
            if (dsLop.Count == 0)
                return (new Dictionary<int, int>(), new Dictionary<int, int>());

            var tenKhoaHoc = dsLop
                .Select(x => x.KhoaHoc)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var tongTheoKhoaHoc = await _context.KhoaHocs
                .Where(x => tenKhoaHoc.Contains(x.TenKhoaHoc))
                .ToDictionaryAsync(x => x.TenKhoaHoc, x => x.SoBuoi);

            var lopIds = dsLop.Select(x => x.Id).ToList();
            var daHoc = await _context.DiemDanhs
                .Where(x => lopIds.Contains(x.LopHocId))
                .GroupBy(x => x.LopHocId)
                .Select(x => new
                {
                    LopHocId = x.Key,
                    SoBuoi = x.Select(y => y.NgayHoc.Date).Distinct().Count()
                })
                .ToDictionaryAsync(x => x.LopHocId, x => x.SoBuoi);

            var tong = dsLop.ToDictionary(
                x => x.Id,
                x => tongTheoKhoaHoc.TryGetValue(x.KhoaHoc, out var soBuoi) ? soBuoi : 0);

            return (daHoc, tong);
        }

        // =============================
        // CHI TIẾT 1 LỚP (JSON cho modal)
        // =============================
        [HttpGet]
        public async Task<IActionResult> ChiTietLop(int id)
        {
            var giaoVien = await LayGiaoVienHienTaiAsync();
            var lop = await _context.LopHocs.FirstOrDefaultAsync(x => x.Id == id);

            if (lop == null || giaoVien == null || lop.GiaoVien != giaoVien.HoTen)
                return NotFound();

            var hocViens = await _context.HocViens
                .Where(x => x.LopHoc == lop.TenLop)
                .OrderBy(x => x.HoTen)
                .Select(x => new
                {
                    x.MaHocVien,
                    x.HoTen,
                    x.SoDienThoai,
                    x.Email,
                    x.TrangThai
                })
                .ToListAsync();

            return Json(new
            {
                lop = new
                {
                    lop.MaLop,
                    lop.TenLop,
                    lop.KhoaHoc,
                    lop.NgonNgu,
                    lop.CaHoc,
                    lop.PhongHoc,
                    lop.SiSoHienTai,
                    lop.SiSoToiDa,
                    lop.TrangThai
                },
                hocViens
            });
        }

        // =============================
        // LẤY DỮ LIỆU ĐIỂM DANH (JSON)
        // =============================
        [HttpGet]
        public async Task<IActionResult> LayDuLieuDiemDanh(int lopId, string? ngay)
        {
            var giaoVien = await LayGiaoVienHienTaiAsync();
            var lop = await _context.LopHocs.FirstOrDefaultAsync(x => x.Id == lopId);

            if (lop == null || giaoVien == null || lop.GiaoVien != giaoVien.HoTen)
                return NotFound();

            DateTime ngayDiemDanh = string.IsNullOrEmpty(ngay)
                ? DateTime.Today
                : DateTime.Parse(ngay);

            var hocViens = await _context.HocViens
                .Where(x => x.LopHoc == lop.TenLop)
                .OrderBy(x => x.HoTen)
                .ToListAsync();

            var diemDanhHomNay = await _context.DiemDanhs
                .Where(x => x.LopHocId == lopId && x.NgayHoc.Date == ngayDiemDanh.Date)
                .ToListAsync();

            var ketQua = hocViens.Select(hv =>
            {
                var dd = diemDanhHomNay.FirstOrDefault(x => x.HocVienId == hv.Id);
                return new
                {
                    hocVienId = hv.Id,
                    maHocVien = hv.MaHocVien,
                    hoTen = hv.HoTen,
                    trangThai = dd?.TrangThai ?? "Chưa điểm danh",
                    ghiChu = dd?.GhiChu ?? ""
                };
            }).ToList();

            return Json(new
            {
                lop = new { lop.Id, lop.MaLop, lop.TenLop },
                ngay = ngayDiemDanh.ToString("yyyy-MM-dd"),
                danhSach = ketQua
            });
            
        }

        // =============================
        // LƯU ĐIỂM DANH
        // =============================
        [HttpPost]
        public async Task<IActionResult> LuuDiemDanh([FromBody] LuuDiemDanhRequest request)
        {
            var giaoVien = await LayGiaoVienHienTaiAsync();
            var lop = await _context.LopHocs.FirstOrDefaultAsync(x => x.Id == request.LopId);

            if (lop == null || giaoVien == null || lop.GiaoVien != giaoVien.HoTen)
                return NotFound();

            DateTime ngay = DateTime.Parse(request.Ngay);

            foreach (var item in request.DanhSach)
            {
                var existing = await _context.DiemDanhs
                    .FirstOrDefaultAsync(x =>
                        x.LopHocId == request.LopId &&
                        x.HocVienId == item.HocVienId &&
                        x.NgayHoc.Date == ngay.Date);

                if (existing != null)
                {
                    existing.TrangThai = item.TrangThai;
                    existing.GhiChu = item.GhiChu;
                    existing.ThoiGianDiemDanh = DateTime.Now;
                    existing.GiaoVienDiemDanh = giaoVien.HoTen;
                }
                else
                {
                    var hv = await _context.HocViens.FindAsync(item.HocVienId);
                    if (hv == null) continue;

                    _context.DiemDanhs.Add(new DiemDanh
                    {
                        LopHocId = request.LopId,
                        MaLop = lop.MaLop,
                        TenLop = lop.TenLop,
                        HocVienId = item.HocVienId,
                        MaHocVien = hv.MaHocVien,
                        HoTenHocVien = hv.HoTen,
                        NgayHoc = ngay,
                        TrangThai = item.TrangThai,
                        GhiChu = item.GhiChu,
                        GiaoVienDiemDanh = giaoVien.HoTen,
                        ThoiGianDiemDanh = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Đã lưu điểm danh thành công" });
        }
        
    }

    // =============================
    // CLASS REQUEST (để nhận dữ liệu từ JS)
    // =============================
    public class LuuDiemDanhRequest
    {
        public int LopId { get; set; }
        public string Ngay { get; set; } = "";
        public List<DiemDanhItem> DanhSach { get; set; } = new();
    }

    public class DiemDanhItem
    {
        public int HocVienId { get; set; }
        public string TrangThai { get; set; } = "Có mặt";
        public string? GhiChu { get; set; }
    }
    public class LuuDiemSoRequest
{
    public int LopId { get; set; }
    public List<DiemSoItem> DanhSach { get; set; } = new();
}

public class DiemSoItem
{
    public int HocVienId { get; set; }
    public decimal? DiemKT1 { get; set; }
    public decimal? DiemKT2 { get; set; }
    public decimal? DiemKT3 { get; set; }
    public decimal? DiemKiemTraLon { get; set; }
    public string? GhiChu { get; set; }
}

}