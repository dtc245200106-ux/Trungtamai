# Chatbot Test Scenarios by Role

## Role: Quản lý (Management)

Vai trò này có quyền xem: Tổng quan, Học viên, Giáo viên, Khóa học, Lớp học, Lịch học, Học phí, Kết quả học tập, Thống kê.

### Scenario 1.1: Student Count Query

**User Question:** "Bao nhiêu học viên?"
**Expected Answer Format:** "Tổng cộng X học viên. Chi tiết: Hoạt động: Y; Tạm dừng: Z."
**Handler:** `TryHandleStudentCountQueryAsync`

### Scenario 1.2: Unpaid Tuition Query

**User Question:** "Có bao nhiêu người chưa đóng học phí?"
**Expected Answer Format:** "Hiện có X học viên chưa đóng học phí trên tổng Y học viên. Danh sách: Name1 (ID1) - còn nợ Zđ; Name2 (ID2) - còn nợ Wđ."
**Handler:** `TryHandleFinanceSummaryAsync`

### Scenario 1.3: Passing Students Query

**User Question:** "Học viên nào đạt điểm kiểm tra lớn?"
**Expected Answer Format:** "Có X học viên đạt. Danh sách: Name1 (ID1) - 8.5; Name2 (ID2) - 9.0."
**Handler:** `TryHandlePassingStudentsQueryAsync`

### Scenario 1.4: Failing Students Query

**User Question:** "Ai không đạt?"
**Expected Answer Format:** "Có X học viên không đạt. Danh sách: Name1 (ID1) - 6.5; Name2 (ID2) - 7.0."
**Handler:** `TryHandlePassingStudentsQueryAsync`

### Scenario 1.5: Class Count Query

**User Question:** "Bao nhiêu lớp đang hoạt động?"
**Expected Answer Format:** "Hiện có X lớp đang hoạt động. Chi tiết theo ngôn ngữ: Tiếng Anh: Y; Tiếng Trung: Z."
**Handler:** `TryHandleClassCountQueryAsync`

### Scenario 1.6: English Classes on Specific Days

**User Question:** "Có lớp tiếng Anh nào học vào thứ 246?" (means Monday, Wednesday, Friday)
**Expected Answer Format:** "Có X lớp tiếng Anh học vào T2, T4, T6. Danh sách: ClassName1 (ClassCode1) - T2,T4,T6 - 18:00-19:30 - P05 - GV: TeacherName; ..."
**Handler:** `TryHandleScheduleClassQueryAsync`

### Scenario 1.7: General Dashboard Question (Gemini)

**User Question:** "Tổng quan về trung tâm như thế nào?"
**Expected Answer:** Gemini will use the allowed data payload including student list, teacher list, class list, scores, and provide a summary.
**Fallback:** Gemini API

### Scenario 1.8: Complex Query with Filters (Gemini)

**User Question:** "Chi tiết các lớp tiếng Trung chưa kết thúc"
**Expected Answer:** Gemini will extract active Chinese classes from the allowed data and provide details.
**Fallback:** Gemini API

---

## Role: Giáo viên (Teacher)

Vai trò này có quyền xem: Tổng quan, Lớp của tôi, Lịch dạy, Điểm số, Kết quả học tập.

### Scenario 2.1: Teacher's Classes

**User Question:** "Tôi dạy những lớp nào?"
**Expected Answer:** Gemini will extract and list all classes assigned to this teacher from the allowed data.
**Fallback:** Gemini API

### Scenario 2.2: Student List in Teacher's Class

**User Question:** "Có bao nhiêu học viên trong lớp của tôi?"
**Expected Answer:** Gemini will count students in teacher's assigned classes from the allowed data.
**Fallback:** Gemini API

### Scenario 2.3: Score Summary for Teacher's Class

**User Question:** "Điểm kiểm tra lớn của lớp tôi như thế nào?"
**Expected Answer:** Gemini will summarize final exam scores from the allowed data for the teacher's classes.
**Fallback:** Gemini API

### Scenario 2.4: Highest/Lowest Score

**User Question:** "Học viên nào có điểm cao nhất?"
**Expected Answer:** Gemini will find the highest score in teacher's classes from the allowed data.
**Fallback:** Gemini API

### Scenario 2.5: Attempt to Query Other Classes

**User Question:** "Điểm số của lớp C1 như thế nào?"
**Expected Answer:** "Bạn chỉ có quyền xem dữ liệu lớp do mình phụ trách. Lớp C1 không nằm trong phạm vi này."
**Fallback:** Gemini API (role constraint in prompt)

---

## Role: Học viên (Student)

Vai trò này có quyền xem: Tổng quan, Lớp của tôi, Lịch học, Điểm danh, Điểm số.

### Scenario 3.1: Student's Class Information

**User Question:** "Tôi học lớp nào?"
**Expected Answer:** Gemini will extract the student's class information from the allowed personal data.
**Fallback:** Gemini API

### Scenario 3.2: Student's Schedule

**User Question:** "Lịch học của tôi là gì?"
**Expected Answer:** Gemini will extract class schedule including NgayHoc (e.g., T2,T4,T6), GioBatDau, GioKetThuc, PhongHoc.
**Fallback:** Gemini API

### Scenario 3.3: Student's Scores

**User Question:** "Điểm số của tôi là bao nhiêu?"
**Expected Answer:** Gemini will list KT1, KT2, KT3, DiemKiemTraLon, DiemTrungBinh, KetQua from allowed personal data.
**Fallback:** Gemini API

### Scenario 3.4: Student's Attendance

**User Question:** "Tôi vắng mấy buổi?"
**Expected Answer:** Gemini will count absences from DiemDanh records in allowed personal data.
**Fallback:** Gemini API

### Scenario 3.5: Exercise Request

**User Question:** "Tạo bài luyện tập tiếng Anh cho tôi"
**Expected Answer:** Chatbot will generate an exercise matching the student's course language level.
**Handler:** Exercise generation instructions in HocVien role prompt

### Scenario 3.6: Attempt to Query Other Students

**User Question:** "Điểm của Nguyễn Văn A là gì?"
**Expected Answer:** "Bạn chỉ có quyền xem dữ liệu của chính mình. Tôi không thể cung cấp thông tin học viên khác."
**Fallback:** Gemini API (role constraint in prompt)

### Scenario 3.7: Attempt to Query Tuition

**User Question:** "Học phí của tôi là bao nhiêu?"
**Expected Answer:** Gemini may provide this from allowed data if it includes tuition info, or explain the constraint.
**Fallback:** Gemini API

---

## Role: Tư vấn viên (Advisor)

Vai trò này có quyền xem: Tổng quan, Khóa học, Lớp học, Học viên (tổng quan), Tư vấn chương trình.

### Scenario 4.1: Course Information

**User Question:** "Có những khóa học nào?"
**Expected Answer:** Gemini will list available courses with NgonNgu, SoBuoi, Gia, MoTa from allowed data.
**Fallback:** Gemini API

### Scenario 4.2: Available Classes for a Language

**User Question:** "Có bao nhiêu lớp tiếng Anh?"
**Expected Answer:** Gemini will count active English classes from the allowed data.
**Fallback:** Gemini API

### Scenario 4.3: Classes on Specific Days

**User Question:** "Có lớp tiếng Anh nào học vào T2?"
**Expected Answer:** Gemini or direct handler will find English classes on Monday.
**Handler:** `TryHandleScheduleClassQueryAsync`

### Scenario 4.4: Advisor Program Suggestion (Gemini)

**User Question:** "Tôi muốn học tiếng Anh trung cấp, lớp nào phù hợp?"
**Expected Answer:** Gemini will suggest classes based on available data, considering NgonNgu and class details.
**Fallback:** Gemini API

### Scenario 4.5: Attempt to Query Student Scores

**User Question:** "Điểm số của học viên X là bao nhiêu?"
**Expected Answer:** "Tôi không có quyền xem điểm số riêng của học viên trừ khi cần thiết cho tư vấn."
**Fallback:** Gemini API (role constraint in prompt)

### Scenario 4.6: Attempt to Query Tuition Details

**User Question:** "Học phí của học viên X là bao nhiêu?"
**Expected Answer:** "Tôi không thể cung cấp thông tin học phí cá nhân của học viên."
**Fallback:** Gemini API (role constraint in prompt)

---

## Common Testing Patterns

### Pattern A: Number Extraction

Questions that require counting and listing specific items:

- "Bao nhiêu X?" → Must return actual count + breakdown
- "Ai đạt / không đạt / chưa đóng?" → Must return names + reasons
- Direct handler should fire before Gemini

### Pattern B: Schedule Extraction

Questions about when/where classes meet:

- "Lớp tiếng Anh nào học vào thứ 2,4,6?" → Must parse day codes and return class schedule
- Direct handler for language + day combo
- Format: "Có X lớp... Danh sách: ..."

### Pattern C: Role Boundary Test

Questions outside the role's scope:

- Management asking about personal scores? → Should fail
- Teacher asking about other teachers' classes? → Should fail
- Student asking about other students? → Should fail
- Advisor asking about student tuition? → Should fail
- Prompt constraint + Gemini fallback

### Pattern D: Complex Semantic Queries

Questions that need AI reasoning:

- "Tổng quan?" → Gemini uses full allowed data payload
- "So sánh..." → Gemini compares from the data
- "Gợi ý..." → Gemini suggests based on the data

---

## Test Execution Checklist

- [ ] Management: Test all 4 direct handlers (count, unpaid, pass/fail, schedule)
- [ ] Management: Test complex query to Gemini
- [ ] Teacher: Verify can only see own classes
- [ ] Teacher: Test Gemini boundary when asking about other classes
- [ ] Student: Verify personal data only
- [ ] Student: Test exercise generation request
- [ ] Advisor: Test schedule queries
- [ ] Advisor: Verify no access to scores/tuition
- [ ] All roles: Verify Vietnamese output format
- [ ] All roles: Verify no generic "không có dữ liệu" when data exists

---

## Build & Deployment Verification

**Last Build Date:** 2026-08-30
**Build Status:** ✅ Succeeded
**Handlers Added:**

1. `TryHandleFinanceSummaryAsync` - Unpaid tuition
2. `TryHandleScheduleClassQueryAsync` - Schedule by language + day
3. `TryHandleStudentCountQueryAsync` - Student count
4. `TryHandlePassingStudentsQueryAsync` - Pass/fail breakdown
5. `TryHandleClassCountQueryAsync` - Class count by language

**Next Steps:**

1. Deploy to test environment
2. Execute test scenarios with real data
3. Adjust prompt if needed based on real test results
4. Monitor Gemini API quota usage
5. Collect user feedback on response quality
