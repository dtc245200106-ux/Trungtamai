using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trungtamai.Attributes;
using Trungtamai.Data;

namespace Trungtamai.Controllers
{
    [AuthorizeRole("TuVanVien")]
    public class TuVanVienController : Controller
    {
        private readonly AppDbContext _context;

        public TuVanVienController(AppDbContext context)
        {
            _context = context;
        }

        // =============================
        // TRANG MẶC ĐỊNH → CHUYỂN HƯỚNG SANG KHÓA HỌC ĐANG MỞ
        // =============================
        public IActionResult Index()
        {

            // Chuyển hướng thẳng sang trang Khóa học đang mở
            return RedirectToAction("KhoaHocMoLop");
        }

        // =============================
        // TRANG KHÓA HỌC / LỚP HỌC ĐANG MỞ
        // =============================
        public async Task<IActionResult> KhoaHocMoLop()
        {

            ViewBag.HoTen = HttpContext.Session.GetString("HoTen");

            // Lấy danh sách ngôn ngữ duy nhất từ bảng KhoaHoc
            ViewBag.DsNgonNgu = await _context.KhoaHocs
                .Select(x => x.NgonNgu)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            return View();
        }

        // =============================
        // API: Lấy danh sách khóa học theo ngôn ngữ
        // =============================
        [HttpGet]
        public async Task<IActionResult> LayKhoaHocTheoNgonNgu(string ngonNgu)
        {
            if (string.IsNullOrWhiteSpace(ngonNgu))
            {
                return Json(new List<object>());
            }

            var ds = await _context.KhoaHocs
                .Where(x => x.NgonNgu == ngonNgu)
                .OrderBy(x => x.TenKhoaHoc)
                .Select(x => new
                {
                    id = x.Id,
                    tenKhoaHoc = x.TenKhoaHoc
                })
                .ToListAsync();

            return Json(ds);
        }

        // =============================
        // API: Lấy danh sách lớp đang mở theo khóa học
        // =============================
        [HttpGet]
        public async Task<IActionResult> LayLopTheoKhoaHoc(int khoaHocId)
        {
            // Lấy tên khóa học từ bảng KhoaHoc
            var khoaHoc = await _context.KhoaHocs.FindAsync(khoaHocId);
            if (khoaHoc == null)
            {
                return Json(new List<object>());
            }

            var lopHocs = await _context.LopHocs
                .Where(x => x.KhoaHoc == khoaHoc.TenKhoaHoc && x.TrangThai != "Đã kết thúc")
                .OrderBy(x => x.TenLop)
                .ToListAsync();

            var lopIds = lopHocs.Select(x => x.Id).ToList();

            // Số buổi đã học của mỗi lớp = số ngày học khác nhau đã có điểm danh
            var soBuoiDaHocTheoLop = await _context.DiemDanhs
                .Where(x => lopIds.Contains(x.LopHocId))
                .Select(x => new { x.LopHocId, Ngay = x.NgayHoc.Date })
                .Distinct()
                .GroupBy(x => x.LopHocId)
                .Select(g => new { LopHocId = g.Key, SoBuoi = g.Count() })
                .ToListAsync();

           var ds = lopHocs.Select(x => new
{
    id = x.Id,
    tenLop = x.TenLop,
    trangThai = x.TrangThai,
    siSoToiDa = x.SiSoToiDa,
    siSoHienTai = x.SiSoHienTai,
    soBuoiDaHoc = soBuoiDaHocTheoLop.FirstOrDefault(s => s.LopHocId == x.Id)?.SoBuoi ?? 0,
    tongSoBuoi = khoaHoc.SoBuoi,
    hocPhi = khoaHoc.Gia
}).ToList();

            return Json(ds);
        }
    }
}