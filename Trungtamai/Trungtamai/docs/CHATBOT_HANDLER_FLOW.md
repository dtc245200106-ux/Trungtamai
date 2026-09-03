# Chatbot Handler Flow Documentation

## Query Processing Pipeline

```
User sends message via ChatbotController.Chat
    ↓
[1] Authentication Check
    ├─ Extract VaiTro from Session
    ├─ Extract UserId from Session
    └─ If missing → Return 401 Unauthorized
    ↓
[2] Direct Handler Chain (checked in order)
    │
    ├─→ TryHandleFinanceSummaryAsync
    │   └─ Keywords: "chưa đóng", "nợ", "học phí"
    │   └─ Permission: QuanLy only
    │   └─ Returns: "Có X người nợ học phí. Danh sách: Name1 - Zđ; Name2 - Wđ."
    │   └─ DB: Queries HocVien.ConNo = SoTienPhaiDong - SoTienDaDong
    │
    ├─→ TryHandleScheduleClassQueryAsync
    │   └─ Keywords: "thứ/thu", "ngày", "học vào", "lịch học", "tiếng"
    │   └─ Permission: QuanLy, TuVanVien
    │   └─ Returns: "Có X lớp [lang] học [days]. Danh sách: ClassName - GioBatDau-GioKetThuc - PhongHoc"
    │   └─ Logic: Parses "246" → [T2,T4,T6]; matches LopHoc.NgayHoc
    │
    ├─→ TryHandleStudentCountQueryAsync
    │   └─ Keywords: "bao nhiêu học viên", "có bao nhiêu", "tổng số học viên"
    │   └─ Permission: QuanLy only
    │   └─ Returns: "Tổng cộng X học viên. Chi tiết: Hoạt động: Y; Tạm dừng: Z."
    │   └─ DB: COUNT(HocVien) by TrangThai
    │
    ├─→ TryHandlePassingStudentsQueryAsync
    │   └─ Keywords: "học viên nào đạt", "ai đạt", "không đạt", "ai không đạt"
    │   └─ Permission: QuanLy only
    │   └─ Returns: "Có X học viên [đạt/không đạt]. Danh sách: Name1 - 8.5; Name2 - 9.0."
    │   └─ Logic: Checks DiemKiemTraLon >= 8 for "đạt", < 8 for "không đạt"
    │
    └─→ TryHandleClassCountQueryAsync
        └─ Keywords: "bao nhiêu lớp", "có bao nhiêu lớp"
        └─ Permission: QuanLy, TuVanVien
        └─ Returns: "Hiện có X lớp đang hoạt động. Chi tiết: Tiếng Anh: Y; Tiếng Trung: Z."
        └─ DB: COUNT(LopHoc where TrangThai != "Đã kết thúc") by NgonNgu
    ↓
[3] If ANY direct handler returns non-null →
    └─ Save ChatMessage to DB
    └─ Return JSON { answer, vaiTro }
    └─ SKIP Gemini entirely
    ↓
[4] If NO direct handler matched → Fall through to Gemini
    │
    ├─ Get Allowed Data (role-based)
    │   ├─ GetQuanLyDataAsync: Returns full view (all students, teachers, scores, classes)
    │   ├─ GetGiaoVienDataAsync: Returns only teacher's classes and scores
    │   ├─ GetHocVienDataAsync: Returns student's personal class, scores, attendance
    │   └─ GetTuVanVienDataAsync: Returns courses and classes only (no scores, no tuition)
    │
    ├─ Construct Role-Specific Prompt
    │   ├─ Role Instructions (roleInstructions[vaiTro])
    │   ├─ Data Payload (JSON with allowed data)
    │   ├─ Output Format Rules (15+ mandatory rules)
    │   ├─ Page-Specific Context (rolePages[vaiTro])
    │   ├─ Page Rules (pageRules[currentPage])
    │   └─ Role Policy (rolePolicy[vaiTro])
    │
    └─ Call GeminiService.AskAsync(prompt)
       └─ Returns: AI-generated answer based on the data + constraints
    ↓
[5] Save to Chat History & Return
    └─ ChatMessage { UserId, UserMessage, BotResponse }
    └─ Return JSON { answer, vaiTro }
```

---

## Direct Handler Details

### 1. TryHandleFinanceSummaryAsync

**Trigger Keywords:**

- "chưa đóng" (not paid yet)
- "nợ" (owes / in debt)
- "học phí" (tuition)
- "bao nhiêu người chưa đóng" (how many unpaid)

**Permission Check:**

```csharp
if (vaiTro != "QuanLy") return null;  // Only management can see
```

**Query Logic:**

```csharp
var unpaid = await _context.HocViens
    .Where(h => h.ConNo > 0)  // where SoTienPhaiDong - SoTienDaDong > 0
    .OrderBy(h => h.HoTen)
    .Select(h => new { h.HoTen, h.MaHocVien, h.ConNo })
    .ToListAsync();
```

**Return Format:**

```
Hiện có X học viên chưa đóng học phí trên tổng Y học viên.
Danh sách:
- Nguyễn Văn A (HV001) - còn nợ 5.000.000đ
- Trần Thị B (HV002) - còn nợ 2.500.000đ
```

**Database Tables Used:**

- `HocVien` (HoTen, MaHocVien, SoTienPhaiDong, SoTienDaDong)

---

### 2. TryHandleScheduleClassQueryAsync

**Trigger Keywords:**

- "thứ" (day of week in Vietnamese)
- "thu" (alternate spelling)
- "ngày" (date)
- "học vào" (learns on)
- "lịch học" (class schedule)
- Combined with language: "tiếng Anh", "tiếng Trung", "tiếng Hàn", etc.

**Permission Check:**

```csharp
if (vaiTro != "QuanLy" && vaiTro != "TuVanVien") return null;
```

**Day Code Parsing:**
User input "246" or "thứ 2, 4, 6" → Parsed to ["T2", "T4", "T6"]

**Mapping:**

- "1" or "chủ nhật" → "CN"
- "2" or "thứ 2" → "T2"
- "3" or "thứ 3" → "T3"
- ... up to "7" or "thứ 7" → "T7"

**Query Logic:**

```csharp
var classes = await _context.LopHocs
    .Where(l => l.NgonNgu.ToLower().Contains(language))  // e.g., contains "Anh"
    .Where(l => l.TrangThai != "Đã kết thúc")
    .Where(l => ContainsAnyDayCode(l.NgayHoc, targetDays))  // NgayHoc="T2,T4,T6"
    .Select(l => new { l.TenLop, l.NgayHoc, l.GioBatDau, l.GioKetThuc, l.PhongHoc, l.GiaoVien })
    .ToListAsync();
```

**Return Format:**

```
Có X lớp tiếng Anh học vào T2,T4,T6. Danh sách:
- English A1 (ENG001) - T2,T4,T6 - 18:00-19:30 - Phòng 05 - GV: Nguyễn Thị C
- English B1 (ENG002) - T2,T4,T6 - 19:30-21:00 - Phòng 06 - GV: Trần Văn D
```

**Database Tables Used:**

- `LopHoc` (TenLop, NgayHoc, GioBatDau, GioKetThuc, PhongHoc, NgonNgu, TrangThai, GiaoVien)

**Algorithm:**

```
Day Code Parsing:
1. Extract numbers from question: "246" → [2, 4, 6]
2. For each number, map to Vietnamese day:
   - If text like "thứ 2,4,6" detected → split and parse
   - If number "2" detected → convert to "T2"
3. Build target array: ["T2", "T4", "T6"]
4. For each LopHoc.NgayHoc (e.g., "T2,T4,T6"):
   - If intersects with target days → Include in results
```

---

### 3. TryHandleStudentCountQueryAsync

**Trigger Keywords:**

- "bao nhiêu học viên"
- "bao nhieu hoc vien"
- "có bao nhiêu"
- "co bao nhieu"
- "tổng số học viên"
- "tong so hoc vien"

**Permission Check:**

```csharp
if (vaiTro != "QuanLy") return null;
```

**Query Logic:**

```csharp
var totalCount = await _context.HocViens.CountAsync();
var byStatus = await _context.HocViens
    .GroupBy(x => x.TrangThai)
    .Select(g => new { TrangThai = g.Key, Count = g.Count() })
    .ToListAsync();
```

**Return Format:**

```
Tổng cộng 150 học viên. Chi tiết: Hoạt động: 120; Tạm dừng: 25; Chưa bắt đầu: 5.
```

**Database Tables Used:**

- `HocVien` (TrangThai)

---

### 4. TryHandlePassingStudentsQueryAsync

**Trigger Keywords for Passing:**

- "học viên nào đạt"
- "hoc vien nao dat"
- "ai đạt"
- "ai dat"

**Trigger Keywords for Failing:**

- "học viên nào không đạt"
- "hoc vien nao khong dat"
- "ai không đạt"
- "ai khong dat"
- "không đạt"

**Permission Check:**

```csharp
if (vaiTro != "QuanLy") return null;
```

**Pass Criteria:**

- `DiemKiemTraLon >= 8` → "Đạt" (Pass)
- `DiemKiemTraLon < 8` → "Không đạt" (Fail)

**Query Logic:**

```csharp
var scoresToCheck = await _context.DiemSos
    .Where(x => x.DiemKiemTraLon.HasValue)
    .Select(x => new { x.HoTenHocVien, x.MaHocVien, x.DiemKiemTraLon })
    .ToListAsync();

var passing = scoresToCheck.Where(x => x.DiemKiemTraLon >= 8m);
```

**Return Format - Passing:**

```
Có 95 học viên đạt. Danh sách: Nguyễn Văn A - 8.5; Trần Thị B - 9.0; ...
```

**Return Format - Failing:**

```
Có 5 học viên không đạt. Danh sách: Lê Văn C - 7.0; Phạm Thị D - 6.5; ...
```

**Database Tables Used:**

- `DiemSo` (HoTenHocVien, MaHocVien, DiemKiemTraLon)

---

### 5. TryHandleClassCountQueryAsync

**Trigger Keywords:**

- "bao nhiêu lớp"
- "bao nhieu lop"
- "có bao nhiêu lớp"
- "co bao nhieu lop"
- But NOT if "thứ" or "thu" present (to avoid confusion with schedule queries)

**Permission Check:**

```csharp
if (vaiTro != "QuanLy" && vaiTro != "TuVanVien") return null;
```

**Query Logic:**

```csharp
var activeClasses = await _context.LopHocs
    .Where(x => x.TrangThai != "Đã kết thúc")
    .CountAsync();

var byLanguage = await _context.LopHocs
    .Where(x => x.TrangThai != "Đã kết thúc")
    .GroupBy(x => x.NgonNgu)
    .Select(g => new { NgonNgu = g.Key, Count = g.Count() })
    .OrderByDescending(x => x.Count)
    .ToListAsync();
```

**Return Format:**

```
Hiện có 45 lớp đang hoạt động. Chi tiết theo ngôn ngữ: Tiếng Anh: 20; Tiếng Trung: 15; Tiếng Hàn: 10.
```

**Database Tables Used:**

- `LopHoc` (TrangThai, NgonNgu)

---

## Gemini Fallback Behavior

When NO direct handler matches, the system:

1. **Collects allowed data** based on user's role
2. **Constructs comprehensive prompt** with:
   - Role-specific instructions
   - Role policy (what they're allowed to see)
   - Current page context
   - Page-specific rules
   - 15+ output format constraints
   - Full data payload as JSON
3. **Calls Gemini API** with `GeminiService.AskAsync(prompt)`
4. **Returns generated answer** from LLM

### Gemini Constraints in Prompt

```
Rule 1: Không bao giờ sinh ra dữ liệu không tồn tại
Rule 2: Khi hỏi "ai" hoặc "những ai", phải liệt kê tên cụ thể
Rule 3: Khi hỏi "bao nhiêu", phải trả lời bằng số cụ thể
Rule 4: Không được sáng tác tên học viên / giáo viên / lớp
Rule 5: Phải nói "không có dữ liệu" nếu thực sự không có
Rule 6: Mỗi tên học viên phải kèm theo mã / chỉ số
Rule 7: Khi so sánh, phải dùng số liệu từ dữ liệu được cung cấp
Rule 8: Không được truy cập dữ liệu ngoài phạm vi của vai trò
Rule 9: Phải kết thúc bằng dấu chấm.
Rule 10: Các con số tiền tệ phải có đơn vị "đ" hoặc "VND"
Rule 11: Ngày tháng phải in ra đầy đủ ngày / tháng / năm
Rule 12: Giờ học phải ở dạng HH:mm (24h)
Rule 13: Lưu ý kỳ hạn và trạng thái khi trả lời
Rule 14: Phải nêu rõ "hiện tại" là kỳ hạn / khóa nào
Rule 15: Không được tư vấn vượt quá dữ liệu được cung cấp
```

---

## Performance Notes

- **Direct Handlers:** O(1) to O(n) depending on data, very fast, NO API calls
- **Gemini Fallback:** ~2-5 seconds per call, subject to API quota
- **Fallback Strategy:** If Gemini quota exceeded, `GetTeacherScoreComparisonAsync` provides basic comparison without AI

---

## Troubleshooting Guide

| Problem                             | Cause                          | Solution                                          |
| ----------------------------------- | ------------------------------ | ------------------------------------------------- |
| "No handler matched, Gemini failed" | Missing VaiTro in session      | Check session middleware in Program.cs            |
| Chatbot returns fabricated names    | Prompt wasn't strict enough    | Add more "Rule X" constraints to roleInstructions |
| Schedule query returns empty list   | Day parsing failed             | Check ExtractDayCodesFromQuestion() logic         |
| Unpaid list missing students        | ConNo computed wrong           | Verify HocVien.SoTienPhaiDong - SoTienDaDong calc |
| Teacher can see other classes       | Role check missing             | Verify TryHandleXxxAsync checks vaiTro correctly  |
| Student queried gets data leakage   | Allowed data includes too much | Review GetHocVienDataAsync filtering              |
| Very slow response                  | Gemini quota hit               | Implement fallback handler or increase quota      |

---

## Future Enhancement Opportunities

1. **More Direct Handlers:**
   - Top N students by score
   - Attendance percentage by student
   - Revenue by language
   - Teacher load (students per teacher)

2. **Caching:**
   - Cache allowed data for 5 minutes
   - Cache frequent queries (count, schedule)

3. **Context Memory:**
   - Track conversation history
   - "Follow-up question" detection
   - Maintain context across multi-turn dialogue

4. **Analytics:**
   - Log all queries
   - Track which handlers fire most
   - Measure Gemini API spend
   - Identify common patterns

5. **Safety:**
   - Rate limiting per user/role
   - SQL injection prevention
   - Gemini prompt injection defense
