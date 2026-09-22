# Sơ đồ UML hệ thống Trungtamai

Domain Model: [DOMAIN_MODEL.md](DOMAIN_MODEL.md)

Tài liệu này mô tả các luồng chính đang được triển khai trong ứng dụng ASP.NET Core MVC:

- Người dùng đăng nhập và được điều hướng theo vai trò.
- Middleware phân quyền kiểm tra Session trước khi cho phép truy cập controller.
- Quản lý thêm học viên, tạo tài khoản đăng nhập và cập nhật sĩ số lớp.

## 1. Sơ đồ hoạt động tổng quát

Ảnh sơ đồ hoạt động: [so-do-hoat-dong-trungtamai-gon.svg](so-do-hoat-dong-trungtamai-gon.svg)

```mermaid
flowchart TD
    A([Bắt đầu]) --> B[Người dùng mở hệ thống]
    B --> C{Đã có Session UserId?}
    C -- Chưa --> D[Hiển thị trang Login]
    D --> E[Nhập tài khoản và mật khẩu]
    E --> F{Đủ thông tin?}
    F -- Không --> G[Hiển thị lỗi nhập liệu]
    G --> E
    F -- Có --> H[AccountController tìm NguoiDung trong CSDL]
    H --> I{Tài khoản hợp lệ?}
    I -- Không --> J[Hiển thị lỗi đăng nhập]
    J --> E
    I -- Có --> K[Lưu UserId, HoTen, VaiTro vào Session]
    K --> L{Vai trò là gì?}
    L -- QuanLy --> M[Trang quản lý]
    L -- GiaoVien --> N[Trang giáo viên]
    L -- HocVien --> O[Trang học viên]
    L -- TuVanVien --> P[Trang tư vấn viên]
    L -- Khác --> Q[Hiển thị lỗi vai trò]
    C -- Có --> R[Người dùng gọi chức năng]
    R --> S[AuthorizeRoleAttribute đọc Session]
    S --> T{Vai trò được phép?}
    T -- Không đăng nhập --> D
    T -- Sai vai trò --> U([HTTP 403 Forbidden])
    T -- Được phép --> V[Controller xử lý nghiệp vụ]
    V --> W[AppDbContext truy vấn hoặc cập nhật CSDL]
    W --> X[Trả View hoặc JSON response]
    X --> Y{Đăng xuất?}
    Y -- Có --> Z[Xóa lịch sử chatbot của user và Clear Session]
    Z --> D
    Y -- Không --> R
```

## 2. Sơ đồ trình tự: đăng nhập và phân quyền

```mermaid
sequenceDiagram
    actor ND as Người dùng
    participant UI as Giao diện Login
    participant AC as AccountController
    participant DB as AppDbContext / CSDL
    participant S as Session
    participant AR as AuthorizeRoleAttribute
    participant C as Controller theo vai trò

    ND->>UI: Nhập TaiKhoan và MatKhau
    UI->>AC: POST /Account/Login
    AC->>AC: Kiểm tra dữ liệu bắt buộc
    alt Thiếu tài khoản hoặc mật khẩu
        AC-->>UI: View Login + thông báo lỗi
    else Đủ dữ liệu
        AC->>DB: Tìm NguoiDung theo tài khoản và mật khẩu
        alt Không tìm thấy tài khoản
            DB-->>AC: null
            AC-->>UI: View Login + thông báo sai thông tin
        else Tìm thấy tài khoản
            DB-->>AC: NguoiDung(Id, HoTen, VaiTro)
            AC->>S: Set UserId, HoTen, VaiTro
            AC-->>ND: Redirect đến Index của vai trò
            ND->>C: GET chức năng cần dùng
            C->>AR: Thực hiện kiểm tra authorization
            AR->>S: Đọc UserId và VaiTro
            alt Chưa đăng nhập
                AR-->>ND: Redirect /Account/Login
            else Sai vai trò
                AR-->>ND: HTTP 403 Forbidden
            else Đúng vai trò
                AR-->>C: Cho phép xử lý
                C-->>ND: View hoặc dữ liệu JSON
            end
        end
    end
```

## 3. Sơ đồ trình tự: quản lý thêm học viên

```mermaid
sequenceDiagram
    actor QL as Quản lý
    participant UI as Giao diện quản lý học viên
    participant AR as AuthorizeRoleAttribute
    participant QC as QuanLyController
    participant DB as AppDbContext / CSDL
    participant S as Session

    QL->>UI: Nhập thông tin học viên
    UI->>QC: POST /QuanLy/ThemHocVien
    QC->>AR: Kiểm tra vai trò QuanLy
    AR->>S: Đọc VaiTro và UserId
    alt Chưa đăng nhập hoặc sai vai trò
        AR-->>QL: Redirect Login hoặc HTTP 403
    else Được phép
        AR-->>QC: Cho phép xử lý
        QC->>QC: Kiểm tra họ tên, số điện thoại, email
        alt Thiếu trường bắt buộc
            QC-->>UI: Redirect HocVien + TempData Error
        else Dữ liệu cơ bản hợp lệ
            QC->>DB: Tìm lớp theo tên
            alt Không tìm thấy lớp
                DB-->>QC: null
                QC-->>UI: Redirect + lỗi lớp học
            else Có lớp
                QC->>DB: Đếm học viên đang học trong lớp
                alt Lớp đã đủ sĩ số
                    DB-->>QC: Số lượng >= SiSoToiDa
                    QC-->>UI: Redirect + lỗi đầy sĩ số
                else Còn chỗ
                    QC->>DB: Kiểm tra email đã là tài khoản chưa
                    alt Email đã tồn tại
                        DB-->>QC: true
                        QC-->>UI: Redirect + lỗi trùng email
                    else Email chưa tồn tại
                        QC->>DB: Lấy học phí khóa học và sinh mã HV
                        QC->>QC: Tạo HocVien và NguoiDung
                        QC->>DB: Add HocVien + Add NguoiDung
                        QC->>DB: SaveChangesAsync()
                        QC->>DB: CapNhatSiSoLopAsync()
                        QC-->>UI: Redirect + TempData Success
                        UI-->>QL: Hiển thị kết quả và thông tin tài khoản
                    end
                end
            end
        end
    end
```

## 4. Quy ước ký hiệu

| Ký hiệu                    | Ý nghĩa                                     |
| -------------------------- | ------------------------------------------- |
| Hình tròn bắt đầu/kết thúc | Điểm bắt đầu hoặc kết thúc luồng hoạt động  |
| Hình thoi                  | Điều kiện rẽ nhánh                          |
| Mũi tên                    | Thứ tự chuyển trạng thái hoặc thông điệp    |
| `actor`                    | Người dùng hoặc tác nhân bên ngoài hệ thống |
| `participant`              | Thành phần phần mềm tham gia trao đổi       |
| `alt`                      | Nhánh điều kiện trong sơ đồ trình tự        |
