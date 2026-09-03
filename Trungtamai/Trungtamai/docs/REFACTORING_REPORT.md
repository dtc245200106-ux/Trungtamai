# Báo Cáo Refactoring Phân Quyền

## Tóm Tắt Công Việc

Hoàn thành việc refactor hệ thống phân quyền từ kiểm tra vai trò thủ công sang sử dụng **Custom Authorization Attribute** (`[AuthorizeRole]`).

## Prompt Quá Trình Tạo Ứng Dụng

Dưới đây là một đoạn prompt mẫu mô tả quy trình tạo ứng dụng Trung tâm Ngoại ngữ bằng AI hỗ trợ, phục vụ việc ghi nhận và minh chứng quá trình phát triển:

```text
Hãy giúp tôi xây dựng một hệ thống quản lý trung tâm ngoại ngữ online với các chức năng:
- Quản lý học viên, giáo viên, quản lý trung tâm, tư vấn viên
- Quản lý khóa học, lớp học, lịch học, học phí, điểm số
- Đăng nhập, phân quyền theo vai trò: QuanLy, GiaoVien, HocVien, TuVanVien
- Giao diện web theo mô hình MVC ASP.NET Core
- Tích hợp chatbot AI để hỗ trợ học viên và giáo viên
- Chatbot phải trả lời theo dữ liệu được phép truy cập theo role
- Lưu nhật ký prompt/response AI để kiểm tra lịch sử sử dụng
- Cấu hình API key Gemini bằng appsettings hoặc biến môi trường
- Hiển thị chatbot rõ ràng, dễ đọc, có cấu trúc theo heading, bullet, câu hỏi, đáp án
- Bảo mật quyền truy cập theo session và phân quyền rõ ràng
- Cho phép quản lý trung tâm có thể truy cập chatbot nếu được phép
- Tạo báo cáo tài liệu mô tả quy trình, refactor và chức năng chính
```

### Mục tiêu của prompt

- Xác định phạm vi nghiệp vụ của hệ thống trung tâm ngoại ngữ
- Tạo cấu trúc backend/frontend phù hợp với ASP.NET Core MVC
- Thiết kế phân quyền theo role rõ ràng, dễ mở rộng
- Tích hợp AI chatbot với kiểm soát quyền truy cập dữ liệu
- Lưu log minh chứng cho việc AI được sử dụng trong ứng dụng
- Tạo tài liệu hoặc báo cáo kỹ thuật để theo dõi tiến độ phát triển

### Ví dụ prompt cho từng giai đoạn

```text
Tạo hệ thống quản lý trung tâm ngoại ngữ bằng ASP.NET Core MVC. Tạo cơ sở dữ liệu với các bảng: User, Role, LopHoc, HocVien, GiaoVien, LichHoc, DiemSo, HocPhi, KhoaHoc, Tuvan. Thiết kế phân quyền theo vai trò QuanLy, GiaoVien, HocVien, TuVanVien.
```

```text
Hãy refactor lại phân quyền bằng custom authorization attribute, không kiểm tra role thủ công ở từng action. Tạo AuthorizeRoleAttribute để tự động kiểm tra Session UserId và VaiTro, redirect nếu chưa đăng nhập và trả về 403 nếu không đủ quyền.
```

```text
Tích hợp Gemini API vào chatbot của hệ thống. Chatbot phải nhận câu hỏi từ người dùng, kiểm tra role và chỉ trả lời dựa trên dữ liệu được phép xem. Lưu prompt, response và lỗi vào log JSON để phục vụ kiểm tra sau này.
```

```text
Hiển thị output chatbot dễ nhìn hơn bằng cách format heading, danh sách, nội dung mục tiêu, đề bài, đáp án, gợi ý. Hỗ trợ nhiều ngôn ngữ, không chỉ tiếng Anh. Tạo UI thân thiện cho học viên khi yêu cầu tạo bài tập.
```

```text
Kiểm tra lỗi chạy ứng dụng: connection string chưa được khởi tạo, view không tìm thấy, role bị chặn. Sửa cấu hình, routing, view path và logic điều kiện quyền tương ứng.
```

### Bản 2: Prompt chuyên nghiệp để trình bày báo cáo / hội đồng

```text
Hãy hỗ trợ tôi phát triển và hoàn thiện hệ thống quản lý trung tâm ngoại ngữ online theo mô hình ASP.NET Core MVC. Hệ thống cần đáp ứng đầy đủ các chức năng quản lý học viên, giáo viên, quản lý trung tâm, tư vấn viên, lớp học, khóa học, lịch học, học phí, điểm số và chatbot AI hỗ trợ người dùng. Cần thiết kế phân quyền rõ ràng theo từng vai trò: QuanLy, GiaoVien, HocVien và TuVanVien, đồng thời đảm bảo mỗi vai trò chỉ truy cập đúng dữ liệu được phép. Hãy triển khai refactor phân quyền bằng custom authorization attribute để giảm lặp code, tăng tính nhất quán và bảo mật. Bên cạnh đó, tích hợp AI Gemini vào chatbot để hỗ trợ học viên và giáo viên trong việc trả lời câu hỏi, tạo bài tập, và quản lý thông tin liên quan đến lớp học, điểm số và lịch học. Yêu cầu chatbot phải kiểm tra quyền truy cập theo role trước khi trả lời, chỉ sử dụng dữ liệu được phép xem, đồng thời lưu nhật ký prompt, response và lỗi vào file log JSON phục vụ kiểm tra và minh chứng quá trình phát triển. Cần tối ưu giao diện chatbot để hiển thị nội dung rõ ràng, dễ đọc, theo cấu trúc heading, list, câu hỏi, đáp án và gợi ý. Cuối cùng, hãy đảm bảo ứng dụng có thể chạy ổn định, xử lý đúng các lỗi về connection string, routing, view path, session, và role-based access control, và tạo báo cáo kỹ thuật ngắn gọn nhưng rõ ràng cho quá trình phát triển và hoàn thiện hệ thống.
```

## Các Thay Đổi Chính

### 1. Tạo Custom Authorization Attribute ✅

- **File**: `backend/Attributes/AuthorizeRoleAttribute.cs`
- **Tính Năng**:
  - Kiểm tra Session UserId và VaiTro
  - Tự động chuyển hướng đến Login nếu chưa xác thực
  - Trả về 403 Forbidden nếu vai trò không được phép
  - Hỗ trợ nhiều vai trò cùng lúc

### 2. Refactor Controllers ✅

#### GiaoVienController

- ✅ Thêm `[AuthorizeRole("GiaoVien")]` vào class
- ✅ Xóa 9 kiểm tra vai trò thủ công từ các methods:
  - DiemSoKetQua()
  - LayDiemTheoLop()
  - LuuDiemSo()
  - Index()
  - LichDay()
  - LopHocCuaToi()
  - ChiTietLop()
  - LayDuLieuDiemDanh()
  - LuuDiemDanh()

#### HocVienController

- ✅ Thêm `[AuthorizeRole("HocVien")]` vào class
- ✅ Xóa 4 kiểm tra vai trò thủ công từ các methods:
  - LichHoc()
  - LopCuaToi()
  - ChiTietLop()
  - Index()

#### TuVanVienController

- ✅ Thêm `[AuthorizeRole("TuVanVien")]` vào class
- ✅ Xóa 2 kiểm tra vai trò thủ công từ các methods:
  - Index()
  - KhoaHocMoLop()

#### QuanLyController

- ✅ Thêm `[AuthorizeRole("QuanLy")]` vào class
- ✅ Xóa 1 kiểm tra vai trò thủ công từ method:
  - Index()

#### ChatbotController

- ✅ Thêm `[AuthorizeRole("GiaoVien", "HocVien", "TuVanVien")]`
- ✅ Vẫn giữ lại kiểm tra chi tiết vai trò để hỗ trợ logic không hỗ trợ chatbot

### 3. Controllers Không Thay Đổi

- **AccountController**: Không cần bảo vệ (Login/Logout)
- **HomeController**: Trang chủ công khai

### 4. Tài Liệu Hóa ✅

- Tạo `docs/AUTHORIZATION_SYSTEM.md` (Tiếng Việt)
  - Hướng dẫn sử dụng custom attribute
  - Mô tả các vai trò
  - Ví dụ code
  - Hướng dẫn mở rộng
  - Troubleshooting

## Kết Quả Xây Dựng

✅ **Build Thành Công**

```
Restore complete (0,6s)
Trungtamai net10.0 succeeded (2,9s) → bin\Debug\net10.0\Trungtamai.dll
Build succeeded in 4,3s
```

## Lợi Ích của Refactor

| Khía Cạnh        | Trước                                              | Sau                              |
| ---------------- | -------------------------------------------------- | -------------------------------- |
| **Lặp Lại Code** | 16+ lần kiểm tra vai trò thủ công                  | 1 attribute tái sử dụng          |
| **Bảo Trì**      | Khó khăn khi thay đổi logic                        | Tập trung tại một chỗ            |
| **Consistency**  | Không nhất quán, mix RedirectToAction/Unauthorized | Thống nhất qua filter            |
| **Dễ Hiểu**      | Phải đọc từng method để hiểu bảo vệ                | Rõ ràng qua attribute trên class |
| **Kiểm Thử**     | Khó kiểm thử từng case                             | Có thể test filter riêng         |

## Công Việc Sắp Tới (Tùy Chọn)

1. **Bảo Mật**: Thêm mã hóa mật khẩu (bcrypt/Argon2)
2. **Session**: Thêm refresh token mechanism
3. **Testing**: Viết unit tests cho `AuthorizeRoleAttribute`
4. **Logging**: Thêm logging khi access denied
5. **Claims-based**: Nâng cấp sang claims-based authorization nếu cần thiết

## Tệp Được Sửa Đổi

```
backend/
├── Attributes/
│   └── AuthorizeRoleAttribute.cs          [NEW]
├── Controllers/
│   ├── GiaoVienController.cs              [MODIFIED]
│   ├── HocVienController.cs               [MODIFIED]
│   ├── TuVanVienController.cs             [MODIFIED]
│   ├── QuanLyController.cs                [MODIFIED]
│   ├── ChatbotController.cs               [MODIFIED]
│   ├── AccountController.cs               [NO CHANGE]
│   └── HomeController.cs                  [NO CHANGE]
└── Data/
    └── AppDbContext.cs                    [COPIED]

database/
└── Migrations/                            [SYNCED to backend]

docs/
└── AUTHORIZATION_SYSTEM.md                [NEW - Documentation]
```

## Kiểm Tra Nhanh

### Để Sử Dụng Custom Attribute:

```csharp
using Trungtamai.Attributes;

[AuthorizeRole("VaiTroName")]
public class MyController : Controller
{
    // Tất cả actions được bảo vệ
}
```

### Để Bảo Vệ Action Cụ Thể:

```csharp
public class MyController : Controller
{
    [AuthorizeRole("Admin")]
    public IActionResult AdminOnly() { }
}
```

---

**Ngày Hoàn Thành**: 2026-08-19
**Trạng Thái**: ✅ Hoàn Thành & Xác Thực
