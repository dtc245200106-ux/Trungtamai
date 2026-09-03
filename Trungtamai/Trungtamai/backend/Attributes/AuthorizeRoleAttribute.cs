using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Trungtamai.Attributes
{
    /// <summary>
    /// Custom authorization attribute để kiểm tra vai trò người dùng
    /// Sử dụng Session để lưu trữ VaiTro
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        /// <summary>
        /// Khởi tạo AuthorizeRoleAttribute
        /// </summary>
        /// <param name="roles">Danh sách vai trò được phép truy cập (ví dụ: "GiaoVien", "HocVien", "TuVanVien")</param>
        public AuthorizeRoleAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Kiểm tra xem user đã login hay chưa
            var userId = context.HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                // Chưa login - redirect về Login
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Lấy vai trò từ Session
            var vaiTro = context.HttpContext.Session.GetString("VaiTro");
            if (string.IsNullOrEmpty(vaiTro))
            {
                // Không có vai trò - redirect về Login
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Kiểm tra xem vai trò có nằm trong danh sách được phép không
            if (_allowedRoles.Length > 0 && !_allowedRoles.Contains(vaiTro))
            {
                // Không có quyền - trả về 403 Forbidden
                context.Result = new ForbidResult();
                return;
            }

            // Tất cả kiểm tra đều pass - cho phép truy cập
        }
    }
}
