# Hệ Thống Phân Quyền (Authorization System)

## Tổng Quan

Dự án sử dụng **Custom Authorization Attribute** (`AuthorizeRoleAttribute`) để quản lý phân quyền người dùng dựa trên các vai trò khác nhau.

## Các Vai Trò (Roles)

Hệ thống hỗ trợ 4 vai trò chính:

| Vai Trò       | Mô Tả            | Controllers         |
| ------------- | ---------------- | ------------------- |
| **QuanLy**    | Quản lý viên     | QuanLyController    |
| **GiaoVien**  | Giáo viên        | GiaoVienController  |
| **HocVien**   | Học viên         | HocVienController   |
| **TuVanVien** | Tư vấn viên      | TuVanVienController |
| **Tất cả**    | Truy cập chatbot | ChatbotController   |

## Cách Hoạt Động

### 1. Custom Authorization Attribute

File: `backend/Attributes/AuthorizeRoleAttribute.cs`

```csharp
[AuthorizeRole("GiaoVien")]
public class GiaoVienController : Controller { }
```

Attribute này:

- ✅ Kiểm tra xem user đã đăng nhập chưa (kiểm tra `UserId` trong Session)
- ✅ Kiểm tra xem user có vai trò được phép không
- ✅ Tự động chuyển hướng đến Login nếu chưa đăng nhập
- ✅ Trả về 403 Forbidden nếu vai trò không được phép

### 2. Session-based Authentication

Khi người dùng đăng nhập thành công, `AccountController` lưu trữ:

```csharp
HttpContext.Session.SetString("UserId", nguoiDung.Id.ToString());
HttpContext.Session.SetString("HoTen", nguoiDung.HoTen);
HttpContext.Session.SetString("VaiTro", nguoiDung.VaiTro);
```

## Cách Sử Dụng

### Bảo vệ toàn bộ Controller

```csharp
using Trungtamai.Attributes;

[AuthorizeRole("GiaoVien")]
public class GiaoVienController : Controller
{
    // Tất cả actions đều được bảo vệ
    public async Task<IActionResult> Index() { }
    public async Task<IActionResult> DiemSoKetQua() { }
}
```

### Bảo vệ Action cụ thể

```csharp
public class QuanLyController : Controller
{
    [AuthorizeRole("QuanLy")]
    public async Task<IActionResult> XoaHocVien(int id) { }

    [AuthorizeRole("QuanLy", "TuVanVien")]  // Nhiều vai trò
    public async Task<IActionResult> XemThongKe() { }
}
```

### Nhiều vai trò được phép

```csharp
[AuthorizeRole("GiaoVien", "HocVien", "TuVanVien")]
public class ChatbotController : ControllerBase { }
```

## Flow Phân Quyền

```
Người dùng truy cập trang
    ↓
AuthorizeRoleAttribute kiểm tra
    ↓
├─ Chưa đăng nhập? → Redirect to Login
├─ Vai trò không hợp lệ? → 403 Forbidden
└─ OK → Cho phép truy cập
```

## Các Controllers Đã Áp Dụng

| Controller          | Vai Trò                    | Ghi Chú                      |
| ------------------- | -------------------------- | ---------------------------- |
| GiaoVienController  | GiaoVien                   | Quản lý lớp, điểm, điểm danh |
| HocVienController   | HocVien                    | Xem lịch học, điểm, lớp      |
| TuVanVienController | TuVanVien                  | Tư vấn khóa học              |
| QuanLyController    | QuanLy                     | Quản lý toàn bộ hệ thống     |
| ChatbotController   | GiaoVien/HocVien/TuVanVien | Chatbot Gemini               |
| AccountController   | Không bảo vệ               | Đăng nhập/Đăng xuất          |
| HomeController      | Không bảo vệ               | Trang chủ                    |

## Logout

Khi đăng xuất, session sẽ bị xóa:

```csharp
public IActionResult Logout()
{
    HttpContext.Session.Clear();  // Xóa toàn bộ session
    return RedirectToAction("Login", "Account");
}
```

## Mở Rộng Hệ Thống

### Thêm Vai Trò Mới

1. Thêm vai trò mới vào bảng `NguoiDung` model
2. Cập nhật `AccountController` để xử lý vai trò mới
3. Tạo controller mới với `[AuthorizeRole("VaiTroMoi")]`

### Bảo Vệ Action Cụ Thể

Thay vì bảo vệ toàn bộ controller, có thể bảo vệ từng action:

```csharp
public class QuanLyController : Controller
{
    [AuthorizeRole("QuanLy")]
    public async Task<IActionResult> XoaHocVien(int id) { }

    public IActionResult TrangCong() { }  // Không bảo vệ
}
```

## Tính Năng Bảo Mật

✅ **Session Timeout** - Tự động timeout sau 8 giờ inactivity
✅ **HttpOnly Cookies** - Bảo vệ chống XSS
✅ **Role-based Access** - Kiểm soát quyền truy cập chi tiết
✅ **Clear Logout** - Xóa dữ liệu session khi đăng xuất

## Troubleshooting

### Vấn đề: User bị redirect về Login liên tục

**Giải pháp**: Kiểm tra xem `UserId` và `VaiTro` có được lưu đúng trong Session không

```csharp
// Debug trong action
var userId = HttpContext.Session.GetString("UserId");
var vaiTro = HttpContext.Session.GetString("VaiTro");
```

### Vấn đề: Trang báo lỗi 403 Forbidden

**Giải pháp**: Vai trò của user không khớp với vai trò yêu cầu. Kiểm tra vai trò trong database.

---

**Lưu ý**: Tất cả các manual check `if (vaiTro != "...")` đã được xóa khỏi code logic.
