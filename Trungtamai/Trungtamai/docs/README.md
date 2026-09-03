# TrungTamAi - Hệ thống Quản lý Trung tâm Ngoại ngữ

Ứng dụng web quản lý trung tâm ngoại ngữ LinguistAI với tích hợp chatbot AI (Gemini) hỗ trợ theo vai trò người dùng.

## 📋 Mục lục

- [Giới thiệu](#giới-thiệu)
- [Công nghệ](#công-nghệ)
- [Yêu cầu hệ thống](#yêu-cầu-hệ-thống)
- [Cài đặt](#cài-đặt)
- [Cấu hình](#cấu-hình)
- [Chạy ứng dụng](#chạy-ứng-dụng)
- [Cấu trúc Project](#cấu-trúc-project)
- [Vai trò & Quyền hạn](#vai-trò--quyền-hạn)
- [Tính năng Chatbot](#tính-năng-chatbot)
- [Troubleshooting](#troubleshooting)

## 📖 Giới thiệu

**Trungtamai** là hệ thống quản lý toàn diện cho trung tâm ngoại ngữ, hỗ trợ:

- 👥 **Quản lý học viên, giáo viên, khóa học, lớp học**
- 📊 **Thống kê, báo cáo, quản lý học phí**
- 💬 **Chatbot AI thông minh** - trả lời câu hỏi tự động theo vai trò & quyền hạn
- 🔐 **Hệ thống phân quyền** - 4 vai trò khác nhau (Quản lý, Giáo viên, Học viên, Tư vấn viên)
- 📱 **Giao diện web responsive** - sử dụng trên desktop, tablet, mobile

## 🛠 Công nghệ

| Công nghệ | Phiên bản | Mục đích |
|-----------|----------|---------|
| **.NET** | 10.0 | Framework chính |
| **ASP.NET Core** | 10.0 | Web framework |
| **Entity Framework Core** | 10.0.5 | ORM & Database |
| **SQL Server** | Tùy | Cơ sở dữ liệu |
| **Gemini API** | 1.19.0 | Chatbot AI |
| **Razor Pages / MVC** | 10.0 | Giao diện web |
| **Session Management** | Distributed Memory | Xác thực người dùng |

## ⚙️ Yêu cầu hệ thống

### Bắt buộc
- **OS**: Windows, Linux, hoặc macOS
- **.NET SDK**: 10.0 hoặc cao hơn
- **SQL Server**: 2019, 2022, hoặc LocalDB
- **Trình duyệt**: Chrome, Firefox, Edge (hỗ trợ ES6+)

### Tùy chọn
- **Visual Studio 2022** hoặc **VS Code** (để phát triển)
- **Git** (quản lý code)

### API Keys cần thiết
- **Google Gemini API Key** - để sử dụng chatbot

## 📥 Cài đặt

### 1. Clone Repository
```bash
git clone <repository-url>
cd Trungtamai/Trungtamai/Trungtamai
```

### 2. Cài đặt Dependencies
```bash
dotnet restore
```

### 3. Cấu hình Database

#### Option A: SQL Server (khuyến nghị cho Production)
Cập nhật connection string trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=TrungtamaiDb;User Id=sa;Password=YourPassword;Encrypt=false;TrustServerCertificate=true;"
  }
}
```

#### Option B: LocalDB (cho Development)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TrungtamaiDb;Integrated Security=true;Encrypt=false;TrustServerCertificate=true;"
  }
}
```

### 4. Chạy Migration (tạo database schema)
```bash
dotnet ef database update
```

## 🔧 Cấu hình

### appsettings.json - Cấu hình chính

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Debug"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=TrungtamaiDb;..."
  },
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE",
    "Model": "gemini-2.0-flash"
  },
  "AllowedHosts": "*"
}
```

### appsettings.Development.json - Cấu hình Development

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "Gemini": {
    "ApiKey": "YOUR_DEV_API_KEY"
  }
}
```

### Lấy Gemini API Key

1. Truy cập: https://aistudio.google.com/apikey
2. Tạo API key mới
3. Copy key và paste vào `appsettings.json` hoặc environment variable: `GEMINI_API_KEY`

## 🚀 Chạy ứng dụng

### Development Mode
```bash
dotnet run
```
Ứng dụng sẽ khởi động tại: `https://localhost:5001` hoặc `http://localhost:5000`

### Production Mode
```bash
dotnet publish -c Release -o publish
cd publish
dotnet Trungtamai.dll
```

### Chạy qua VS Code
1. Mở project trong VS Code
2. Nhấn `F5` hoặc chạy từ **Run** menu
3. Trình duyệt sẽ mở tự động

## 📁 Cấu trúc Project

```
Trungtamai/
├── Controllers/              # Xử lý yêu cầu HTTP
│   ├── AccountController.cs  # Đăng nhập / Đăng xuất
│   ├── ChatbotController.cs  # Chatbot API
│   ├── QuanLyController.cs   # Trang quản lý
│   ├── GiaoVienController.cs # Trang giáo viên
│   ├── HocVienController.cs  # Trang học viên
│   └── TuVanVienController.cs # Trang tư vấn viên
│
├── Models/                   # Các model / Entity
│   ├── HocVien.cs           # Học viên
│   ├── GiaoVien.cs          # Giáo viên
│   ├── LopHoc.cs            # Lớp học
│   ├── KhoaHoc.cs           # Khóa học
│   ├── DiemSo.cs            # Điểm số
│   ├── DiemDanh.cs          # Điểm danh
│   ├── ChatMessage.cs       # Lịch sử chat
│   └── ...
│
├── Services/                 # Business logic
│   ├── GeminiService.cs      # Gọi Gemini API
│   ├── ChatbotDataService.cs # Xử lý dữ liệu chatbot
│   └── ...
│
├── Data/                     # Database
│   ├── AppDbContext.cs       # Entity Framework DbContext
│   └── Migrations/           # Database migrations
│
├── Views/                    # Razor Pages / MVC Views
│   ├── Account/
│   ├── QuanLy/
│   ├── GiaoVien/
│   ├── HocVien/
│   ├── TuVanVien/
│   └── Shared/
│
├── wwwroot/                  # Static files (CSS, JS, Images)
│   ├── css/
│   ├── js/
│   └── images/
│
├── Program.cs                # Application entry point
├── appsettings.json          # Cấu hình ứng dụng
├── Trungtamai.csproj         # Project file
│
└── Documentation/
    ├── CHATBOT_TEST_SCENARIOS.md  # Test scenarios
    ├── CHATBOT_HANDLER_FLOW.md    # Technical flow
    └── IMPLEMENTATION_READY.md    # Implementation notes
```

## 👥 Vai trò & Quyền hạn

### 1. **Quản lý (QuanLy)**
- ✅ Xem tổng quan trung tâm
- ✅ Quản lý học viên, giáo viên
- ✅ Quản lý khóa học, lớp học
- ✅ Xem lịch học, lịch dạy
- ✅ Quản lý học phí
- ✅ Xem kết quả học tập, điểm số
- ✅ Thống kê
- 💬 Chatbot: Trả lời câu hỏi về số liệu toàn trung tâm

### 2. **Giáo viên (GiaoVien)**
- ✅ Xem lớp do mình dạy
- ✅ Xem lịch dạy
- ✅ Xem học viên trong lớp
- ✅ Xem / Nhập điểm số lớp mình
- ✅ Xem kết quả học tập
- 💬 Chatbot: Trả lời câu hỏi về các lớp do mình dạy

### 3. **Học viên (HocVien)**
- ✅ Xem thông tin cá nhân
- ✅ Xem lớp học của mình
- ✅ Xem lịch học
- ✅ Xem điểm danh
- ✅ Xem điểm số của mình
- 💬 Chatbot: Trả lời câu hỏi cá nhân + tạo bài luyện tập

### 4. **Tư vấn viên (TuVanVien)**
- ✅ Xem khóa học
- ✅ Xem lớp học
- ✅ Xem thông tin học viên (tổng quan)
- ✅ Tư vấn chương trình
- 💬 Chatbot: Tư vấn về khóa học, lớp học phù hợp

## 💬 Tính năng Chatbot

### Giới thiệu

Chatbot được xây dựng bằng **Gemini AI** với hỗ trợ **role-based** - mỗi vai trò chỉ được hỏi về dữ liệu phù hợp.

### Query Types được hỗ trợ

#### 1️⃣ **Direct Handlers** (Không cần AI - nhanh < 200ms)
- ✅ "Bao nhiêu học viên?" → Trả về số lượng theo trạng thái
- ✅ "Ai chưa đóng học phí?" → Danh sách học viên nợ
- ✅ "Ai đạt kiểm tra?" → Danh sách học viên đạt (điểm >= 8)
- ✅ "Lớp tiếng Anh thứ 246?" → Danh sách lớp với lịch chi tiết
- ✅ "Bao nhiêu lớp?" → Số lớp theo ngôn ngữ

#### 2️⃣ **AI-Generated Responses** (Sử dụng Gemini - 2-5 giây)
- ✅ "Tổng quan trung tâm"
- ✅ "Tôi dạy những lớp nào?"
- ✅ "Tôi học lớp nào?"
- ✅ "Điểm của tôi là bao nhiêu?"
- ✅ "Tạo bài luyện tập tiếng Anh cho tôi"
- ✅ "Lớp nào phù hợp cho tôi?"

### Cách sử dụng Chatbot

#### Từ Frontend
```
1. Đăng nhập vào hệ thống
2. Tìm icon chatbot (💬) ở góc phải màn hình
3. Nhập câu hỏi bằng tiếng Việt
4. Nhấn Enter hoặc nút gửi
5. Chatbot sẽ trả lời dựa trên vai trò của bạn
```

#### Từ API (Postman / cURL)
```bash
POST /api/chatbot HTTP/1.1
Host: localhost:5001
Content-Type: application/json

{
  "message": "Bao nhiêu học viên?"
}
```

Response:
```json
{
  "answer": "Tổng cộng 150 học viên. Chi tiết: Hoạt động: 120; Tạm dừng: 25; Chưa bắt đầu: 5.",
  "vaiTro": "QuanLy"
}
```

### Prompting Guidelines

Chatbot sử dụng **15+ prompt rules** để đảm bảo:
1. ✅ Không sinh ra dữ liệu không tồn tại
2. ✅ Tuân thủ quyền hạn của từng vai trò
3. ✅ Trả về số liệu thực từ database
4. ✅ Liệt kê tên thực khi hỏi "ai"
5. ✅ Không sáng tác dữ liệu

Xem chi tiết tại: [CHATBOT_HANDLER_FLOW.md](CHATBOT_HANDLER_FLOW.md)

## 🧪 Testing

### Chạy Unit Tests (nếu có)
```bash
dotnet test
```

### Test Chatbot qua Postman
1. Mở Postman
2. Tạo POST request đến: `http://localhost:5001/api/chatbot`
3. Body (JSON):
```json
{
  "message": "Bao nhiêu học viên?"
}
```
4. Nhấn Send

### Test Scenarios
Xem tài liệu chi tiết: [CHATBOT_TEST_SCENARIOS.md](CHATBOT_TEST_SCENARIOS.md)

## 🐛 Troubleshooting

### 1. **Database Connection Error**
```
Error: A network-related or instance-specific error occurred while establishing a connection to SQL Server.
```
**Giải pháp:**
- Kiểm tra connection string trong `appsettings.json`
- Đảm bảo SQL Server đang chạy
- Kiểm tra tường lửa cho cổng 1433 (SQL Server)

### 2. **Gemini API Key Invalid**
```
Error: Invalid API key provided.
```
**Giải pháp:**
- Kiểm tra API key trong `appsettings.json`
- Lấy key mới từ: https://aistudio.google.com/apikey
- Đảm bảo API key không chứa khoảng trắng

### 3. **Chatbot không trả lời**
**Giải pháp:**
- Kiểm tra cơ sở dữ liệu có dữ liệu hay không
- Kiểm tra Gemini API quota (không vượt quá)
- Xem log (Development mode) để debug

### 4. **Session hết hạn - Cần đăng nhập lại**
**Giải pháp:**
- Mặc định session timeout là 8 giờ
- Có thể cấu hình trong `Program.cs`:
```csharp
options.IdleTimeout = TimeSpan.FromHours(8);
```

### 5. **Trang không tải CSS/JS**
**Giải pháp:**
- Kiểm tra folder `wwwroot` có tồn tại không
- Chạy `dotnet clean` rồi `dotnet build` lại
- Clear browser cache (Ctrl+Shift+Delete)

## 📝 Logging

### Bật Debug Logging
Sửa `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### Xem Logs
- Development: Console output
- Production: Kiểm tra file log (nếu được cấu hình)

## 🚀 Deployment

### Deploy lên Azure
```bash
# 1. Publish
dotnet publish -c Release -o publish

# 2. Zip file
Compress-Archive -Path publish -DestinationPath app.zip

# 3. Deploy (dùng Azure CLI hoặc Portal)
az webapp deployment source config-zip --name <app-name> --resource-group <rg-name> --src app.zip
```

### Deploy lên Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet build "Trungtamai.csproj"
RUN dotnet publish "Trungtamai.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 80 443
ENTRYPOINT ["dotnet", "Trungtamai.dll"]
```

## 📞 Hỗ trợ & Tài liệu

- **Project Issues**: GitHub Issues
- **Documentation**: `/Documents` folder
- **Chatbot Flow**: [CHATBOT_HANDLER_FLOW.md](CHATBOT_HANDLER_FLOW.md)
- **Test Cases**: [CHATBOT_TEST_SCENARIOS.md](CHATBOT_TEST_SCENARIOS.md)
- **Implementation**: [IMPLEMENTATION_READY.md](IMPLEMENTATION_READY.md)

## 📄 License

MIT License - Xem `LICENSE` file để chi tiết

## 🙋 Đóng góp

Pull requests được chào đón. Để các thay đổi lớn, vui lòng tạo issue trước.

---

**Phiên bản**: 1.0  
**Cập nhật lần cuối**: 2026-09-01  
**Trạng thái**: ✅ Production Ready
