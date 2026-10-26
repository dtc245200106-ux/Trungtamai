using Microsoft.EntityFrameworkCore;
using Trungtamai.Models;

namespace Trungtamai.Data;

public static class DevelopmentDataSeeder
{
    private const string DemoPassword = "Demo123!";

    public static async Task SeedIfEmptyAsync(AppDbContext db)
    {
        if (await db.NguoiDungs.AnyAsync()
            || await db.KhoaHocs.AnyAsync()
            || await db.GiaoViens.AnyAsync()
            || await db.LopHocs.AnyAsync()
            || await db.HocViens.AnyAsync()
            || await db.DiemDanhs.AnyAsync()
            || await db.DiemSos.AnyAsync()
            || await db.ChatMessages.AnyAsync())
        {
            return;
        }

        var courses = new[]
        {
            new KhoaHoc
            {
                TenKhoaHoc = "Tiếng Anh giao tiếp A2",
                NgonNgu = "Tiếng Anh",
                SoBuoi = 24,
                Gia = 4800000m,
                MoTa = "Giao tiếp căn bản theo tình huống hằng ngày."
            },
            new KhoaHoc
            {
                TenKhoaHoc = "Tiếng Nhật sơ cấp N5",
                NgonNgu = "Tiếng Nhật",
                SoBuoi = 30,
                Gia = 6000000m,
                MoTa = "Làm quen chữ viết và giao tiếp tiếng Nhật sơ cấp."
            },
            new KhoaHoc
            {
                TenKhoaHoc = "Tiếng Hàn sơ cấp 1",
                NgonNgu = "Tiếng Hàn",
                SoBuoi = 24,
                Gia = 5200000m,
                MoTa = "Nhập môn tiếng Hàn dành cho người mới bắt đầu."
            }
        };
        db.KhoaHocs.AddRange(courses);

        var teachers = new[]
        {
            new GiaoVien
            {
                MaGiaoVien = "GV001",
                HoTen = "Nguyễn Minh Anh",
                Email = "minhanh.gv@trungtamai.local",
                SoDienThoai = "0900000001",
                NgonNgu = "Tiếng Anh",
                ChuyenMon = "Giao tiếp, IELTS",
                KinhNghiem = 6
            },
            new GiaoVien
            {
                MaGiaoVien = "GV002",
                HoTen = "Trần Sakura",
                Email = "sakura.gv@trungtamai.local",
                SoDienThoai = "0900000002",
                NgonNgu = "Tiếng Nhật",
                ChuyenMon = "JLPT N5-N3",
                KinhNghiem = 4
            }
        };
        db.GiaoViens.AddRange(teachers);

        db.NguoiDungs.AddRange(
            new NguoiDung
            {
                TaiKhoan = "admin@trungtamai.local",
                MatKhau = DemoPassword,
                HoTen = "Quản lý Demo",
                VaiTro = "QuanLy"
            },
            new NguoiDung
            {
                TaiKhoan = teachers[0].Email,
                MatKhau = teachers[0].SoDienThoai[^4..],
                HoTen = teachers[0].HoTen,
                VaiTro = "GiaoVien"
            },
            new NguoiDung
            {
                TaiKhoan = teachers[1].Email,
                MatKhau = teachers[1].SoDienThoai[^4..],
                HoTen = teachers[1].HoTen,
                VaiTro = "GiaoVien"
            },
            new NguoiDung
            {
                TaiKhoan = "tuvan@trungtamai.local",
                MatKhau = DemoPassword,
                HoTen = "Tư vấn Demo",
                VaiTro = "TuVanVien"
            });

        await db.SaveChangesAsync();

        var today = DateTime.Today;
        var todayCode = today.DayOfWeek switch
        {
            DayOfWeek.Monday => "T2",
            DayOfWeek.Tuesday => "T3",
            DayOfWeek.Wednesday => "T4",
            DayOfWeek.Thursday => "T5",
            DayOfWeek.Friday => "T6",
            DayOfWeek.Saturday => "T7",
            _ => "CN"
        };
        var nextDayCode = today.AddDays(1).DayOfWeek switch
        {
            DayOfWeek.Monday => "T2",
            DayOfWeek.Tuesday => "T3",
            DayOfWeek.Wednesday => "T4",
            DayOfWeek.Thursday => "T5",
            DayOfWeek.Friday => "T6",
            DayOfWeek.Saturday => "T7",
            _ => "CN"
        };
        var englishClass = new LopHoc
        {
            MaLop = "LH001",
            TenLop = "Anh giao tiep A2 - Demo",
            KhoaHoc = courses[0].TenKhoaHoc,
            NgonNgu = courses[0].NgonNgu,
            GiaoVien = teachers[0].HoTen,
            SiSoToiDa = 12,
            SiSoHienTai = 3,
            NgayHoc = todayCode,
            GioBatDau = "18:00",
            GioKetThuc = "19:30",
            PhongHoc = "P01",
            CaHoc = $"{todayCode}, 18:00-19:30, P01",
            TrangThai = "Đang học"
        };
        var japaneseClass = new LopHoc
        {
            MaLop = "LH002",
            TenLop = "Nhat ngu N5 - Demo",
            KhoaHoc = courses[1].TenKhoaHoc,
            NgonNgu = courses[1].NgonNgu,
            GiaoVien = teachers[1].HoTen,
            SiSoToiDa = 10,
            SiSoHienTai = 0,
            NgayHoc = nextDayCode,
            GioBatDau = "19:45",
            GioKetThuc = "21:15",
            PhongHoc = "P02",
            CaHoc = $"{nextDayCode}, 19:45-21:15, P02",
            TrangThai = "Sắp khai giảng"
        };
        var endedClass = new LopHoc
        {
            MaLop = "LH003",
            TenLop = "Anh giao tiep A1 - Da ket thuc",
            KhoaHoc = courses[0].TenKhoaHoc,
            NgonNgu = courses[0].NgonNgu,
            GiaoVien = teachers[0].HoTen,
            SiSoToiDa = 10,
            SiSoHienTai = 0,
            NgayHoc = "T7",
            GioBatDau = "09:00",
            GioKetThuc = "10:30",
            PhongHoc = "P03",
            CaHoc = "T7, 09:00-10:30, P03",
            TrangThai = "Đã kết thúc"
        };
        db.LopHocs.AddRange(englishClass, japaneseClass, endedClass);
        await db.SaveChangesAsync();

        var students = new[]
        {
            new HocVien
            {
                MaHocVien = "HV001",
                HoTen = "Lê Hoàng Nam",
                SoDienThoai = "0911111001",
                Email = "nam.hv@trungtamai.local",
                KhoaHoc = courses[0].TenKhoaHoc,
                LopHoc = englishClass.TenLop,
                NgonNgu = courses[0].NgonNgu,
                TrangThai = "Đang học",
                SoTienPhaiDong = courses[0].Gia,
                SoTienDaDong = 2400000m,
                NgayDongCuoi = today.AddDays(-3)
            },
            new HocVien
            {
                MaHocVien = "HV002",
                HoTen = "Phạm Gia Hân",
                SoDienThoai = "0911111002",
                Email = "han.hv@trungtamai.local",
                KhoaHoc = courses[0].TenKhoaHoc,
                LopHoc = englishClass.TenLop,
                NgonNgu = courses[0].NgonNgu,
                TrangThai = "Đang học",
                SoTienPhaiDong = courses[0].Gia,
                SoTienDaDong = 0m
            },
            new HocVien
            {
                MaHocVien = "HV003",
                HoTen = "Vũ Đức Minh",
                SoDienThoai = "0911111003",
                Email = "minh.hv@trungtamai.local",
                KhoaHoc = courses[0].TenKhoaHoc,
                LopHoc = englishClass.TenLop,
                NgonNgu = courses[0].NgonNgu,
                TrangThai = "Đang học",
                SoTienPhaiDong = courses[0].Gia,
                SoTienDaDong = courses[0].Gia,
                NgayDongCuoi = today.AddDays(-1)
            },
            new HocVien
            {
                MaHocVien = "HV004",
                HoTen = "Đặng Thuỳ Dương",
                SoDienThoai = "0911111004",
                Email = "duong.hv@trungtamai.local",
                KhoaHoc = courses[0].TenKhoaHoc,
                LopHoc = englishClass.TenLop,
                NgonNgu = courses[0].NgonNgu,
                TrangThai = "Tạm ngưng",
                SoTienPhaiDong = courses[0].Gia,
                SoTienDaDong = 1000000m
            },
            new HocVien
            {
                MaHocVien = "HV005",
                HoTen = "Ngô Mai Chi",
                SoDienThoai = "0911111005",
                Email = "chi.hv@trungtamai.local",
                KhoaHoc = courses[0].TenKhoaHoc,
                LopHoc = endedClass.TenLop,
                NgonNgu = courses[0].NgonNgu,
                TrangThai = "Đã kết thúc",
                SoTienPhaiDong = courses[0].Gia,
                SoTienDaDong = courses[0].Gia,
                NgayDongCuoi = today.AddDays(-30)
            }
        };
        db.HocViens.AddRange(students);
        db.NguoiDungs.AddRange(students.Select(student => new NguoiDung
        {
            TaiKhoan = student.Email,
            MatKhau = student.SoDienThoai[^4..],
            HoTen = student.HoTen,
            VaiTro = "HocVien"
        }));
        await db.SaveChangesAsync();

        db.DiemDanhs.AddRange(
            MakeAttendance(englishClass, students[0], today, "Có mặt", "Đúng giờ", teachers[0].HoTen),
            MakeAttendance(englishClass, students[1], today, "Muộn", "Đến muộn 10 phút", teachers[0].HoTen),
            MakeAttendance(englishClass, students[2], today, "Vắng", "Chưa liên hệ", teachers[0].HoTen));

        db.DiemSos.AddRange(
            MakeScore(englishClass, students[0], 8m, 8.5m, 9m, 9m, teachers[0].HoTen, "Tiến bộ tốt"),
            MakeScore(englishClass, students[1], 6m, 6.5m, 7m, 6.5m, teachers[0].HoTen, "Cần ôn thêm"),
            MakeScore(englishClass, students[2], 8m, 8m, 8.5m, null, teachers[0].HoTen, "Chưa thi cuối khóa"));

        await db.SaveChangesAsync();
    }

    private static DiemDanh MakeAttendance(
        LopHoc lop,
        HocVien hocVien,
        DateTime ngay,
        string trangThai,
        string ghiChu,
        string giaoVien)
    {
        return new DiemDanh
        {
            LopHocId = lop.Id,
            MaLop = lop.MaLop,
            TenLop = lop.TenLop,
            HocVienId = hocVien.Id,
            MaHocVien = hocVien.MaHocVien,
            HoTenHocVien = hocVien.HoTen,
            NgayHoc = ngay,
            TrangThai = trangThai,
            GhiChu = ghiChu,
            GiaoVienDiemDanh = giaoVien,
            ThoiGianDiemDanh = DateTime.Now
        };
    }

    private static DiemSo MakeScore(
        LopHoc lop,
        HocVien hocVien,
        decimal? diemKt1,
        decimal? diemKt2,
        decimal? diemKt3,
        decimal? diemCuoiKhoa,
        string giaoVien,
        string ghiChu)
    {
        return new DiemSo
        {
            HocVienId = hocVien.Id,
            LopHocId = lop.Id,
            MaHocVien = hocVien.MaHocVien,
            HoTenHocVien = hocVien.HoTen,
            MaLop = lop.MaLop,
            TenLop = lop.TenLop,
            DiemKT1 = diemKt1,
            DiemKT2 = diemKt2,
            DiemKT3 = diemKt3,
            DiemKiemTraLon = diemCuoiKhoa,
            GhiChu = ghiChu,
            GiaoVienNhap = giaoVien,
            NgayCapNhat = DateTime.Now
        };
    }
}
