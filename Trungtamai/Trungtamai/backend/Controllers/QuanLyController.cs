using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trungtamai.Attributes;
using Trungtamai.Data;
using Trungtamai.Models;

namespace Trungtamai.Controllers
{
    [AuthorizeRole("QuanLy")]
    public class QuanLyController : Controller
    {
        private readonly AppDbContext _context;

        public QuanLyController(AppDbContext context)
        {
            _context = context;
        }

        // =============================
// TRANG TỔNG QUAN
// =============================
public async Task<IActionResult> Index()
{
    ViewBag.HoTen = HttpContext.Session.GetString("HoTen");

    // ===== THỐNG KÊ CHÍNH =====
    var tongHocVien = await _context.HocViens.CountAsync();
    var tongGiaoVien = await _context.GiaoViens.CountAsync();
    var lopDangHoatDong = await _context.LopHocs
        .CountAsync(x => x.TrangThai == "Đang học" || x.TrangThai == "Sắp khai giảng");
    var tongKhoaHoc = await _context.KhoaHocs.CountAsync();

    
    var lopHocs = await _context.LopHocs
        .Where(x => x.TrangThai != "Đã kết thúc")
        .ToListAsync();
    var khoaHocs = await _context.KhoaHocs.ToListAsync();

    // Doanh thu thực tế = tổng số tiền học viên đã đóng
decimal doanhThu = await _context.HocViens.SumAsync(x => x.SoTienDaDong);
decimal tongPhaiDong = await _context.HocViens.SumAsync(x => x.SoTienPhaiDong);
decimal tongConNo = await _context.HocViens
    .SumAsync(x => x.SoTienPhaiDong > x.SoTienDaDong ? x.SoTienPhaiDong - x.SoTienDaDong : 0);

ViewBag.TongHocVien = tongHocVien;
ViewBag.TongGiaoVien = tongGiaoVien;
ViewBag.LopDangHoatDong = lopDangHoatDong;
ViewBag.TongKhoaHoc = tongKhoaHoc;
ViewBag.DoanhThuThang = doanhThu;
ViewBag.TongPhaiDong = tongPhaiDong;
ViewBag.TongConNo = tongConNo;

    // ===== LỊCH HỌC HÔM NAY (theo thứ trong tuần) =====
    // T2=Monday ... CN=Sunday
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

    var lichHomNay = await _context.LopHocs
        .Where(x => x.TrangThai != "Đã kết thúc"
                 && x.NgayHoc != null
                 && x.NgayHoc.Contains(thuHomNay))
        .OrderBy(x => x.GioBatDau)
        .ToListAsync();

    ViewBag.LichHomNay = lichHomNay;

    // ===== SỐ LỚP THEO NGÔN NGỮ =====
    var soLopAnh = await _context.LopHocs.CountAsync(x => x.NgonNgu == "Tiếng Anh" && x.TrangThai != "Đã kết thúc");
    var soLopTrung = await _context.LopHocs.CountAsync(x => x.NgonNgu == "Tiếng Trung" && x.TrangThai != "Đã kết thúc");
    var soLopNhat = await _context.LopHocs.CountAsync(x => x.NgonNgu == "Tiếng Nhật" && x.TrangThai != "Đã kết thúc");
    var soLopHan = await _context.LopHocs.CountAsync(x => x.NgonNgu == "Tiếng Hàn" && x.TrangThai != "Đã kết thúc");
    var tongLop = soLopAnh + soLopTrung + soLopNhat + soLopHan;
    if (tongLop == 0) tongLop = 1; // tránh chia 0

    ViewBag.SoLopAnh = soLopAnh;
    ViewBag.SoLopTrung = soLopTrung;
    ViewBag.SoLopNhat = soLopNhat;
    ViewBag.SoLopHan = soLopHan;
    ViewBag.PctAnh = (int)(soLopAnh * 100.0 / tongLop);
    ViewBag.PctTrung = (int)(soLopTrung * 100.0 / tongLop);
    ViewBag.PctNhat = (int)(soLopNhat * 100.0 / tongLop);
    ViewBag.PctHan = (int)(soLopHan * 100.0 / tongLop);

    return View();
}
        

        // =============================
        // QUẢN LÝ KHÓA HỌC
        // =============================
        public async Task<IActionResult> KhoaHoc()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");

            if (vaiTro != "QuanLy")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.HoTen = HttpContext.Session.GetString("HoTen");

            var khoaHocs = await _context.KhoaHocs
                .OrderBy(x => x.TenKhoaHoc)
                .ToListAsync();

            return View(khoaHocs);
        }

        // =============================
        // QUẢN LÝ HỌC VIÊN
        // =============================
        public async Task<IActionResult> HocVien()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "QuanLy")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.HoTen = HttpContext.Session.GetString("HoTen");

            var hocViens = await _context.HocViens
                .OrderBy(x => x.MaHocVien)
                .ToListAsync();

            ViewBag.TongHocVien = hocViens.Count;
            ViewBag.DangHoc = hocViens.Count(x => x.TrangThai == "Đang học");
            ViewBag.TamNgung = hocViens.Count(x => x.TrangThai == "Tạm ngưng");
            ViewBag.DaKetThuc = hocViens.Count(x => x.TrangThai == "Đã kết thúc");

            var khoaHocs = await _context.KhoaHocs
                .OrderBy(x => x.TenKhoaHoc)
                .Select(x => new { ten = x.TenKhoaHoc, ngonNgu = x.NgonNgu })
                .ToListAsync();
            ViewBag.DanhSachKhoaHocJson = System.Text.Json.JsonSerializer.Serialize(khoaHocs);

            var lopHocs = await _context.LopHocs
                .OrderBy(x => x.TenLop)
                .Select(x => new { ten = x.TenLop, khoaHoc = x.KhoaHoc, ngonNgu = x.NgonNgu })
                .ToListAsync();
            ViewBag.DanhSachLopHocJson = System.Text.Json.JsonSerializer.Serialize(lopHocs);

            return View(hocViens);
        }

                [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemHocVien(
            string hoTen,
            string soDienThoai,
            string email,
            string khoaHoc,
            string lopHoc,
            string ngonNgu,
            string trangThai)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "QuanLy")
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(hoTen) ||
                string.IsNullOrWhiteSpace(soDienThoai) ||
                string.IsNullOrWhiteSpace(email))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin bắt buộc.";
                return RedirectToAction(nameof(HocVien));
            }

            string emailTrim = email.Trim();

            bool taiKhoanTonTai = await _context.NguoiDungs
                .AnyAsync(x => x.TaiKhoan == emailTrim);

            if (taiKhoanTonTai)
            {
                TempData["Error"] = "Email này đã được dùng làm tài khoản đăng nhập.";
                return RedirectToAction(nameof(HocVien));
            }

            int soLuong = await _context.HocViens.CountAsync();
            string maHV = $"HV{soLuong + 1:D3}";

            string sdt = soDienThoai.Trim();
            string matKhau = sdt.Length >= 4
                ? sdt.Substring(sdt.Length - 4)
                : "0000";

            decimal hocPhi = 0;
            string tenKH = (khoaHoc ?? "").Trim();
            if (!string.IsNullOrEmpty(tenKH))
            {
                var kh = await _context.KhoaHocs
                    .FirstOrDefaultAsync(x => x.TenKhoaHoc == tenKH);
                if (kh != null)
                    hocPhi = kh.Gia;
            }

            var hv = new HocVien
            {
                MaHocVien = maHV,
                HoTen = hoTen.Trim(),
                SoDienThoai = sdt,
                Email = emailTrim,
                KhoaHoc = tenKH,
                LopHoc = (lopHoc ?? "").Trim(),
                NgonNgu = (ngonNgu ?? "").Trim(),
                TrangThai = string.IsNullOrWhiteSpace(trangThai) ? "Đang học" : trangThai.Trim(),
                SoTienPhaiDong = hocPhi,
                SoTienDaDong = 0
            };

            var taiKhoanDangNhap = new NguoiDung
            {
                HoTen = hv.HoTen,
                TaiKhoan = emailTrim,
                MatKhau = matKhau,
                VaiTro = "HocVien"
            };

            _context.HocViens.Add(hv);
            _context.NguoiDungs.Add(taiKhoanDangNhap);
            await _context.SaveChangesAsync();

            await CapNhatSiSoLopAsync(hv.LopHoc);

            TempData["Success"] =
                $"Thêm học viên thành công. Mã: {maHV}. " +
                $"Tài khoản: {emailTrim} / Mật khẩu: {matKhau}";

            return RedirectToAction(nameof(HocVien));
        }

        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> SuaHocVien(
    int id,
    string hoTen,
    string soDienThoai,
    string email,
    string khoaHoc,
    string lopHoc,
    string ngonNgu,
    string trangThai)
{
    var vaiTro = HttpContext.Session.GetString("VaiTro");
    if (vaiTro != "QuanLy")
        return RedirectToAction("Login", "Account");

    var hv = await _context.HocViens.FindAsync(id);
    if (hv == null)
    {
        TempData["Error"] = "Không tìm thấy học viên.";
        return RedirectToAction(nameof(HocVien));
    }

    if (string.IsNullOrWhiteSpace(hoTen) ||
        string.IsNullOrWhiteSpace(soDienThoai) ||
        string.IsNullOrWhiteSpace(email))
    {
        TempData["Error"] = "Vui lòng nhập đầy đủ thông tin bắt buộc.";
        return RedirectToAction(nameof(HocVien));
    }

    string emailMoi = email.Trim();
    string emailCu = hv.Email;

    // Nếu đổi email → kiểm tra trùng
    if (!string.Equals(emailCu, emailMoi, StringComparison.OrdinalIgnoreCase))
    {
        bool emailTonTai = await _context.NguoiDungs
            .AnyAsync(x => x.TaiKhoan == emailMoi);

        if (emailTonTai)
        {
            TempData["Error"] = "Email mới đã được dùng làm tài khoản.";
            return RedirectToAction(nameof(HocVien));
        }
    }

                        string? lopCu = hv.LopHoc;
            string khoaMoi = (khoaHoc ?? "").Trim();

            hv.HoTen = hoTen.Trim();
            hv.SoDienThoai = soDienThoai.Trim();
            hv.Email = emailMoi;
            hv.KhoaHoc = khoaMoi;
            hv.LopHoc = (lopHoc ?? "").Trim();
            hv.NgonNgu = (ngonNgu ?? "").Trim();
            hv.TrangThai = string.IsNullOrWhiteSpace(trangThai) ? "Đang học" : trangThai.Trim();

            // Luôn lấy học phí phải đóng = giá khóa học hiện tại
            if (!string.IsNullOrEmpty(khoaMoi))
            {
                var kh = await _context.KhoaHocs
                    .FirstOrDefaultAsync(x => x.TenKhoaHoc == khoaMoi);
                if (kh != null)
                    hv.SoTienPhaiDong = kh.Gia;
            }

    // Cập nhật tài khoản đăng nhập
    var taiKhoan = await _context.NguoiDungs
        .FirstOrDefaultAsync(x => x.TaiKhoan == emailCu);

    if (taiKhoan != null)
    {
        taiKhoan.HoTen = hv.HoTen;
        taiKhoan.TaiKhoan = emailMoi;

        string sdt = hv.SoDienThoai;
        taiKhoan.MatKhau = sdt.Length >= 4
            ? sdt.Substring(sdt.Length - 4)
            : "0000";
    }
    else
    {
        // Chưa có tài khoản → tạo mới
        string sdt = hv.SoDienThoai;
        string matKhau = sdt.Length >= 4 ? sdt.Substring(sdt.Length - 4) : "0000";

        _context.NguoiDungs.Add(new NguoiDung
        {
            HoTen = hv.HoTen,
            TaiKhoan = emailMoi,
            MatKhau = matKhau,
            VaiTro = "HocVien"
        });
    }

    await _context.SaveChangesAsync();

    await CapNhatSiSoLopAsync(lopCu);
    if (!string.Equals(lopCu, hv.LopHoc, StringComparison.OrdinalIgnoreCase))
    {
        await CapNhatSiSoLopAsync(hv.LopHoc);
    }

    TempData["Success"] = "Cập nhật học viên thành công.";
    return RedirectToAction(nameof(HocVien));
}

       [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> XoaHocVien(int id)
{
    var vaiTro = HttpContext.Session.GetString("VaiTro");
    if (vaiTro != "QuanLy")
        return RedirectToAction("Login", "Account");

    var hv = await _context.HocViens.FindAsync(id);
    if (hv == null)
    {
        TempData["Error"] = "Không tìm thấy học viên.";
        return RedirectToAction(nameof(HocVien));
    }

    string? lopBiAnhHuong = hv.LopHoc;

    // Xóa tài khoản đăng nhập
    var taiKhoan = await _context.NguoiDungs
        .FirstOrDefaultAsync(x => x.TaiKhoan == hv.Email);

    if (taiKhoan != null)
    {
        _context.NguoiDungs.Remove(taiKhoan);
    }

    _context.HocViens.Remove(hv);
    await _context.SaveChangesAsync();

    await CapNhatSiSoLopAsync(lopBiAnhHuong);

    TempData["Success"] = "Xóa học viên thành công.";
    return RedirectToAction(nameof(HocVien));
}

        // =============================
        // QUẢN LÝ GIÁO VIÊN
        // =============================
        public async Task<IActionResult> GiaoVien()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "QuanLy")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.HoTen = HttpContext.Session.GetString("HoTen");

            var giaoViens = await _context.GiaoViens
                .OrderBy(x => x.MaGiaoVien)
                .ToListAsync();

            ViewBag.TongGiaoVien = giaoViens.Count;

            return View(giaoViens);
        }

        // =============================
        // THÊM GIÁO VIÊN
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemGiaoVien(
            string hoTen,
            string email,
            string soDienThoai,
            string ngonNgu,
            string chuyenMon,
            int kinhNghiem)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");

            if (vaiTro != "QuanLy")
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(hoTen) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(soDienThoai) ||
                string.IsNullOrWhiteSpace(ngonNgu) ||
                string.IsNullOrWhiteSpace(chuyenMon))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin.";
                return RedirectToAction(nameof(GiaoVien));
            }

            bool emailTonTai = await _context.GiaoViens
                .AnyAsync(x => x.Email == email.Trim());

            if (emailTonTai)
            {
                TempData["Error"] = "Email này đã tồn tại.";
                return RedirectToAction(nameof(GiaoVien));
            }

            int soLuongGiaoVien = await _context.GiaoViens.CountAsync();
            string maGiaoVien = $"GV{soLuongGiaoVien + 1:D3}";

            // Kiểm tra email đã dùng làm tài khoản chưa
bool taiKhoanTonTai = await _context.NguoiDungs
    .AnyAsync(x => x.TaiKhoan == email.Trim());

if (taiKhoanTonTai)
{
    TempData["Error"] = "Email này đã được dùng làm tài khoản đăng nhập.";
    return RedirectToAction(nameof(GiaoVien));
}

string sdt = soDienThoai.Trim();
string matKhau = sdt.Length >= 4
    ? sdt.Substring(sdt.Length - 4)
    : "0000";

var giaoVien = new GiaoVien
{
    MaGiaoVien = maGiaoVien,
    HoTen = hoTen.Trim(),
    Email = email.Trim(),
    SoDienThoai = sdt,
    NgonNgu = ngonNgu.Trim(),
    ChuyenMon = chuyenMon.Trim(),
    KinhNghiem = kinhNghiem
};

var taiKhoanDangNhap = new NguoiDung
{
    HoTen = hoTen.Trim(),
    TaiKhoan = email.Trim(),      // ← đổi từ maGiaoVien sang email
    MatKhau = matKhau,
    VaiTro = "GiaoVien"
};

_context.GiaoViens.Add(giaoVien);
_context.NguoiDungs.Add(taiKhoanDangNhap);

await _context.SaveChangesAsync();

TempData["Success"] =
    $"Thêm giáo viên thành công. Mã: {maGiaoVien}. " +
    $"Tài khoản: {email.Trim()} / Mật khẩu: {matKhau}";

            return RedirectToAction(nameof(GiaoVien));
        }

        // =============================
        // SỬA GIÁO VIÊN
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaGiaoVien(
            int id,
            string hoTen,
            string email,
            string soDienThoai,
            string ngonNgu,
            string chuyenMon,
            int kinhNghiem)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "QuanLy")
            {
                return RedirectToAction("Login", "Account");
            }

            var giaoVien = await _context.GiaoViens.FindAsync(id);
            if (giaoVien == null)
            {
                TempData["Error"] = "Không tìm thấy giáo viên.";
                return RedirectToAction(nameof(GiaoVien));
            }

            if (string.IsNullOrWhiteSpace(hoTen) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(soDienThoai) ||
                string.IsNullOrWhiteSpace(ngonNgu) ||
                string.IsNullOrWhiteSpace(chuyenMon))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin.";
                return RedirectToAction(nameof(GiaoVien));
            }

            bool emailTonTai = await _context.GiaoViens
                .AnyAsync(x => x.Email == email.Trim() && x.Id != id);

            if (emailTonTai)
            {
                TempData["Error"] = "Email này đã tồn tại.";
                return RedirectToAction(nameof(GiaoVien));
            }

            giaoVien.HoTen = hoTen.Trim();
            giaoVien.Email = email.Trim();
            giaoVien.SoDienThoai = soDienThoai.Trim();
            giaoVien.NgonNgu = ngonNgu.Trim();
            giaoVien.ChuyenMon = chuyenMon.Trim();
            giaoVien.KinhNghiem = kinhNghiem;

            // Tìm tài khoản: ưu tiên theo Email mới, fallback theo Email cũ nếu đổi email
var taiKhoanDangNhap = await _context.NguoiDungs
    .FirstOrDefaultAsync(x => x.TaiKhoan == giaoVien.Email || x.TaiKhoan == email.Trim());

// Nếu vẫn null, thử tìm theo mã GV (dữ liệu cũ)
if (taiKhoanDangNhap == null)
{
    taiKhoanDangNhap = await _context.NguoiDungs
        .FirstOrDefaultAsync(x => x.TaiKhoan == giaoVien.MaGiaoVien);
}

string emailCu = giaoVien.Email;

giaoVien.HoTen = hoTen.Trim();
giaoVien.Email = email.Trim();
giaoVien.SoDienThoai = soDienThoai.Trim();
giaoVien.NgonNgu = ngonNgu.Trim();
giaoVien.ChuyenMon = chuyenMon.Trim();
giaoVien.KinhNghiem = kinhNghiem;

if (taiKhoanDangNhap != null)
{
    taiKhoanDangNhap.HoTen = giaoVien.HoTen;
    taiKhoanDangNhap.TaiKhoan = giaoVien.Email;   // cập nhật tài khoản = email mới

    string sdt = giaoVien.SoDienThoai;
    taiKhoanDangNhap.MatKhau = sdt.Length >= 4
        ? sdt.Substring(sdt.Length - 4)
        : "0000";
}

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật giáo viên thành công.";
            return RedirectToAction(nameof(GiaoVien));
        }

        // =============================
        // XÓA GIÁO VIÊN
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaGiaoVien(int id)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "QuanLy")
            {
                return RedirectToAction("Login", "Account");
            }

            var giaoVien = await _context.GiaoViens.FindAsync(id);
            if (giaoVien == null)
            {
                TempData["Error"] = "Không tìm thấy giáo viên.";
                return RedirectToAction(nameof(GiaoVien));
            }

            var taiKhoanDangNhap = await _context.NguoiDungs
    .FirstOrDefaultAsync(x => x.TaiKhoan == giaoVien.Email);

// Fallback dữ liệu cũ (tài khoản = MaGiaoVien)
if (taiKhoanDangNhap == null)
{
    taiKhoanDangNhap = await _context.NguoiDungs
        .FirstOrDefaultAsync(x => x.TaiKhoan == giaoVien.MaGiaoVien);
}

if (taiKhoanDangNhap != null)
{
    _context.NguoiDungs.Remove(taiKhoanDangNhap);
}

            _context.GiaoViens.Remove(giaoVien);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa giáo viên thành công.";
            return RedirectToAction(nameof(GiaoVien));
        }

        // =============================
        // QUẢN LÝ LỚP HỌC
        // =============================
        public async Task<IActionResult> LopHoc()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "QuanLy")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.HoTen = HttpContext.Session.GetString("HoTen");

            var lopHocs = await _context.LopHocs
                .OrderBy(x => x.MaLop)
                .ToListAsync();

            ViewBag.TongLop = lopHocs.Count;
            ViewBag.DangHoc = lopHocs.Count(x => x.TrangThai == "Đang học");
            ViewBag.SapKhaiGiang = lopHocs.Count(x => x.TrangThai == "Sắp khai giảng");

            var khoaHocs = await _context.KhoaHocs
                .OrderBy(x => x.TenKhoaHoc)
                .Select(x => new { ten = x.TenKhoaHoc, ngonNgu = x.NgonNgu })
                .ToListAsync();
            ViewBag.DanhSachKhoaHocJson = System.Text.Json.JsonSerializer.Serialize(khoaHocs);

            var giaoViens = await _context.GiaoViens
                .OrderBy(x => x.HoTen)
                .Select(x => new { ten = x.HoTen, ngonNgu = x.NgonNgu })
                .ToListAsync();
            ViewBag.DanhSachGiaoVienJson = System.Text.Json.JsonSerializer.Serialize(giaoViens);

            return View(lopHocs);
        }

        // =============================
        // THÊM LỚP HỌC
        // =============================
        
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ThemLopHoc(
    string tenLop,
    string khoaHoc,
    string ngonNgu,
    string giaoVien,
    int siSoToiDa,
    string[] ngayHoc,
    string gioBatDau,
    string gioKetThuc,
    string phongHoc,
    string trangThai)
{
    var vaiTro = HttpContext.Session.GetString("VaiTro");
    if (vaiTro != "QuanLy")
        return RedirectToAction("Login", "Account");

    string ngayHocStr = (ngayHoc != null && ngayHoc.Length > 0)
        ? string.Join(",", ngayHoc)
        : "";

    if (string.IsNullOrWhiteSpace(tenLop) ||
        string.IsNullOrWhiteSpace(khoaHoc) ||
        string.IsNullOrWhiteSpace(ngonNgu) ||
        string.IsNullOrWhiteSpace(giaoVien) ||
        string.IsNullOrWhiteSpace(ngayHocStr) ||
        string.IsNullOrWhiteSpace(gioBatDau) ||
        string.IsNullOrWhiteSpace(gioKetThuc) ||
        string.IsNullOrWhiteSpace(phongHoc) ||
        string.IsNullOrWhiteSpace(trangThai))
    {
        TempData["Error"] = "Vui lòng nhập đầy đủ thông tin (kể cả ngày học, giờ, phòng).";
        return RedirectToAction(nameof(LopHoc));
    }

    // Kiểm tra trùng lịch (phòng + giáo viên)
    var (trung, msg) = await KiemTraTrungLichAsync(
        ngayHocStr, gioBatDau, gioKetThuc, phongHoc, giaoVien);

    if (trung)
    {
        TempData["Error"] = msg;
        return RedirectToAction(nameof(LopHoc));
    }

    int soLuong = await _context.LopHocs.CountAsync();
    string maLop = $"LH{soLuong + 1:D3}";
    string caHoc = TaoChuoiCaHoc(ngayHocStr, gioBatDau, gioKetThuc, phongHoc);

    var lop = new LopHoc
    {
        MaLop = maLop,
        TenLop = tenLop.Trim(),
        KhoaHoc = khoaHoc.Trim(),
        NgonNgu = ngonNgu.Trim(),
        GiaoVien = giaoVien.Trim(),
        SiSoToiDa = siSoToiDa,
        SiSoHienTai = 0,
        NgayHoc = ngayHocStr.Trim().ToUpper(),
        GioBatDau = gioBatDau.Trim(),
        GioKetThuc = gioKetThuc.Trim(),
        PhongHoc = phongHoc.Trim().ToUpper(),
        CaHoc = caHoc,
        TrangThai = trangThai.Trim()
    };

    _context.LopHocs.Add(lop);
    await _context.SaveChangesAsync();

    TempData["Success"] = $"Tạo lớp thành công. Mã: {maLop}. Ca: {caHoc}";
    return RedirectToAction(nameof(LopHoc));
}

        // =============================
        // SỬA LỚP HỌC
        // =============================
        
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> SuaLopHoc(
    int id,
    string tenLop,
    string khoaHoc,
    string ngonNgu,
    string giaoVien,
    int siSoToiDa,
    int siSoHienTai,
    string[] ngayHoc,
    string gioBatDau,
    string gioKetThuc,
    string phongHoc,
    string trangThai)
{
    var vaiTro = HttpContext.Session.GetString("VaiTro");
    if (vaiTro != "QuanLy")
        return RedirectToAction("Login", "Account");

    var lop = await _context.LopHocs.FindAsync(id);
    if (lop == null)
    {
        TempData["Error"] = "Không tìm thấy lớp học.";
        return RedirectToAction(nameof(LopHoc));
    }

    string ngayHocStr = (ngayHoc != null && ngayHoc.Length > 0)
        ? string.Join(",", ngayHoc)
        : "";

    if (string.IsNullOrWhiteSpace(tenLop) ||
        string.IsNullOrWhiteSpace(khoaHoc) ||
        string.IsNullOrWhiteSpace(ngonNgu) ||
        string.IsNullOrWhiteSpace(giaoVien) ||
        string.IsNullOrWhiteSpace(ngayHocStr) ||
        string.IsNullOrWhiteSpace(gioBatDau) ||
        string.IsNullOrWhiteSpace(gioKetThuc) ||
        string.IsNullOrWhiteSpace(phongHoc) ||
        string.IsNullOrWhiteSpace(trangThai))
    {
        TempData["Error"] = "Vui lòng nhập đầy đủ thông tin.";
        return RedirectToAction(nameof(LopHoc));
    }

    var (trung, msg) = await KiemTraTrungLichAsync(
        ngayHocStr, gioBatDau, gioKetThuc, phongHoc, giaoVien, excludeId: id);

    if (trung)
    {
        TempData["Error"] = msg;
        return RedirectToAction(nameof(LopHoc));
    }

    lop.TenLop = tenLop.Trim();
    lop.KhoaHoc = khoaHoc.Trim();
    lop.NgonNgu = ngonNgu.Trim();
    lop.GiaoVien = giaoVien.Trim();
    lop.SiSoToiDa = siSoToiDa;
    lop.SiSoHienTai = siSoHienTai;
    lop.NgayHoc = ngayHocStr.Trim().ToUpper();
    lop.GioBatDau = gioBatDau.Trim();
    lop.GioKetThuc = gioKetThuc.Trim();
    lop.PhongHoc = phongHoc.Trim().ToUpper();
    lop.CaHoc = TaoChuoiCaHoc(lop.NgayHoc, lop.GioBatDau, lop.GioKetThuc, lop.PhongHoc);
    lop.TrangThai = trangThai.Trim();

    await _context.SaveChangesAsync();

    TempData["Success"] = "Cập nhật lớp học thành công.";
    return RedirectToAction(nameof(LopHoc));
}

        // =============================
        // XÓA LỚP HỌC
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaLopHoc(int id)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "QuanLy")
            {
                return RedirectToAction("Login", "Account");
            }

            var lop = await _context.LopHocs.FindAsync(id);
            if (lop == null)
            {
                TempData["Error"] = "Không tìm thấy lớp học.";
                return RedirectToAction(nameof(LopHoc));
            }

            _context.LopHocs.Remove(lop);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa lớp học thành công.";
            return RedirectToAction(nameof(LopHoc));
        }

        // =============================
        // QUẢN LÝ LỊCH HỌC
        // =============================
       public async Task<IActionResult> LichHoc()
{
    var vaiTro = HttpContext.Session.GetString("VaiTro");
    if (vaiTro != "QuanLy")
        return RedirectToAction("Login", "Account");

    ViewBag.HoTen = HttpContext.Session.GetString("HoTen");

    var lopHocs = await _context.LopHocs
        .Where(x => x.TrangThai != "Đã kết thúc")
        .OrderBy(x => x.GioBatDau)
        .ToListAsync();

    ViewBag.BuoiSang = lopHocs.Count(x =>
        TimeSpan.TryParse(x.GioBatDau, out var t) && t < TimeSpan.FromHours(12));
    ViewBag.BuoiChieu = lopHocs.Count(x =>
        TimeSpan.TryParse(x.GioBatDau, out var t) && t >= TimeSpan.FromHours(12) && t < TimeSpan.FromHours(17));
    ViewBag.BuoiToi = lopHocs.Count(x =>
        TimeSpan.TryParse(x.GioBatDau, out var t) && t >= TimeSpan.FromHours(17));
    ViewBag.TongLop = lopHocs.Count;

    return View(lopHocs);
}

        // =============================
        // QUẢN LÝ HỌC PHÍ
        // =============================
        public async Task<IActionResult> HocPhi(string? q, string? trangThai)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "QuanLy")
                return RedirectToAction("Login", "Account");

            ViewBag.HoTen = HttpContext.Session.GetString("HoTen");

            var query = _context.HocViens.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(x =>
                    x.HoTen.Contains(q) || x.MaHocVien.Contains(q) ||
                    x.KhoaHoc.Contains(q) || x.LopHoc.Contains(q));
            }

            var list = await query.OrderBy(x => x.MaHocVien).ToListAsync();
            // Đồng bộ SoTienPhaiDong từ giá khóa học
            var khoaHocs = await _context.KhoaHocs.ToListAsync();
            bool coThayDoi = false;
            foreach (var hv in list)
            {
                var kh = khoaHocs.FirstOrDefault(k =>
                    string.Equals(k.TenKhoaHoc, hv.KhoaHoc, StringComparison.OrdinalIgnoreCase));
                if (kh != null && hv.SoTienPhaiDong != kh.Gia)
                {
                    hv.SoTienPhaiDong = kh.Gia;
                    // Không cho đã đóng vượt quá học phí mới
                    if (hv.SoTienDaDong > hv.SoTienPhaiDong)
                        hv.SoTienDaDong = hv.SoTienPhaiDong;
                    coThayDoi = true;
                }
            }
            if (coThayDoi)
                await _context.SaveChangesAsync();
            if (!string.IsNullOrWhiteSpace(trangThai) && trangThai != "Tất cả")
                list = list.Where(x => x.TrangThaiHocPhi == trangThai).ToList();

            decimal tongPhaiDong = list.Sum(x => x.SoTienPhaiDong);
            decimal tongDaDong = list.Sum(x => x.SoTienDaDong);
            decimal tongConNo = list.Sum(x => x.ConNo);
            decimal tyLeThu = tongPhaiDong > 0
                ? Math.Round(tongDaDong * 100m / tongPhaiDong, 1) : 0;

            ViewBag.TongPhaiDong = tongPhaiDong;
            ViewBag.TongDaDong = tongDaDong;
            ViewBag.TongConNo = tongConNo;
            ViewBag.TyLeThu = tyLeThu;
            ViewBag.SearchQ = q ?? "";
            ViewBag.FilterTrangThai = trangThai ?? "Tất cả";


            var theoKhoa = list
                .GroupBy(x => string.IsNullOrWhiteSpace(x.KhoaHoc) ? "Khác" : x.KhoaHoc)
                .Select(g => new {
                    Ten = g.Key,
                    DaDong = g.Sum(x => x.SoTienDaDong),
                    ConNo = g.Sum(x => x.ConNo),
                    PhaiDong = g.Sum(x => x.SoTienPhaiDong)
                })
                .OrderByDescending(x => x.PhaiDong)
                .Take(8)
                .ToList();

            ViewBag.ChartLabels = theoKhoa.Select(x => x.Ten).ToList();
            ViewBag.ChartDaDong = theoKhoa.Select(x => x.DaDong).ToList();
            ViewBag.ChartConNo = theoKhoa.Select(x => x.ConNo).ToList();
            ViewBag.PieDaDong = list.Count(x => x.TrangThaiHocPhi == "Đã đóng");
            ViewBag.PieChuaDu = list.Count(x => x.TrangThaiHocPhi == "Chưa đủ");
            ViewBag.PieChuaDong = list.Count(x => x.TrangThaiHocPhi == "Chưa đóng");
            ViewBag.PieChuaGan = list.Count(x => x.TrangThaiHocPhi == "Chưa gán");

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GhiNhanThanhToan(int id, decimal soTien, string? ghiChu)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "QuanLy")
                return RedirectToAction("Login", "Account");

            var hv = await _context.HocViens.FindAsync(id);
            if (hv == null)
            {
                TempData["Error"] = "Không tìm thấy học viên.";
                return RedirectToAction(nameof(HocPhi));
            }
            if (soTien <= 0)
            {
                TempData["Error"] = "Số tiền phải lớn hơn 0.";
                return RedirectToAction(nameof(HocPhi));
            }

            hv.SoTienDaDong += soTien;
            if (hv.SoTienDaDong > hv.SoTienPhaiDong)
                hv.SoTienDaDong = hv.SoTienPhaiDong;
            hv.NgayDongCuoi = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Đã ghi nhận {soTien:N0} ₫ cho {hv.HoTen}. Còn nợ: {hv.ConNo:N0} ₫.";
            return RedirectToAction(nameof(HocPhi));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatHocPhi(int id, decimal soTienPhaiDong, decimal soTienDaDong)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "QuanLy")
                return RedirectToAction("Login", "Account");

            var hv = await _context.HocViens.FindAsync(id);
            if (hv == null)
            {
                TempData["Error"] = "Không tìm thấy học viên.";
                return RedirectToAction(nameof(HocPhi));
            }
            if (soTienPhaiDong < 0 || soTienDaDong < 0)
            {
                TempData["Error"] = "Số tiền không được âm.";
                return RedirectToAction(nameof(HocPhi));
            }

            hv.SoTienPhaiDong = soTienPhaiDong;
            hv.SoTienDaDong = Math.Min(soTienDaDong, soTienPhaiDong);
            if (soTienDaDong > 0)
                hv.NgayDongCuoi = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật học phí cho {hv.HoTen}.";
            return RedirectToAction(nameof(HocPhi));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DongBoHocPhiTuKhoaHoc()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != "QuanLy")
                return RedirectToAction("Login", "Account");

            var khoaHocs = await _context.KhoaHocs.ToListAsync();
            var hocViens = await _context.HocViens
                .Where(x => x.SoTienPhaiDong == 0).ToListAsync();
            int count = 0;

            foreach (var hv in hocViens)
            {
                var kh = khoaHocs.FirstOrDefault(k =>
                    string.Equals(k.TenKhoaHoc, hv.KhoaHoc, StringComparison.OrdinalIgnoreCase));
                if (kh != null && kh.Gia > 0)
                {
                    hv.SoTienPhaiDong = kh.Gia;
                    count++;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã đồng bộ học phí từ khóa học cho {count} học viên.";
            return RedirectToAction(nameof(HocPhi));
        }

        // =============================
// QUẢN LÝ KẾT QUẢ HỌC TẬP
// =============================
public async Task<IActionResult> KetQuaHocTap(
    string? tuKhoa = null,
    string? lop = null,
    string? ketQua = null)
{
    var vaiTro = HttpContext.Session.GetString("VaiTro");
    if (vaiTro != "QuanLy")
        return RedirectToAction("Login", "Account");

    ViewBag.HoTen = HttpContext.Session.GetString("HoTen");

    var hocVienQuery = _context.HocViens.AsQueryable();

    if (!string.IsNullOrWhiteSpace(tuKhoa))
    {
        tuKhoa = tuKhoa.Trim();
        var keyword = tuKhoa.ToLower();
        hocVienQuery = hocVienQuery.Where(x =>
            x.HoTen.ToLower().Contains(keyword) ||
            x.MaHocVien.ToLower().Contains(keyword));
    }

    if (!string.IsNullOrWhiteSpace(lop) && lop != "Tất cả")
    {
        hocVienQuery = hocVienQuery.Where(x => x.LopHoc == lop);
    }

    var hocViens = await hocVienQuery
        .OrderBy(x => x.HoTen)
        .ToListAsync();

    var hocVienIds = hocViens.Select(x => x.Id).ToList();
    var diemSoByHocVien = await _context.DiemSos
        .Where(x => hocVienIds.Contains(x.HocVienId))
        .ToListAsync();

    var danhSach = hocViens
        .Select(hv =>
        {
            var ds = diemSoByHocVien.FirstOrDefault(x => x.HocVienId == hv.Id);
            return new DiemSo
            {
                Id = ds?.Id ?? 0,
                HocVienId = hv.Id,
                LopHocId = ds?.LopHocId ?? 0,
                MaHocVien = hv.MaHocVien,
                HoTenHocVien = hv.HoTen,
                MaLop = ds?.MaLop ?? hv.LopHoc,
                TenLop = ds?.TenLop ?? hv.LopHoc,
                DiemKT1 = ds?.DiemKT1,
                DiemKT2 = ds?.DiemKT2,
                DiemKT3 = ds?.DiemKT3,
                DiemKiemTraLon = ds?.DiemKiemTraLon,
                GhiChu = ds?.GhiChu ?? "",
                GiaoVienNhap = ds?.GiaoVienNhap ?? "",
                NgayCapNhat = ds?.NgayCapNhat ?? DateTime.Now
            };
        })
        .Where(x =>
        {
            if (string.IsNullOrWhiteSpace(ketQua) || ketQua == "Tất cả") return true;
            if (ketQua == "Đạt") return x.DiemKiemTraLon >= 8.0m;
            if (ketQua == "Không đạt") return x.DiemKiemTraLon != null && x.DiemKiemTraLon < 8.0m;
            return x.DiemKiemTraLon == null;
        })
        .OrderByDescending(x => x.NgayCapNhat)
        .ToList();

    // ===== Thống kê =====
    var tong = danhSach.Count;
    var dat = danhSach.Count(x => x.KetQua == "Đạt");
    var khongDat = danhSach.Count(x => x.KetQua == "Không đạt");
    var chuaThi = danhSach.Count(x => x.KetQua == "Chưa thi");

    var diemTB = danhSach
        .Where(x => x.DiemTrungBinh != null)
        .Select(x => x.DiemTrungBinh!.Value)
        .DefaultIfEmpty(0)
        .Average();

    ViewBag.Tong = tong;
    ViewBag.Dat = dat;
    ViewBag.KhongDat = khongDat;
    ViewBag.ChuaThi = chuaThi;
    ViewBag.DiemTrungBinh = Math.Round(diemTB, 1);
    ViewBag.TyLeDat = tong > 0 ? Math.Round((double)dat / tong * 100, 1) : 0;

    // Danh sách lớp để dropdown lọc
    ViewBag.DanhSachLop = await _context.LopHocs
        .Where(x => !string.IsNullOrWhiteSpace(x.TenLop))
        .Select(x => x.TenLop)
        .Distinct()
        .OrderBy(x => x)
        .ToListAsync();

    // Giữ lại giá trị filter để hiển thị lại trên form
    ViewBag.TuKhoa = tuKhoa;
    ViewBag.Lop = lop;
    ViewBag.KetQua = ketQua;

    return View(danhSach);
}

        // =============================
// THỐNG KÊ
// =============================
public async Task<IActionResult> ThongKe()
{
    var vaiTro = HttpContext.Session.GetString("VaiTro");
    if (vaiTro != "QuanLy")
        return RedirectToAction("Login", "Account");

    ViewBag.HoTen = HttpContext.Session.GetString("HoTen");

    // ========== 1. SĨ SỐ ==========
    var tongHocVien = await _context.HocViens.CountAsync();
    var dangHoc = await _context.HocViens.CountAsync(x => x.TrangThai == "Đang học");
    var tamNgung = await _context.HocViens.CountAsync(x => x.TrangThai == "Tạm ngưng");
    var daKetThuc = await _context.HocViens.CountAsync(x => x.TrangThai == "Đã kết thúc");

    var tongLop = await _context.LopHocs.CountAsync();
    var lopDangHoatDong = await _context.LopHocs
        .CountAsync(x => x.TrangThai == "Đang học" || x.TrangThai == "Sắp khai giảng");

    // Sĩ số trung bình / lớp (chỉ lấy lớp đang hoạt động)
    var siSoTrungBinh = await _context.LopHocs
        .Where(x => x.TrangThai != "Đã kết thúc")
        .AverageAsync(x => (double?)x.SiSoHienTai) ?? 0;

    // ========== 2. DOANH THU ==========
    decimal doanhThuThucTe = await _context.HocViens.SumAsync(x => (decimal?)x.SoTienDaDong) ?? 0;
    decimal doanhThuDuKien = await _context.HocViens.SumAsync(x => (decimal?)x.SoTienPhaiDong) ?? 0;
    decimal tongConNo = await _context.HocViens
        .SumAsync(x => x.SoTienPhaiDong > x.SoTienDaDong
            ? x.SoTienPhaiDong - x.SoTienDaDong : 0);

    // ========== 3. TỶ LỆ HOÀN THÀNH ==========
    double tyLeHoanThanh = tongHocVien > 0
        ? Math.Round((double)daKetThuc / tongHocVien * 100, 1)
        : 0;

    // ========== DỮ LIỆU CHI TIẾT THEO KHÓA ==========
    var thongKeTheoKhoa = await _context.HocViens
        .GroupBy(x => x.KhoaHoc)
        .Select(g => new
        {
            TenKhoa = g.Key,
            SiSo = g.Count(),
            DangHoc = g.Count(x => x.TrangThai == "Đang học"),
            DaKetThuc = g.Count(x => x.TrangThai == "Đã kết thúc"),
            DoanhThu = g.Sum(x => x.SoTienDaDong),
            TyLeHoanThanh = g.Count() > 0
                ? Math.Round((double)g.Count(x => x.TrangThai == "Đã kết thúc") / g.Count() * 100, 1)
                : 0
        })
        .OrderByDescending(x => x.SiSo)
        .ToListAsync();

    // ========== DỮ LIỆU THEO LỚP ==========
    var thongKeTheoLop = await _context.LopHocs
        .OrderByDescending(x => x.SiSoHienTai)
        .Select(x => new
        {
            x.TenLop,
            x.KhoaHoc,
            x.SiSoHienTai,
            x.SiSoToiDa,
            TyLeLapDay = x.SiSoToiDa > 0
                ? Math.Round((double)x.SiSoHienTai / x.SiSoToiDa * 100, 1)
                : 0,
            x.TrangThai
        })
        .ToListAsync();

    // Đưa dữ liệu ra View
    ViewBag.TongHocVien = tongHocVien;
    ViewBag.DangHoc = dangHoc;
    ViewBag.TamNgung = tamNgung;
    ViewBag.DaKetThuc = daKetThuc;
    ViewBag.TongLop = tongLop;
    ViewBag.LopDangHoatDong = lopDangHoatDong;
    ViewBag.SiSoTrungBinh = Math.Round(siSoTrungBinh, 1);

    ViewBag.DoanhThuThucTe = doanhThuThucTe;
    ViewBag.DoanhThuDuKien = doanhThuDuKien;
    ViewBag.TongConNo = tongConNo;

    ViewBag.TyLeHoanThanh = tyLeHoanThanh;

    ViewBag.ThongKeTheoKhoa = thongKeTheoKhoa;
    ViewBag.ThongKeTheoLop = thongKeTheoLop;

    return View();
}

        // =============================
        // API THÊM KHÓA HỌC
        // =============================
        [HttpPost]
        public async Task<IActionResult> ThemKhoaHoc([FromBody] KhoaHoc model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.KhoaHocs.Add(model);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Thêm khóa học thành công"
            });
        }

        // =============================
        // API SỬA KHÓA HỌC
        // =============================
        [HttpPut]
        public async Task<IActionResult> SuaKhoaHoc(int id, [FromBody] KhoaHoc model)
        {
            var khoaHoc = await _context.KhoaHocs.FindAsync(id);

            if (khoaHoc == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy khóa học"
                });
            }

            khoaHoc.TenKhoaHoc = model.TenKhoaHoc;
            khoaHoc.NgonNgu = model.NgonNgu;
            khoaHoc.SoBuoi = model.SoBuoi;
            khoaHoc.Gia = model.Gia;
            khoaHoc.MoTa = model.MoTa;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Cập nhật khóa học thành công"
            });
        }

        // =============================
        // API XÓA KHÓA HỌC
        // =============================
        [HttpDelete]
        public async Task<IActionResult> XoaKhoaHoc(int id)
        {
            var khoaHoc = await _context.KhoaHocs.FindAsync(id);

            if (khoaHoc == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy khóa học"
                });
            }

            _context.KhoaHocs.Remove(khoaHoc);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Xóa khóa học thành công"
            });
        }

        // =============================
        // TỰ ĐỘNG CẬP NHẬT SĨ SỐ LỚP
        // =============================
        private async Task CapNhatSiSoLopAsync(string? tenLop)
        {
            if (string.IsNullOrWhiteSpace(tenLop))
                return;

            var lop = await _context.LopHocs
                .FirstOrDefaultAsync(x => x.TenLop == tenLop.Trim());

            if (lop == null)
                return;

            lop.SiSoHienTai = await _context.HocViens
                .CountAsync(x => x.LopHoc == lop.TenLop && x.TrangThai == "Đang học");

            await _context.SaveChangesAsync();
        }
        // =============================
// KIỂM TRA TRÙNG LỊCH
// =============================
private async Task<(bool trung, string message)> KiemTraTrungLichAsync(
    string ngayHoc,
    string gioBatDau,
    string gioKetThuc,
    string phongHoc,
    string giaoVien,
    int? excludeId = null)
{
    if (string.IsNullOrWhiteSpace(ngayHoc) ||
        string.IsNullOrWhiteSpace(gioBatDau) ||
        string.IsNullOrWhiteSpace(gioKetThuc) ||
        string.IsNullOrWhiteSpace(phongHoc))
    {
        return (false, "");
    }

    var cacThu = ngayHoc.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(x => x.ToUpper())
                        .ToHashSet();

    if (!TimeSpan.TryParse(gioBatDau, out var batDau) ||
        !TimeSpan.TryParse(gioKetThuc, out var ketThuc))
    {
        return (true, "Giờ học không hợp lệ (định dạng HH:mm).");
    }

    if (ketThuc <= batDau)
        return (true, "Giờ kết thúc phải sau giờ bắt đầu.");

    var danhSachLop = await _context.LopHocs
        .Where(x => x.TrangThai != "Đã kết thúc")
        .Where(x => excludeId == null || x.Id != excludeId)
        .ToListAsync();

    foreach (var lop in danhSachLop)
    {
        if (string.IsNullOrWhiteSpace(lop.NgayHoc)) continue;

        var thuLopKhac = lop.NgayHoc.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                    .Select(x => x.ToUpper())
                                    .ToHashSet();

        // Có chung ít nhất 1 thứ
        if (!cacThu.Overlaps(thuLopKhac)) continue;

        if (!TimeSpan.TryParse(lop.GioBatDau, out var bdKhac) ||
            !TimeSpan.TryParse(lop.GioKetThuc, out var ktKhac))
            continue;

        // Có chồng giờ không? (overlap)
        bool chongGio = batDau < ktKhac && ketThuc > bdKhac;
        if (!chongGio) continue;

        // 1. Trùng phòng
        if (string.Equals(lop.PhongHoc?.Trim(), phongHoc.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return (true,
                $"Trùng phòng {phongHoc} với lớp {lop.TenLop} ({lop.NgayHoc}, {lop.GioBatDau}-{lop.GioKetThuc}).");
        }

        // 2. Trùng giáo viên
        if (string.Equals(lop.GiaoVien?.Trim(), giaoVien?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return (true,
                $"Giáo viên {giaoVien} đang dạy lớp {lop.TenLop} cùng khung giờ ({lop.NgayHoc}, {lop.GioBatDau}-{lop.GioKetThuc}).");
        }
    }

    return (false, "");
}

private static string TaoChuoiCaHoc(string ngayHoc, string gioBatDau, string gioKetThuc, string phongHoc)
{
    var thu = ngayHoc.Replace(",", "-").Replace(" ", "");
    return $"{thu}, {gioBatDau}-{gioKetThuc}, {phongHoc}";
}
    }
}