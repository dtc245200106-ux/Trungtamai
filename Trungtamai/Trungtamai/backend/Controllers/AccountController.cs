using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trungtamai.Data;

namespace Trungtamai.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(
            string TaiKhoan,
            string MatKhau)
        {
            if (string.IsNullOrWhiteSpace(TaiKhoan) ||
                string.IsNullOrWhiteSpace(MatKhau))
            {
                ViewBag.Error =
                    "Vui lòng nhập đầy đủ tài khoản và mật khẩu.";

                return View();
            }

            var nguoiDung = await _context.NguoiDungs
                .FirstOrDefaultAsync(x =>
                    x.TaiKhoan == TaiKhoan &&
                    x.MatKhau == MatKhau);

            if (nguoiDung == null)
            {
                ViewBag.Error =
                    "Tài khoản hoặc mật khẩu không đúng.";

                return View();
            }

            HttpContext.Session.SetString(
                "UserId",
                nguoiDung.Id.ToString());

            HttpContext.Session.SetString(
                "HoTen",
                nguoiDung.HoTen);

            HttpContext.Session.SetString(
                "VaiTro",
                nguoiDung.VaiTro);

            switch (nguoiDung.VaiTro)
            {
                case "QuanLy":
                    return RedirectToAction(
                        "Index",
                        "QuanLy");

                case "GiaoVien":
                    return RedirectToAction(
                        "Index",
                        "GiaoVien");

                case "HocVien":
                    return RedirectToAction(
                        "Index",
                        "HocVien");

                case "TuVanVien":
                    return RedirectToAction(
                        "Index",
                        "TuVanVien");

                default:
                    ViewBag.Error =
                        "Vai trò tài khoản không hợp lệ.";

                    return View();
            }
        }

        public IActionResult Logout()
        {
            if (int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
            {
                var history = _context.ChatMessages
                    .Where(x => x.UserId == userId);
                _context.ChatMessages.RemoveRange(history);
                _context.SaveChanges();
            }

            HttpContext.Session.Clear();

            return RedirectToAction(
                "Login",
                "Account");
        }
    }
}