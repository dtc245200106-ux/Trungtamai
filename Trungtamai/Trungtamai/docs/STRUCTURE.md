# Cấu Trúc Dự Án

Dự án này tuân theo mô hình kiến trúc phân lớp với sự tách biệt rõ ràng giữa các lớp:

## Tổ Chức Thư Mục

```
Trungtamai/
├── backend/                    # Backend API và logic nghiệp vụ
│   ├── Controllers/            # Các controller xử lý yêu cầu API
│   ├── Services/               # Lớp xử lý logic nghiệp vụ
│   ├── Models/                 # Các model dữ liệu và DTOs
│   ├── Properties/             # Thuộc tính dự án và launchSettings
│   ├── Program.cs              # Điểm vào ứng dụng
│   └── Trungtamai.csproj       # File cấu hình dự án
│
├── frontend/                   # Giao diện người dùng
│   ├── Views/                  # Razor views và template HTML
│   └── wwwroot/                # Static assets (CSS, JS, hình ảnh)
│
├── database/                   # Truy cập dữ liệu và lưu trữ
│   ├── Data/                   # EF Core DbContext và models
│   └── Migrations/             # File migration cơ sở dữ liệu
│
├── config/                     # File cấu hình
│   ├── appsettings.json        # Cấu hình cho môi trường production
│   └── appsettings.Development.json  # Cấu hình cho môi trường phát triển
│
├── docs/                       # Tài liệu
│   ├── README.md               # Tổng quan dự án
│   ├── CHATBOT_HANDLER_FLOW.md # Tài liệu luồng xử lý chatbot
│   ├── CHATBOT_TEST_SCENARIOS.md # Các kịch bản kiểm thử
│   └── IMPLEMENTATION_READY.md # Ghi chú triển khai
│
├── bin/                        # File nhị phân đã biên dịch (tự động tạo)
└── obj/                        # Build objects (tự động tạo)
```

## Mô Tả Chi Tiết Các Lớp

### Lớp Backend

- **Controllers/**: Các controller ASP.NET Core MVC xử lý yêu cầu HTTP
- **Services/**: Lớp xử lý logic nghiệp vụ với các dịch vụ ứng dụng
- **Models/**: Các model miền (domain models) và DTOs để truyền dữ liệu
- **Properties/**: Siêu dữ liệu dự án và cấu hình khởi động

### Lớp Frontend

- **Views/**: Razor views để render phía máy chủ
- **wwwroot/**: Các tài sản tĩnh được phục vụ trực tiếp cho client

### Lớp Database

- **Data/**: Entity Framework Core context và database models
- **Migrations/**: File migration EF Core để thay đổi schema cơ sở dữ liệu

### Lớp Cấu Hình

- **config/**: Các cài đặt ứng dụng cho các môi trường khác nhau

### Lớp Tài Liệu

- **docs/**: Tài liệu và thông số kỹ thuật dự án

## Xây Dựng và Chạy Ứng Dụng

1. Điều hướng đến thư mục `backend`
2. Chạy: `dotnet build` (từ thư mục backend hoặc cập nhật đường dẫn trong csproj)
3. Chạy: `dotnet run`

Cấu hình được tải từ thư mục `config` khi khởi động ứng dụng.

## Ghi Chú Phát Triển

- Hãy chắc chắn cập nhật các build scripts để tính đến vị trí mới của thư mục `backend/`
- Database migrations nên được chạy từ thư mục backend
- Frontend assets được phục vụ từ `frontend/wwwroot/` thông qua backend
