# PROMPT: Xây dựng ứng dụng "TrungTamAi" – Hệ thống quản lý trung tâm ngoại ngữ

Hãy xây dựng cho tôi một ứng dụng web **ASP.NET Core MVC (.NET 10)** tên là **Trungtamai** — hệ thống quản lý trung tâm ngoại ngữ có tích hợp **chatbot AI (Gemini)** hỗ trợ theo vai trò người dùng. Yêu cầu chi tiết như sau:

## 1. Công nghệ sử dụng
- .NET 10 / ASP.NET Core MVC (Razor Views, không dùng Blazor/React)
- Entity Framework Core 10.0.5 + SQL Server (hỗ trợ cả LocalDB cho dev)
- Google Gemini API (model `gemini-2.0-flash`) cho chatbot AI
- Xác thực bằng **Session** (không dùng Identity/JWT), session timeout 8 giờ, cookie HttpOnly
- Bootstrap + jQuery + jQuery Validation cho giao diện (Views dùng Razor + Bootstrap có sẵn trong wwwroot/lib)

## 2. Cấu trúc thư mục (tách lớp rõ ràng dù vẫn là 1 project MVC)
```
Trungtamai/
├── backend/
│   ├── Attributes/AuthorizeRoleAttribute.cs
│   ├── Controllers/ (Account, Chatbot, GiaoVien, HocVien, Home, QuanLy, TuVanVien)
│   ├── Data/AppDbContext.cs
│   ├── Migrations/
│   ├── Models/
│   ├── Services/ (AILoggingService, ChatbotDataService, GeminiService)
│   ├── Views/ (bản chính thức được app dùng)
│   ├── Program.cs
│   └── Trungtamai.csproj
├── frontend/
│   ├── Views/ (bản đồng bộ/tham chiếu cho tách biệt UI)
│   └── wwwroot/ (css, js, lib bootstrap/jquery)
├── database/
│   ├── Data/AppDbContext.cs
│   └── Migrations/
├── config/ (appsettings.json, appsettings.Development.json)
└── docs/ (README.md, AUTHORIZATION_SYSTEM.md, CHATBOT_HANDLER_FLOW.md, CHATBOT_TEST_SCENARIOS.md, IMPLEMENTATION_READY.md)
```

## 3. Vai trò & phân quyền (RBAC theo Session, không dùng ASP.NET Identity)
Tạo 4 vai trò lưu trong bảng `NguoiDung` (cột `VaiTro`):
1. **QuanLy** (Quản lý) — toàn quyền: học viên, giáo viên, khóa học, lớp học, lịch học, học phí, kết quả học tập/điểm số, thống kê.
2. **GiaoVien** (Giáo viên) — chỉ xem lớp mình dạy, lịch dạy, học viên trong lớp, nhập/xem điểm số lớp mình.
3. **HocVien** (Học viên) — chỉ xem thông tin cá nhân, lớp của mình, lịch học, điểm danh, điểm số.
4. **TuVanVien** (Tư vấn viên) — xem khóa học, lớp học, thông tin tổng quan học viên (không xem điểm/học phí cá nhân), tư vấn mở lớp.

Xây dựng **Custom Authorization Attribute** `AuthorizeRoleAttribute` (kế thừa `ActionFilterAttribute`/`Attribute, IAuthorizationFilter`) nhận danh sách role cho phép:
```csharp
[AuthorizeRole("GiaoVien")]
public class GiaoVienController : Controller { }

[AuthorizeRole("QuanLy", "TuVanVien")]
public IActionResult XemThongKe() { }
```
Logic của attribute:
- Kiểm tra `Session["UserId"]` tồn tại chưa → chưa có thì redirect `Account/Login`.
- Kiểm tra `Session["VaiTro"]` có nằm trong danh sách role cho phép không → không có thì trả 403 Forbidden.

`AccountController.Login` khi đăng nhập thành công lưu Session:
```csharp
HttpContext.Session.SetString("UserId", nguoiDung.Id.ToString());
HttpContext.Session.SetString("HoTen", nguoiDung.HoTen);
HttpContext.Session.SetString("VaiTro", nguoiDung.VaiTro);
```
`Logout` gọi `HttpContext.Session.Clear()` rồi redirect về Login.

## 4. Các Model (Entity Framework Core)

**NguoiDung** (bảng tài khoản đăng nhập)
- Id (int), TaiKhoan (string, required), MatKhau (string, required), HoTen (string, required), VaiTro (string, required: "QuanLy"/"GiaoVien"/"HocVien"/"TuVanVien")

**HocVien** (Học viên)
- Id, MaHocVien, HoTen, SoDienThoai, Email, KhoaHoc, LopHoc, NgonNgu, TrangThai (default "Đang học")
- SoTienPhaiDong (decimal 18,2), SoTienDaDong (decimal 18,2), NgayDongCuoi (DateTime?)
- `[NotMapped] ConNo => Max(0, SoTienPhaiDong - SoTienDaDong)`
- `[NotMapped] TrangThaiHocPhi`: "Đã đóng" nếu đã đóng đủ, "Chưa đủ" nếu đóng một phần, "Chưa đóng" nếu chưa đóng gì.

**GiaoVien** (Giáo viên)
- Id, MaGiaoVien, HoTen, Email, SoDienThoai, NgonNgu, ChuyenMon, KinhNghiem (int, số năm)

**TuVanVien** (Tư vấn viên)
- Id, MaTuVanVien, HoTen, Email, SoDienThoai, LinhVucPhuTrach (string, vd "Anh văn giao tiếp, TOEIC"), KinhNghiem (int)

**KhoaHoc** (Khóa học)
- Id, TenKhoaHoc (required, max 200), NgonNgu (required, max 100), SoBuoi (int), Gia (decimal 18,2), MoTa

**LopHoc** (Lớp học)
- Id, MaLop, TenLop, KhoaHoc, NgonNgu, GiaoVien
- SiSoToiDa (int), SiSoHienTai (int)
- NgayHoc (string dạng "T2,T4,T6" — các thứ học trong tuần, phân tách bằng dấu phẩy)
- GioBatDau, GioKetThuc (string dạng "HH:mm")
- PhongHoc (string, ví dụ P01→P20)
- CaHoc (string hiển thị tổng hợp, vd "T2-T4, 18:00-20:00, P05")
- TrangThai (default "Sắp khai giảng", có thể là "Đang học"/"Đã kết thúc"...)

**DiemDanh** (Điểm danh)
- Id, LopHocId, MaLop, TenLop, HocVienId, MaHocVien, HoTenHocVien
- NgayHoc (DateTime), TrangThai (default "Có mặt"; giá trị: "Có mặt"|"Vắng"|"Muộn"|"Có phép")
- GhiChu (string?), GiaoVienDiemDanh (string), ThoiGianDiemDanh (DateTime, default Now)

**DiemSo** (Điểm số)
- Id, HocVienId, LopHocId, MaHocVien, HoTenHocVien, MaLop, TenLop
- DiemKT1, DiemKT2, DiemKT3, DiemKiemTraLon (decimal 4,1, nullable — điểm 3 bài kiểm tra nhỏ + 1 bài kiểm tra lớn)
- `[NotMapped] DiemTrungBinh`: trung bình có trọng số = (KT1+KT2+KT3 + KiemTraLon*2) / (số bài đã có, KiemTraLon tính hệ số 2), làm tròn 1 chữ số thập phân, null nếu chưa có điểm nào.
- `[NotMapped] KetQua`: "Chưa thi" nếu DiemKiemTraLon null; "Đạt" nếu >= 8.0; ngược lại "Không đạt".
- GhiChu (string?), GiaoVienNhap (string), NgayCapNhat (DateTime, default Now)

**ChatMessage** (Lịch sử chat với bot)
- Id, UserId (int), UserMessage (required), BotResponse (required), CreatedAt (DateTime, default UtcNow)

**ErrorViewModel** — RequestId, ShowRequestId (chuẩn MVC mặc định)

Tạo `AppDbContext : DbContext` với `DbSet` cho tất cả các entity trên (`HocViens`, `GiaoViens`, `TuVanViens`, `KhoaHocs`, `LopHocs`, `DiemDanhs`, `DiemSos`, `ChatMessages`, `NguoiDungs`), và migration EF Core tương ứng (chạy `dotnet ef database update` để tạo schema).

## 5. Controllers & Views cần tạo

### AccountController (không bảo vệ)
- `GET/POST Login` — kiểm tra TaiKhoan/MatKhau trong bảng NguoiDung, set Session, redirect theo VaiTro tương ứng vào trang Index của controller role đó.
- `Logout` — clear session, quay lại Login.

### HomeController (không bảo vệ)
- `Index`, `Privacy` — trang chủ giới thiệu trung tâm.

### QuanLyController [AuthorizeRole("QuanLy")]
Views: `Index` (dashboard tổng quan), `HocVien` (CRUD học viên + trạng thái học phí), `GiaoVien` (CRUD giáo viên), `KhoaHoc` (CRUD khóa học), `LopHoc` (CRUD lớp học, gán giáo viên/lịch/phòng), `LichHoc` (xem lịch toàn trung tâm), `HocPhi` (quản lý thu học phí, danh sách nợ), `KetQuaHocTap` (bảng điểm — **lưu ý bug đã fix**: phải LEFT JOIN từ bảng HocVien sang DiemSo để hiển thị TẤT CẢ học viên kể cả chưa có điểm, không được query trực tiếp từ DiemSo), `ThongKe` (thống kê số liệu: số học viên theo trạng thái, số lớp theo ngôn ngữ, doanh thu...).

### GiaoVienController [AuthorizeRole("GiaoVien")]
Views: `Index` (dashboard), `LopHocCuaToi` (danh sách lớp giáo viên đang dạy), `LichDay` (lịch dạy cá nhân), `DiemSoKetQua` (nhập/xem điểm học viên trong lớp mình dạy).

### HocVienController [AuthorizeRole("HocVien")]
Views: `Index` (dashboard cá nhân), `LopCuaToi` (lớp đang học), `LichHoc` (lịch học cá nhân) — hiển thị thêm điểm danh + điểm số của bản thân.

### TuVanVienController [AuthorizeRole("TuVanVien")]
Views: `Index` (dashboard), `KhoaHocMoLop` (xem khóa học và các lớp đang/sắp mở để tư vấn).

### ChatbotController [AuthorizeRole("GiaoVien", "HocVien", "TuVanVien", "QuanLy")] (API, kiểu ControllerBase)
- `POST /api/chatbot` — body `{ "message": "..." }`, response `{ "answer": "...", "vaiTro": "..." }`.

### Shared
- `_Layout.cshtml` dùng chung, có navbar đổi menu theo VaiTro trong Session, và icon chatbot (💬) nổi góc phải màn hình mở popup chat (dùng JS `wwwroot/js/site.js` + `wwwroot/js/chatbot-history.js` để load lịch sử chat qua AJAX).
- `Error.cshtml`, `_ValidationScriptsPartial.cshtml`.

## 6. Logic Chatbot AI (phần quan trọng nhất — chi tiết đầy đủ)

Tạo 3 service:
- **GeminiService**: gọi Gemini API (`gemini-2.0-flash`) bằng HttpClient, method `AskAsync(string prompt) -> Task<string>`. Đọc API key từ `appsettings.json` mục `Gemini:ApiKey` (hoặc biến môi trường `GEMINI_API_KEY`) và `Gemini:Model`.
- **ChatbotDataService**: chứa các hàm lấy dữ liệu theo vai trò (`GetQuanLyDataAsync`, `GetGiaoVienDataAsync`, `GetHocVienDataAsync`, `GetTuVanVienDataAsync`) và các **Direct Handler** (trả lời trực tiếp từ DB, không gọi AI, để nhanh <200ms) — mô tả chi tiết bên dưới.
- **AILoggingService**: log lại tương tác/chi phí gọi Gemini (tùy chọn, dùng cho debug & audit).

### Luồng xử lý trong `ChatbotController.Chat`:
```
1. Lấy VaiTro và UserId từ Session. Nếu thiếu → trả 401 Unauthorized.
2. Chạy tuần tự các Direct Handler theo thứ tự dưới đây (mỗi handler trả về string hoặc null):
   - Nếu 1 handler trả về non-null → lưu vào bảng ChatMessage, trả JSON { answer, vaiTro }, DỪNG (không gọi Gemini).
3. Nếu KHÔNG có handler nào khớp → dùng Gemini:
   a. Lấy dữ liệu được phép xem theo vai trò (Get<VaiTro>DataAsync).
   b. Ghép prompt gồm: hướng dẫn theo vai trò + dữ liệu (JSON) + 15 quy tắc định dạng đầu ra bắt buộc + ngữ cảnh trang hiện tại + policy vai trò.
   c. Gọi GeminiService.AskAsync(prompt).
   d. Lưu ChatMessage, trả JSON { answer, vaiTro }.
```

### 5 Direct Handlers cần cài đặt trong ChatbotDataService (thứ tự kiểm tra như dưới):

**1. TryHandleFinanceSummaryAsync** (chỉ QuanLy)
- Từ khóa: "chưa đóng", "nợ", "học phí"
- Query: `HocVien` where `ConNo > 0`, order theo HoTen
- Trả về dạng: `"Hiện có X học viên chưa đóng học phí trên tổng Y học viên. Danh sách: Tên (MãHV) - còn nợ Z đ; ..."`

**2. TryHandleScheduleClassQueryAsync** (QuanLy, TuVanVien)
- Từ khóa: "thứ"/"thu", "ngày", "học vào", "lịch học" kết hợp tên ngôn ngữ ("tiếng Anh", "tiếng Trung"...)
- Parse mã thứ trong câu hỏi: số "246" hoặc "thứ 2,4,6" → map thành ["T2","T4","T6"] (1/chủ nhật→CN, 2→T2,... 7→T7)
- Query `LopHoc` where NgonNgu chứa từ khóa ngôn ngữ, TrangThai != "Đã kết thúc", NgayHoc giao với các thứ mục tiêu
- Trả về: `"Có X lớp [ngôn ngữ] học [các thứ]. Danh sách: TênLớp - NgayHoc - GioBatDau-GioKetThuc - PhòngHọc - GV: Tên"`

**3. TryHandleStudentCountQueryAsync** (chỉ QuanLy)
- Từ khóa: "bao nhiêu học viên", "có bao nhiêu", "tổng số học viên"
- Query: COUNT tổng + GROUP BY TrangThai
- Trả về: `"Tổng cộng X học viên. Chi tiết: Hoạt động: Y; Tạm dừng: Z; ..."`

**4. TryHandlePassingStudentsQueryAsync** (chỉ QuanLy)
- Từ khóa đạt: "học viên nào đạt", "ai đạt"; từ khóa không đạt: "học viên nào không đạt", "ai không đạt", "không đạt"
- Tiêu chí: DiemKiemTraLon >= 8 → Đạt; < 8 → Không đạt
- Query bảng DiemSo where DiemKiemTraLon != null
- Trả về: `"Có X học viên [đạt/không đạt]. Danh sách: Tên - điểm; ..."`

**5. TryHandleClassCountQueryAsync** (QuanLy, TuVanVien)
- Từ khóa: "bao nhiêu lớp", "có bao nhiêu lớp" — nhưng KHÔNG kích hoạt nếu câu có chứa "thứ"/"thu" (tránh nhầm với handler lịch học)
- Query: COUNT LopHoc where TrangThai != "Đã kết thúc" + GROUP BY NgonNgu
- Trả về: `"Hiện có X lớp đang hoạt động. Chi tiết: Tiếng Anh: Y; Tiếng Trung: Z; ..."`

### 15 quy tắc bắt buộc chèn vào prompt Gemini (roleInstructions):
1. Không bao giờ sinh ra dữ liệu không tồn tại.
2. Khi hỏi "ai"/"những ai", phải liệt kê tên cụ thể.
3. Khi hỏi "bao nhiêu", phải trả lời bằng số cụ thể.
4. Không được sáng tác tên học viên/giáo viên/lớp.
5. Phải nói "không có dữ liệu" nếu thực sự không có.
6. Mỗi tên học viên phải kèm mã/chỉ số.
7. Khi so sánh, phải dùng số liệu từ dữ liệu được cung cấp.
8. Không được truy cập dữ liệu ngoài phạm vi vai trò.
9. Phải kết thúc câu trả lời bằng dấu chấm.
10. Số tiền phải có đơn vị "đ" hoặc "VND".
11. Ngày tháng in đầy đủ ngày/tháng/năm.
12. Giờ học ở dạng HH:mm (24h).
13. Lưu ý kỳ hạn và trạng thái khi trả lời.
14. Phải nêu rõ "hiện tại" là kỳ hạn/khóa nào.
15. Không được tư vấn vượt quá dữ liệu được cung cấp.

### Dữ liệu được phép theo từng vai trò (Get<VaiTro>DataAsync):
- **QuanLy**: toàn bộ dữ liệu (tất cả học viên, giáo viên, lớp, điểm, học phí).
- **GiaoVien**: chỉ lớp mình dạy + học viên/điểm trong các lớp đó.
- **HocVien**: chỉ thông tin cá nhân (lớp của mình, điểm số, điểm danh của mình).
- **TuVanVien**: chỉ khóa học + lớp học (KHÔNG bao gồm điểm số cá nhân, KHÔNG bao gồm học phí cá nhân của học viên).

### Ví dụ test case cần đảm bảo hoạt động đúng:
- QuanLy hỏi "Bao nhiêu học viên?" → trả lời tổng số + breakdown trạng thái (direct handler).
- QuanLy hỏi "Ai chưa đóng học phí?" → danh sách kèm số tiền nợ (direct handler).
- GiaoVien hỏi "Tôi dạy những lớp nào?" → Gemini trả lời dựa trên dữ liệu lọc theo giáo viên đó.
- GiaoVien hỏi về lớp không phải của mình → phải bị chặn ("Bạn chỉ có quyền xem dữ liệu lớp do mình phụ trách.").
- HocVien hỏi "Điểm của tôi bao nhiêu?" → Gemini trả lời từ dữ liệu cá nhân.
- HocVien hỏi "Tạo bài luyện tập tiếng Anh cho tôi" → Gemini sinh bài tập phù hợp ngôn ngữ khóa học của học viên.
- TuVanVien hỏi "Điểm số của học viên X bao nhiêu?" → phải bị chặn ("Tôi không có quyền xem điểm số riêng của học viên.").
- TuVanVien hỏi "Học phí học viên X là bao nhiêu?" → phải bị chặn tương tự.

## 7. Cấu hình (appsettings.json)
```json
{
  "Logging": {
    "LogLevel": { "Default": "Information", "Microsoft.EntityFrameworkCore.Database.Command": "Debug" }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TrungtamaiDb;Integrated Security=true;Encrypt=false;TrustServerCertificate=true;"
  },
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE",
    "Model": "gemini-2.0-flash"
  },
  "AllowedHosts": "*"
}
```
Trong `Program.cs`: cấu hình DbContext (SQL Server), Session (`options.IdleTimeout = TimeSpan.FromHours(8)`, HttpOnly cookie), đăng ký HttpClient cho GeminiService, đăng ký ChatbotDataService/AILoggingService, MVC routing mặc định `{controller=Home}/{action=Index}/{id?}`, dùng `app.UseSession()` trước `app.UseAuthorization()`/MapControllerRoute.

## 8. Yêu cầu bảo mật
- Session-based auth (HttpOnly cookie), timeout 8 giờ.
- Kiểm soát quyền chi tiết theo role bằng custom attribute (không dùng if-else rải rác trong action).
- Logout phải clear toàn bộ session.
- Chatbot không được rò rỉ dữ liệu ngoài phạm vi vai trò (kiểm tra kỹ ở cả tầng Direct Handler lẫn tầng data-payload gửi cho Gemini).

## 9. Tài liệu cần sinh kèm (docs/)
- `README.md`: giới thiệu, hướng dẫn cài đặt, cấu hình, chạy, vai trò & quyền hạn, API chatbot mẫu, troubleshooting, hướng dẫn deploy Azure/Docker.
- `AUTHORIZATION_SYSTEM.md`: giải thích cơ chế AuthorizeRoleAttribute, session, cách mở rộng vai trò mới.
- `CHATBOT_HANDLER_FLOW.md`: sơ đồ luồng xử lý chatbot, chi tiết từng Direct Handler, 15 rule Gemini, bảng troubleshooting.
- `CHATBOT_TEST_SCENARIOS.md`: các kịch bản test theo từng vai trò.
- `IMPLEMENTATION_READY.md`: checklist hoàn thành, bảng thống kê handler, hướng dẫn thêm handler mới theo template chuẩn.

## 10. Ghi chú triển khai quan trọng
- Bug đã biết cần tránh: trang Kết Quả Học Tập (QuanLy/KetQuaHocTap) phải lấy danh sách xuất phát từ bảng `HocVien` rồi LEFT JOIN sang `DiemSo`, KHÔNG được query trực tiếp từ `DiemSo` (nếu không học viên chưa có điểm sẽ bị mất khỏi danh sách).
- Toàn bộ giao diện, nhãn, thông báo, và câu trả lời chatbot đều bằng **tiếng Việt**.
- Ưu tiên Direct Handler trước khi gọi Gemini để tiết kiệm chi phí & tăng tốc độ phản hồi (mục tiêu giảm ~60-70% lượt gọi Gemini).

---

Hãy triển khai đầy đủ code cho tất cả các phần trên: Models, DbContext + Migrations, Attributes, Controllers, Views (Razor + Bootstrap), Services (Gemini, ChatbotData, AILogging), Program.cs, appsettings, và tài liệu docs như mô tả.
