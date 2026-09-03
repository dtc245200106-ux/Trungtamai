using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trungtamai.Attributes;
using Trungtamai.Data;

namespace Trungtamai.Controllers
{
    [AuthorizeRole("HocVien")]
    public class HocVienController : Controller
    {
        private readonly AppDbContext _context;

        public HocVienController(AppDbContext context)
        {
            _context = context;
        }

        

// =============================
// LỊCH HỌC CỦA HỌC VIÊN
// =============================

        public async Task<IActionResult> LichHoc()
{

    if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int userId))
        return RedirectToAction("Login", "Account");

    var taiKhoan = await _context.NguoiDungs.FindAsync(userId);
    if (taiKhoan == null)
        return RedirectToAction("Login", "Account");

    ViewBag.HoTen = taiKhoan.HoTen;

    var hocVien = await _context.HocViens
        .FirstOrDefaultAsync(x => x.Email == taiKhoan.TaiKhoan);

    var lop = (hocVien != null && !string.IsNullOrWhiteSpace(hocVien.LopHoc))
        ? await _context.LopHocs.FirstOrDefaultAsync(x => x.TenLop == hocVien.LopHoc)
        : null;

    return View(lop); // Model là LopHoc? (có thể null nếu chưa xếp lớp)
}
        public async Task<IActionResult> LopCuaToi()
{
    if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int userId))
        return RedirectToAction("Login", "Account");

    var taiKhoan = await _context.NguoiDungs.FindAsync(userId);
    if (taiKhoan == null)
        return RedirectToAction("Login", "Account");

    ViewBag.HoTen = taiKhoan.HoTen;

    var hocVien = await _context.HocViens
        .FirstOrDefaultAsync(x => x.Email == taiKhoan.TaiKhoan);

    var lop = (hocVien != null && !string.IsNullOrWhiteSpace(hocVien.LopHoc))
        ? await _context.LopHocs.FirstOrDefaultAsync(x => x.TenLop == hocVien.LopHoc)
        : null;

    ViewBag.LopId = lop?.Id; // dùng để JS gọi ChiTietLop?id=...
    return View();
}
// =============================
// CHI TIẾT LỚP CỦA TÔI (JSON cho modal)
// =============================
[HttpGet]
public async Task<IActionResult> ChiTietLop(int id)
{
    if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int userId))
        return Unauthorized();

    var taiKhoan = await _context.NguoiDungs.FindAsync(userId);
    var hocVien = taiKhoan == null
        ? null
        : await _context.HocViens.FirstOrDefaultAsync(x => x.Email == taiKhoan.TaiKhoan);

    var lop = await _context.LopHocs.FirstOrDefaultAsync(x => x.Id == id);

    // Chỉ cho xem đúng lớp mà chính học viên này đang học
    if (lop == null || hocVien == null || lop.TenLop != hocVien.LopHoc)
        return NotFound();

    var diemDanh = await _context.DiemDanhs
        .Where(x => x.MaHocVien == hocVien.MaHocVien && x.MaLop == lop.MaLop)
        .OrderByDescending(x => x.NgayHoc)
        .Select(x => new { ngay = x.NgayHoc, trangThai = x.TrangThai, ghiChu = x.GhiChu })
        .ToListAsync();

    var diemEntity = await _context.DiemSos
    .Where(x => x.MaHocVien == hocVien.MaHocVien && x.MaLop == lop.MaLop)
    .FirstOrDefaultAsync();

object? diemSo = null;
if (diemEntity != null)
{
    diemSo = new
    {
        diemKT1 = diemEntity.DiemKT1,
        diemKT2 = diemEntity.DiemKT2,
        diemKT3 = diemEntity.DiemKT3,
        diemKiemTraLon = diemEntity.DiemKiemTraLon,
        diemTrungBinh = diemEntity.DiemTrungBinh,
        ketQua = diemEntity.KetQua
    };
}

    return Json(new
    {
        lop = new
        {
            lop.MaLop,
            lop.TenLop,
            lop.GiaoVien,
            lop.PhongHoc,
            lop.CaHoc,
            lop.TrangThai
        },
        diemDanh,
        diemSo
    });
}
        // =============================
        // TRANG TỔNG QUAN CỦA HỌC VIÊN
        // =============================
        public async Task<IActionResult> Index()
        {

            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out int userId))
                return RedirectToAction("Login", "Account");

            var taiKhoan = await _context.NguoiDungs.FindAsync(userId);
            if (taiKhoan == null)
                return RedirectToAction("Login", "Account");

            ViewBag.HoTen = taiKhoan.HoTen;

            // NguoiDung.TaiKhoan == HocVien.Email (xem QuanLyController.ThemHocVien)
            var hocVien = await _context.HocViens
                .FirstOrDefaultAsync(x => x.Email == taiKhoan.TaiKhoan);

            var lichHocHomNay = new List<object>();
            var dsLopHocCuaToi = new List<object>();
            decimal? diemTrungBinh = null;
            decimal hocPhiConNo = 0;

            if (hocVien != null)
            {
                hocPhiConNo = hocVien.ConNo;

                var diem = await _context.DiemSos
                    .Where(x => x.MaHocVien == hocVien.MaHocVien)
                    .OrderByDescending(x => x.NgayCapNhat)
                    .FirstOrDefaultAsync();
                diemTrungBinh = diem?.DiemTrungBinh;

                // HocVien.LopHoc lưu TenLop
                var lop = string.IsNullOrWhiteSpace(hocVien.LopHoc)
                    ? null
                    : await _context.LopHocs.FirstOrDefaultAsync(x => x.TenLop == hocVien.LopHoc);

                if (lop != null)
                {
                    string[] thuVN = { "CN", "T2", "T3", "T4", "T5", "T6", "T7" };
                    string homNay = thuVN[(int)DateTime.Now.DayOfWeek];
                    var cacThu = (lop.NgayHoc ?? "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    if (cacThu.Contains(homNay))
                    {
                        lichHocHomNay.Add(new
                        {
                            gio = $"{lop.GioBatDau} - {lop.GioKetThuc}",
                            lop = lop.TenLop,
                            phong = lop.PhongHoc,
                            trangThai = "Sắp diễn ra"
                        });
                    }

                    int soBuoiDaHoc = await _context.DiemDanhs.CountAsync(x =>
    x.MaHocVien == hocVien.MaHocVien &&
    x.MaLop == lop.MaLop &&
    (x.TrangThai == "Có mặt" || x.TrangThai == "Muộn"));

                    var khoaHoc = await _context.KhoaHocs
                        .FirstOrDefaultAsync(x => x.TenKhoaHoc == lop.KhoaHoc);
                    int tongSoBuoi = khoaHoc?.SoBuoi ?? 0;

                    dsLopHocCuaToi.Add(new
{
    id = lop.Id,          // ← thêm dòng này
    ma = lop.MaLop,
    ten = lop.TenLop,
    giaoVien = lop.GiaoVien,
    tienDo = tongSoBuoi > 0 ? $"{soBuoiDaHoc}/{tongSoBuoi} buổi" : $"{soBuoiDaHoc} buổi",
    lich = string.IsNullOrWhiteSpace(lop.CaHoc) ? $"{lop.NgayHoc}, {lop.GioBatDau}" : lop.CaHoc,
    trangThai = lop.TrangThai
});
                }
            }

            ViewBag.LichHocHomNayJson = System.Text.Json.JsonSerializer.Serialize(lichHocHomNay);
            ViewBag.DanhSachLopJson = System.Text.Json.JsonSerializer.Serialize(dsLopHocCuaToi);
            ViewBag.DiemTrungBinh = diemTrungBinh.HasValue ? diemTrungBinh.Value.ToString("0.0") : "--";
            ViewBag.HocPhiConNo = hocPhiConNo.ToString(System.Globalization.CultureInfo.InvariantCulture);

            return View();
        }
    }
}