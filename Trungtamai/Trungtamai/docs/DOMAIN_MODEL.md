# Domain Model hệ thống Trungtamai

Ảnh sơ đồ: [domain-model-trungtamai.svg](domain-model-trungtamai.svg)

Sơ đồ dưới đây mô tả các thực thể nghiệp vụ chính của hệ thống và các liên kết logic đang thể hiện trong mã nguồn. Một số liên kết như `HocVien - LopHoc` và `LopHoc - GiaoVien` hiện được lưu bằng mã/tên chuỗi, chưa phải navigation property hoặc foreign key bắt buộc của Entity Framework.

```mermaid
classDiagram
    class NguoiDung {
        +int Id
        +string TaiKhoan
        +string MatKhau
        +string HoTen
        +string VaiTro
    }

    class HocVien {
        +int Id
        +string MaHocVien
        +string HoTen
        +string SoDienThoai
        +string Email
        +string KhoaHoc
        +string LopHoc
        +string NgonNgu
        +string TrangThai
        +decimal SoTienPhaiDong
        +decimal SoTienDaDong
        +DateTime NgayDongCuoi
    }

    class GiaoVien {
        +int Id
        +string MaGiaoVien
        +string HoTen
        +string Email
        +string SoDienThoai
        +string NgonNgu
        +string ChuyenMon
        +int KinhNghiem
    }

    class TuVanVien {
        +int Id
        +string MaTuVanVien
        +string HoTen
        +string Email
        +string SoDienThoai
        +string LinhVucPhuTrach
        +int KinhNghiem
    }

    class KhoaHoc {
        +int Id
        +string TenKhoaHoc
        +string NgonNgu
        +int SoBuoi
        +decimal Gia
        +string MoTa
    }

    class LopHoc {
        +int Id
        +string MaLop
        +string TenLop
        +string KhoaHoc
        +string NgonNgu
        +string GiaoVien
        +int SiSoToiDa
        +int SiSoHienTai
        +string NgayHoc
        +string GioBatDau
        +string GioKetThuc
        +string PhongHoc
        +string CaHoc
        +string TrangThai
    }

    class DiemDanh {
        +int Id
        +int LopHocId
        +int HocVienId
        +DateTime NgayHoc
        +string TrangThai
        +string GhiChu
        +string GiaoVienDiemDanh
        +DateTime ThoiGianDiemDanh
    }

    class DiemSo {
        +int Id
        +int HocVienId
        +int LopHocId
        +decimal DiemKT1
        +decimal DiemKT2
        +decimal DiemKT3
        +decimal DiemKiemTraLon
        +string GhiChu
        +string GiaoVienNhap
        +DateTime NgayCapNhat
    }

    class ChatMessage {
        +int Id
        +int UserId
        +string UserMessage
        +string BotResponse
        +DateTime CreatedAt
    }

    NguoiDung "1" .. "0..1" HocVien : tai khoan
    NguoiDung "1" .. "0..*" ChatMessage : lich su chat
    KhoaHoc "1" .. "0..*" LopHoc : gom cac lop
    KhoaHoc "1" .. "0..*" HocVien : dang ky
    GiaoVien "1" .. "0..*" LopHoc : giang day
    LopHoc "1" .. "0..*" HocVien : co hoc vien
    HocVien "1" --> "0..*" DiemDanh : duoc diem danh
    LopHoc "1" --> "0..*" DiemDanh : co buoi diem danh
    HocVien "1" --> "0..*" DiemSo : co ket qua
    LopHoc "1" --> "0..*" DiemSo : co bang diem
```

## Ý nghĩa nghiệp vụ

| Thực thể      | Vai trò                                                                        |
| ------------- | ------------------------------------------------------------------------------ |
| `NguoiDung`   | Xác thực tài khoản và phân quyền `QuanLy`, `GiaoVien`, `HocVien`, `TuVanVien`. |
| `KhoaHoc`     | Mô tả khóa học, ngôn ngữ, số buổi và học phí.                                  |
| `LopHoc`      | Tổ chức lớp, giáo viên, lịch học, phòng học và sĩ số.                          |
| `HocVien`     | Lưu thông tin học viên, trạng thái học và tình trạng học phí.                  |
| `GiaoVien`    | Lưu thông tin giảng viên và chuyên môn.                                        |
| `TuVanVien`   | Lưu thông tin nhân viên tư vấn khóa học.                                       |
| `DiemDanh`    | Ghi nhận việc tham dự của học viên theo lớp và ngày học.                       |
| `DiemSo`      | Lưu các điểm kiểm tra của học viên trong lớp.                                  |
| `ChatMessage` | Lưu lịch sử trao đổi giữa người dùng và chatbot.                               |

## Quy tắc nghiệp vụ chính

- Chỉ tài khoản có vai trò phù hợp mới được truy cập chức năng tương ứng.
- Một học viên được gắn với khóa học và lớp học.
- Không thêm học viên nếu lớp đã đạt `SiSoToiDa`.
- Khi thêm học viên, hệ thống tạo đồng thời hồ sơ `HocVien` và tài khoản `NguoiDung`.
- Giá trị học phí phải đóng được lấy từ giá của `KhoaHoc`.
- Giáo viên có thể nhập điểm và điểm danh cho học viên trong lớp phụ trách.
