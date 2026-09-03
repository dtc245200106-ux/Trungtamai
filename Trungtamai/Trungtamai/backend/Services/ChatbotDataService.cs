using Microsoft.EntityFrameworkCore;
using Trungtamai.Data;

namespace Trungtamai.Services
{
    /// <summary>
    /// Service để xử lý các câu hỏi chatbot cụ thể
    /// AI-generated & reviewed: Các handler methods được AI tạo, sinh viên đã review logic
    /// </summary>
    public class ChatbotDataService
    {
        private readonly AppDbContext _context;

        private static readonly HashSet<string> SupportedRoles =
            new(StringComparer.Ordinal)
            {
                "QuanLy",
                "GiaoVien",
                "HocVien",
                "TuVanVien"
            };

        public ChatbotDataService(AppDbContext context)
        {
            _context = context;
        }

        public bool IsSupportedRole(string vaiTro)
        {
            return SupportedRoles.Contains(vaiTro);
        }

        public async Task<string?> TryHandleFinanceSummaryAsync(string vaiTro, string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return null;

            var q = question.Trim();
            var lower = q.ToLowerInvariant();

            var asksUnpaidTuition = lower.Contains("chưa đóng học phí")
                || lower.Contains("chua dong hoc phi")
                || lower.Contains("còn nợ học phí")
                || lower.Contains("con no hoc phi")
                || lower.Contains("chưa đóng")
                || lower.Contains("chua dong")
                || lower.Contains("bao nhiêu người chưa đóng")
                || lower.Contains("so nguoi chua dong");

            if (!asksUnpaidTuition)
                return null;

            if (vaiTro != "QuanLy")
                return null;

            var tongHocVien = await _context.HocViens.CountAsync();
            var chuaDongList = await _context.HocViens
                .Where(x => x.SoTienPhaiDong > x.SoTienDaDong)
                .OrderBy(x => x.HoTen)
                .Select(x => new { x.HoTen, x.MaHocVien, x.KhoaHoc, x.LopHoc, x.SoTienPhaiDong, x.SoTienDaDong, ConNo = x.SoTienPhaiDong - x.SoTienDaDong })
                .ToListAsync();

            var chuaDong = chuaDongList.Count;

            if (chuaDong == 0)
                return "Hiện tại không có học viên nào chưa đóng học phí.";

            var danhSach = string.Join("; ", chuaDongList.Select(x => $"{x.HoTen} ({x.MaHocVien}) - còn nợ {x.ConNo:N0}đ"));

            return $"Hiện có {chuaDong} học viên chưa đóng học phí trên tổng {tongHocVien} học viên. Danh sách: {danhSach}.";
        }

        public async Task<string?> TryHandleScheduleClassQueryAsync(string vaiTro, string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return null;

            var q = question.Trim();
            var lower = q.ToLowerInvariant();

            var isScheduleQuestion = lower.Contains("thứ") || lower.Contains("thu") ||
                                     lower.Contains("ngày") || lower.Contains("ngay") ||
                                     lower.Contains("học vào") || lower.Contains("hoc vao") ||
                                     lower.Contains("lịch học") || lower.Contains("lich hoc") ||
                                     lower.Contains("học thứ") || lower.Contains("hoc thu") ||
                                     lower.Contains("đi vào thứ") || lower.Contains("di vao thu");

            var mentionsEnglish = lower.Contains("tiếng anh") || lower.Contains("tieng anh") ||
                                  lower.Contains("lớp anh") || lower.Contains("lop anh") ||
                                  lower.Contains("english");

            if (!isScheduleQuestion || !mentionsEnglish || (vaiTro != "QuanLy" && vaiTro != "TuVanVien"))
                return null;

            var dayCodes = ExtractDayCodesFromQuestion(q);
            if (dayCodes.Count == 0)
                return null;

            var allClasses = await _context.LopHocs
                .Where(x => x.TrangThai != "Đã kết thúc")
                .Where(x => x.NgonNgu != null && (x.NgonNgu.Contains("Tiếng Anh") || x.NgonNgu.Contains("tiếng anh") || x.NgonNgu.Contains("English") || x.NgonNgu.Contains("Anh")))
                .OrderBy(x => x.TenLop)
                .ToListAsync();

            var matching = allClasses
                .Where(x => !string.IsNullOrWhiteSpace(x.NgayHoc) && ContainsAnyDayCode(x.NgayHoc, dayCodes))
                .Select(x => new
                {
                    x.MaLop,
                    x.TenLop,
                    x.NgonNgu,
                    x.NgayHoc,
                    x.GioBatDau,
                    x.GioKetThuc,
                    x.PhongHoc,
                    x.GiaoVien,
                    x.KhoaHoc,
                    x.SiSoHienTai,
                    x.SiSoToiDa
                })
                .ToList();

            if (matching.Count == 0)
                return $"Hiện chưa có lớp tiếng Anh nào học vào {string.Join(", ", dayCodes)}.";

            var danhSach = string.Join("; ", matching.Select(x =>
                $"{x.TenLop} ({x.MaLop}) - {x.NgayHoc} - {x.GioBatDau}-{x.GioKetThuc} - {x.PhongHoc} - GV: {x.GiaoVien}"));

            return $"Có {matching.Count} lớp tiếng Anh học vào {string.Join(", ", dayCodes)}. Danh sách: {danhSach}.";
        }

        // Tách mã ngày học từ câu hỏi như: "246" hoặc "thứ 2,4,6".
        // Mục tiêu là đưa về danh sách day code: T2, T4, T6 để so khớp với LopHoc.NgayHoc.
        private static List<string> ExtractDayCodesFromQuestion(string question)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var text = question.Trim();

            var matches = System.Text.RegularExpressions.Regex.Matches(text, @"\d+");
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var value = match.Value;
                if (value.Length > 1)
                {
                    foreach (var ch in value)
                    {
                        if (int.TryParse(ch.ToString(), out var num))
                        {
                            var code = ToDayCode(num);
                            if (!string.IsNullOrEmpty(code))
                                result.Add(code);
                        }
                    }
                }
                else if (int.TryParse(value, out var num))
                {
                    var code = ToDayCode(num);
                    if (!string.IsNullOrEmpty(code))
                        result.Add(code);
                }
            }

            if (result.Count == 0)
            {
                var dayMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["cn"] = "CN",
                    ["chủ nhật"] = "CN",
                    ["thu 2"] = "T2",
                    ["thứ 2"] = "T2",
                    ["thu 3"] = "T3",
                    ["thứ 3"] = "T3",
                    ["thu 4"] = "T4",
                    ["thứ 4"] = "T4",
                    ["thu 5"] = "T5",
                    ["thứ 5"] = "T5",
                    ["thu 6"] = "T6",
                    ["thứ 6"] = "T6",
                    ["thu 7"] = "T7",
                    ["thứ 7"] = "T7"
                };

                foreach (var kvp in dayMap)
                {
                    if (text.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                        result.Add(kvp.Value);
                }
            }

            return result
                .OrderBy(x => x switch
                {
                    "T2" => 2,
                    "T3" => 3,
                    "T4" => 4,
                    "T5" => 5,
                    "T6" => 6,
                    "T7" => 7,
                    "CN" => 8,
                    _ => 99
                })
                .ToList();
        }

        private static string ToDayCode(int dayNumber)
        {
            return dayNumber switch
            {
                1 => "CN",
                2 => "T2",
                3 => "T3",
                4 => "T4",
                5 => "T5",
                6 => "T6",
                7 => "T7",
                _ => string.Empty
            };
        }

        // Kiểm tra xem lịch học của lớp có trùng với ít nhất một ngày trong câu hỏi hay không.
        // Ví dụ: LopHoc.NgayHoc = "T2,T4,T6" và targetDays = [T2, T6] => true.
        private static bool ContainsAnyDayCode(string ngayHoc, IEnumerable<string> targetDays)
        {
            if (string.IsNullOrWhiteSpace(ngayHoc))
                return false;

            var normalized = ngayHoc
                .Replace(" ", "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var set = new HashSet<string>(normalized, StringComparer.OrdinalIgnoreCase);
            foreach (var day in targetDays)
            {
                if (set.Contains(day))
                    return true;
            }

            return false;
        }

        // Xử lý câu hỏi thống kê tổng số học viên theo vai trò quản lý.
        // Dùng khi người dùng hỏi: "bao nhiêu học viên?", "tổng số học viên".
        public async Task<string?> TryHandleStudentCountQueryAsync(string vaiTro, string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return null;

            var lower = question.ToLowerInvariant();

            var asksStudentCount = lower.Contains("bao nhiêu học viên") || lower.Contains("bao nhieu hoc vien") ||
                                   lower.Contains("có bao nhiêu") || lower.Contains("co bao nhieu") ||
                                   lower.Contains("tổng số học viên") || lower.Contains("tong so hoc vien");

            if (!asksStudentCount || vaiTro != "QuanLy")
                return null;

            var totalCount = await _context.HocViens.CountAsync();
            var byStatus = await _context.HocViens
                .GroupBy(x => x.TrangThai)
                .Select(g => new { TrangThai = g.Key, Count = g.Count() })
                .ToListAsync();

            var statusBreakdown = string.Join("; ", byStatus.Select(x => $"{x.TrangThai}: {x.Count}"));
            return $"Tổng cộng {totalCount} học viên. Chi tiết: {statusBreakdown}.";
        }

        // Xử lý câu hỏi về học viên đạt/không đạt.
        // Dùng khi người dùng hỏi: "ai đạt?", "học viên nào không đạt?".
        public async Task<string?> TryHandlePassingStudentsQueryAsync(string vaiTro, string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return null;

            var lower = question.ToLowerInvariant();

            var asksPassingStudents = (lower.Contains("học viên nào đạt") || lower.Contains("hoc vien nao dat") ||
                                       lower.Contains("ai đạt") || lower.Contains("ai dat") ||
                                       lower.Contains("đạt") && lower.Contains("điểm")) &&
                                      !lower.Contains("không đạt");

            var asksFailingStudents = (lower.Contains("học viên nào không đạt") || lower.Contains("hoc vien nao khong dat") ||
                                       lower.Contains("ai không đạt") || lower.Contains("ai khong dat") ||
                                       lower.Contains("không đạt"));

            if ((!asksPassingStudents && !asksFailingStudents) || vaiTro != "QuanLy")
                return null;

            var scoresToCheck = await _context.DiemSos
                .Where(x => x.DiemKiemTraLon.HasValue)
                .Select(x => new { x.HoTenHocVien, x.MaHocVien, x.TenLop, x.DiemKiemTraLon, x.GhiChu })
                .ToListAsync();

            if (scoresToCheck.Count == 0)
                return "Chưa có dữ liệu điểm số kiểm tra lớn.";

            if (asksPassingStudents)
            {
                var passing = scoresToCheck
                    .Where(x => x.DiemKiemTraLon >= 8m)
                    .OrderBy(x => x.HoTenHocVien)
                    .ToList();

                if (passing.Count == 0)
                    return "Hiện chưa có học viên nào đạt điểm kiểm tra lớn >= 8.";

                var danhSach = string.Join("; ", passing.Select(x => $"{x.HoTenHocVien} ({x.MaHocVien}) - {x.DiemKiemTraLon}"));
                return $"Có {passing.Count} học viên đạt. Danh sách: {danhSach}.";
            }
            else
            {
                var failing = scoresToCheck
                    .Where(x => x.DiemKiemTraLon < 8m)
                    .OrderBy(x => x.HoTenHocVien)
                    .ToList();

                if (failing.Count == 0)
                    return "Hiện không có học viên nào không đạt.";

                var danhSach = string.Join("; ", failing.Select(x => $"{x.HoTenHocVien} ({x.MaHocVien}) - {x.DiemKiemTraLon}"));
                return $"Có {failing.Count} học viên không đạt. Danh sách: {danhSach}.";
            }
        }

        // Xử lý câu hỏi thống kê số lớp đang hoạt động theo ngôn ngữ.
        // Ví dụ: "bao nhiêu lớp?" hoặc "có bao nhiêu lớp tiếng Anh?".
        public async Task<string?> TryHandleClassCountQueryAsync(string vaiTro, string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return null;

            var lower = question.ToLowerInvariant();

            var asksClassCount = (lower.Contains("bao nhiêu lớp") || lower.Contains("bao nhieu lop") ||
                                  lower.Contains("có bao nhiêu lớp") || lower.Contains("co bao nhieu lop")) &&
                                 !lower.Contains("thứ") && !lower.Contains("thu");

            if (!asksClassCount || (vaiTro != "QuanLy" && vaiTro != "TuVanVien"))
                return null;

            var activeClasses = await _context.LopHocs
                .Where(x => x.TrangThai != "Đã kết thúc")
                .CountAsync();

            var byLanguage = await _context.LopHocs
                .Where(x => x.TrangThai != "Đã kết thúc")
                .GroupBy(x => x.NgonNgu)
                .Select(g => new { NgonNgu = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var breakdown = string.Join("; ", byLanguage.Select(x => $"{x.NgonNgu}: {x.Count}"));
            return $"Hiện có {activeClasses} lớp đang hoạt động. Chi tiết theo ngôn ngữ: {breakdown}.";
        }

        public async Task<string?> GetTeacherScoreComparisonAsync(int userId, string question)
        {
            if (!question.Contains("điểm", StringComparison.OrdinalIgnoreCase))
                return null;

            var nguoiDung = await _context.NguoiDungs.FindAsync(userId);
            if (nguoiDung == null)
                return null;

            var giaoVien = await _context.GiaoViens.FirstOrDefaultAsync(x =>
                x.Email == nguoiDung.TaiKhoan ||
                x.MaGiaoVien == nguoiDung.TaiKhoan ||
                x.HoTen == nguoiDung.HoTen);

            if (giaoVien == null)
                return null;

            var diemSo = await _context.DiemSos
                .Where(x => _context.LopHocs.Any(l =>
                    l.Id == x.LopHocId && l.GiaoVien == giaoVien.HoTen))
                .Select(x => new
                {
                    x.HoTenHocVien,
                    x.MaHocVien,
                    x.TenLop,
                    x.DiemKT1,
                    x.DiemKT2,
                    x.DiemKT3,
                    x.DiemKiemTraLon
                })
                .ToListAsync();

            var coDiem = diemSo
                .Select(x => new
                {
                    x.HoTenHocVien,
                    x.MaHocVien,
                    x.TenLop,
                    Diem = TinhDiemTrungBinh(x.DiemKT1, x.DiemKT2, x.DiemKT3, x.DiemKiemTraLon)
                })
                .Where(x => x.Diem.HasValue)
                .ToList();

            if (coDiem.Count == 0)
                return "Chưa có dữ liệu điểm số của học viên trong các lớp bạn phụ trách.";

            var laCaoNhat = question.Contains("cao nhất", StringComparison.OrdinalIgnoreCase) ||
                            question.Contains("cao nhat", StringComparison.OrdinalIgnoreCase);
            var laThapNhat = question.Contains("thấp nhất", StringComparison.OrdinalIgnoreCase) ||
                             question.Contains("thap nhat", StringComparison.OrdinalIgnoreCase);

            if (!laCaoNhat && !laThapNhat)
                return null;

            var diemMucTieu = laCaoNhat
                ? coDiem.Max(x => x.Diem!.Value)
                : coDiem.Min(x => x.Diem!.Value);
            var hocViens = coDiem
                .Where(x => x.Diem == diemMucTieu)
                .Select(x => $"{x.HoTenHocVien} ({x.MaHocVien}), lớp {x.TenLop}: {x.Diem:0.0}")
                .ToList();
            var moTa = laCaoNhat ? "cao nhất" : "thấp nhất";

            return $"Học viên có điểm trung bình {moTa} là: {string.Join("; ", hocViens)}.";
        }

        public async Task<string> GetAllowedDataAsync(
            int userId,
            string vaiTro)
        {
            switch (vaiTro)
            {
                case "QuanLy":
                    return await GetQuanLyDataAsync();

                case "GiaoVien":
                    return await GetGiaoVienDataAsync(userId);

                case "HocVien":
                    return await GetHocVienDataAsync(userId);

                case "TuVanVien":
                    return await GetTuVanVienDataAsync();

                default:
                    return "Tài khoản không có quyền sử dụng chatbot.";
            }
        }

        // ==========================================
        // QUẢN LÝ
        // ==========================================
        private async Task<string> GetQuanLyDataAsync()
        {
            var tongHocVien = await _context.HocViens.CountAsync();
            var tongGiaoVien = await _context.GiaoViens.CountAsync();
            var tongKhoaHoc = await _context.KhoaHocs.CountAsync();

            var lopHoc = await _context.LopHocs
                .Where(x => x.TrangThai != "Đã kết thúc")
                .Select(x => new
                {
                    x.MaLop,
                    x.TenLop,
                    x.KhoaHoc,
                    x.NgonNgu,
                    x.GiaoVien,
                    x.SiSoHienTai,
                    x.SiSoToiDa,
                    x.TrangThai
                })
                .ToListAsync();

            var hocViens = await _context.HocViens
                .Select(x => new
                {
                    x.MaHocVien,
                    x.HoTen,
                    x.Email,
                    x.KhoaHoc,
                    x.LopHoc,
                    x.NgonNgu,
                    x.TrangThai,
                    x.SoTienPhaiDong,
                    x.SoTienDaDong,
                    ConNo = x.SoTienPhaiDong - x.SoTienDaDong
                })
                .OrderBy(x => x.HoTen)
                .ToListAsync();

            var diemSo = await _context.DiemSos
                .Select(x => new
                {
                    x.MaHocVien,
                    x.HoTenHocVien,
                    x.MaLop,
                    x.TenLop,
                    x.DiemKT1,
                    x.DiemKT2,
                    x.DiemKT3,
                    x.DiemKiemTraLon,
                    DiemTrungBinh = x.DiemTrungBinh,
                    KetQua = x.KetQua,
                    x.GiaoVienNhap,
                    x.NgayCapNhat
                })
                .OrderByDescending(x => x.NgayCapNhat)
                .ToListAsync();

            return $"""
            DỮ LIỆU QUẢN LÝ TRUNG TÂM

            Tổng học viên: {tongHocVien}
            Tổng giáo viên: {tongGiaoVien}
            Tổng khóa học: {tongKhoaHoc}

            DANH SÁCH LỚP:
            {System.Text.Json.JsonSerializer.Serialize(lopHoc)}

            DANH SÁCH HỌC VIÊN:
            {System.Text.Json.JsonSerializer.Serialize(hocViens)}

            DANH SÁCH ĐIỂM SỐ:
            {System.Text.Json.JsonSerializer.Serialize(diemSo)}
            """;
        }

        // ==========================================
        // GIÁO VIÊN
        // ==========================================
        private async Task<string> GetGiaoVienDataAsync(int userId)
        {
            var nguoiDung = await _context.NguoiDungs
                .FindAsync(userId);

            if (nguoiDung == null)
                return "Không tìm thấy tài khoản.";

            var giaoVien = await _context.GiaoViens
                .FirstOrDefaultAsync(x =>
                    x.Email == nguoiDung.TaiKhoan);

            giaoVien ??= await _context.GiaoViens
                .FirstOrDefaultAsync(x =>
                    x.MaGiaoVien == nguoiDung.TaiKhoan);

            giaoVien ??= await _context.GiaoViens
                .FirstOrDefaultAsync(x =>
                    x.HoTen == nguoiDung.HoTen);

            if (giaoVien == null)
                return "Không tìm thấy hồ sơ giáo viên.";

            var lopHoc = await _context.LopHocs
                .Where(x => x.GiaoVien == giaoVien.HoTen)
                .Select(x => new
                {
                    x.MaLop,
                    x.TenLop,
                    x.KhoaHoc,
                    x.NgonNgu,
                    x.SiSoHienTai,
                    x.SiSoToiDa,
                    x.NgayHoc,
                    x.GioBatDau,
                    x.GioKetThuc,
                    x.PhongHoc,
                    x.TrangThai
                })
                .ToListAsync();

            var lopIds = await _context.LopHocs
                .Where(x => x.GiaoVien == giaoVien.HoTen)
                .Select(x => x.Id)
                .ToListAsync();

            var diemSoRaw = await _context.DiemSos
                .Where(x => lopIds.Contains(x.LopHocId))
                .Select(x => new
                {
                    x.MaHocVien,
                    x.HoTenHocVien,
                    x.MaLop,
                    x.TenLop,
                    x.DiemKT1,
                    x.DiemKT2,
                    x.DiemKT3,
                    x.DiemKiemTraLon,
                    x.GhiChu
                })
                .ToListAsync();

            var diemSo = diemSoRaw.Select(x => new
            {
                x.MaHocVien,
                x.HoTenHocVien,
                x.MaLop,
                x.TenLop,
                x.DiemKT1,
                x.DiemKT2,
                x.DiemKT3,
                x.DiemKiemTraLon,
                diemTrungBinh = TinhDiemTrungBinh(x.DiemKT1, x.DiemKT2, x.DiemKT3, x.DiemKiemTraLon),
                ketQua = x.DiemKiemTraLon.HasValue
                    ? (x.DiemKiemTraLon.Value >= 8m ? "Đạt" : "Không đạt")
                    : "Chưa thi",
                x.GhiChu
            }).ToList();

            return $"""
            DỮ LIỆU GIÁO VIÊN

            Giáo viên: {giaoVien.HoTen}
            Email: {giaoVien.Email}
            Chuyên môn: {giaoVien.ChuyenMon}

            CÁC LỚP ĐƯỢC PHÂN CÔNG:
            {System.Text.Json.JsonSerializer.Serialize(lopHoc)}

            ĐIỂM SỐ HỌC VIÊN TRONG CÁC LỚP CỦA GIÁO VIÊN:
            {System.Text.Json.JsonSerializer.Serialize(diemSo)}
            """;
        }

        // ==========================================
        // HỌC VIÊN
        // ==========================================
        private async Task<string> GetHocVienDataAsync(int userId)
        {
            var nguoiDung = await _context.NguoiDungs
                .FindAsync(userId);

            if (nguoiDung == null)
                return "Không tìm thấy tài khoản.";

            var hocVien = await _context.HocViens
                .FirstOrDefaultAsync(x =>
                    x.Email == nguoiDung.TaiKhoan);

            hocVien ??= await _context.HocViens
                .FirstOrDefaultAsync(x =>
                    x.MaHocVien == nguoiDung.TaiKhoan);

            if (hocVien == null)
                return "Không tìm thấy hồ sơ học viên.";

            var lop = await _context.LopHocs
                .FirstOrDefaultAsync(x =>
                    x.TenLop == hocVien.LopHoc);

            var diemSoRaw = await _context.DiemSos
                .Where(x => x.HocVienId == hocVien.Id)
                .Select(x => new
                {
                    x.MaLop,
                    x.TenLop,
                    x.DiemKT1,
                    x.DiemKT2,
                    x.DiemKT3,
                    x.DiemKiemTraLon,
                    x.GhiChu
                })
                .ToListAsync();

            var diemSo = diemSoRaw.Select(x => new
            {
                x.MaLop,
                x.TenLop,
                x.DiemKT1,
                x.DiemKT2,
                x.DiemKT3,
                x.DiemKiemTraLon,
                diemTrungBinh = TinhDiemTrungBinh(x.DiemKT1, x.DiemKT2, x.DiemKT3, x.DiemKiemTraLon),
                ketQua = x.DiemKiemTraLon.HasValue
                    ? (x.DiemKiemTraLon.Value >= 8m ? "Đạt" : "Không đạt")
                    : "Chưa thi",
                x.GhiChu
            }).ToList();

            var diemDanh = await _context.DiemDanhs
                .Where(x => x.HocVienId == hocVien.Id)
                .OrderByDescending(x => x.NgayHoc)
                .Select(x => new
                {
                    x.MaLop,
                    x.TenLop,
                    x.NgayHoc,
                    x.TrangThai,
                    x.GhiChu
                })
                .ToListAsync();

            return $"""
            DỮ LIỆU RIÊNG CỦA HỌC VIÊN

            Họ tên: {hocVien.HoTen}
            Mã học viên: {hocVien.MaHocVien}
            Email: {hocVien.Email}
            Khóa học: {hocVien.KhoaHoc}
            Lớp: {hocVien.LopHoc}
            Ngôn ngữ: {hocVien.NgonNgu}
            Trạng thái: {hocVien.TrangThai}

            Học phí phải đóng: {hocVien.SoTienPhaiDong}
            Đã đóng: {hocVien.SoTienDaDong}
            Còn nợ: {hocVien.ConNo}

            THÔNG TIN LỚP:
            {System.Text.Json.JsonSerializer.Serialize(lop)}

            ĐIỂM SỐ CỦA CHÍNH HỌC VIÊN:
            {System.Text.Json.JsonSerializer.Serialize(diemSo)}

            ĐIỂM DANH CỦA CHÍNH HỌC VIÊN:
            {System.Text.Json.JsonSerializer.Serialize(diemDanh)}
            """;
        }

        private static decimal? TinhDiemTrungBinh(
            decimal? diemKT1,
            decimal? diemKT2,
            decimal? diemKT3,
            decimal? diemKiemTraLon)
        {
            decimal tong = 0;
            var heSo = 0;

            if (diemKT1.HasValue) { tong += diemKT1.Value; heSo++; }
            if (diemKT2.HasValue) { tong += diemKT2.Value; heSo++; }
            if (diemKT3.HasValue) { tong += diemKT3.Value; heSo++; }
            if (diemKiemTraLon.HasValue) { tong += diemKiemTraLon.Value * 2; heSo += 2; }

            return heSo > 0 ? Math.Round(tong / heSo, 1) : null;
        }

        // ==========================================
        // TƯ VẤN VIÊN
        // ==========================================
        private async Task<string> GetTuVanVienDataAsync()
        {
            var khoaHoc = await _context.KhoaHocs
                .Select(x => new
                {
                    x.TenKhoaHoc,
                    x.NgonNgu,
                    x.SoBuoi,
                    x.Gia,
                    x.MoTa
                })
                .ToListAsync();

            var lopHoc = await _context.LopHocs
                .Where(x => x.TrangThai != "Đã kết thúc")
                .Select(x => new
                {
                    x.MaLop,
                    x.TenLop,
                    x.KhoaHoc,
                    x.NgonNgu,
                    x.GiaoVien,
                    x.SiSoHienTai,
                    x.SiSoToiDa,
                    x.NgayHoc,
                    x.GioBatDau,
                    x.GioKetThuc,
                    x.PhongHoc,
                    x.TrangThai
                })
                .ToListAsync();

            return $"""
            DỮ LIỆU TƯ VẤN

            KHÓA HỌC:
            {System.Text.Json.JsonSerializer.Serialize(khoaHoc)}

            CÁC LỚP ĐANG MỞ:
            {System.Text.Json.JsonSerializer.Serialize(lopHoc)}

            Chỉ sử dụng dữ liệu khóa học và lớp học để tư vấn.
            Không được cung cấp điểm số, điểm danh,
            học phí hoặc dữ liệu cá nhân của học viên.
            """;
        }
    }
}