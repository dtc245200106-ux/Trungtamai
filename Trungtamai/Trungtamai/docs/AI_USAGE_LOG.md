# AI Usage Log & Documentation

**Dự Án**: Trungtamai - Hệ Thống Quản Lý Trung Tâm Ngoại Ngữ  
**Ngôn Ngữ AI**: Gemini 2.5 Flash  
**Ngày Bắt Đầu**: 2026-08-01  
**Trạng Thái**: ✅ Hoạt Động

---

## 📋 Tóm Tắt Sử Dụng AI

### Thống Kê Chung

- **Tổng Module Sử Dụng AI**: 4
- **Tổng Files Được Hỗ Trợ**: 4
- **Phương Pháp**: AI tạo code skeleton → Sinh viên review & tùy chỉnh

---

## 🔧 Các Module Sử Dụng AI

### 1. **GeminiService.cs**

**Mục Đích**: Gọi Gemini API để xử lý câu hỏi chatbot

**Prompt AI Gốc**:

```
Tạo một service C# để gọi Gemini API với async/await.
Service sẽ nhận message và trả về response từ AI.
```

**Code Được Tạo**:

```csharp
public class GeminiService
{
    private readonly Client _client;
    private readonly string _model;

    public GeminiService(IConfiguration configuration)
    {
        // Setup API client
    }

    public async Task<string> AskAsync(string message)
    {
        var response = await _client.Models.GenerateContentAsync(...);
        return response.Text;
    }
}
```

**Phần Sinh Viên Đã Review & Sửa**:

- ✅ Thêm logging (AILoggingService)
- ✅ Thêm exception handling
- ✅ Truyền role & userId cho logging
- ✅ Thêm comment "AI-generated & reviewed"

**Trạng Thái**: ✅ Hoàn Thành

---

### 2. **ChatbotDataService.cs**

**Mục Đích**: Xử lý các câu hỏi chatbot cụ thể (học phí, lịch học, điểm số, v.v.)

**Prompt AI Gốc**:

```
Tạo các method để trích xuất và xử lý các câu hỏi về:
- Học phí chưa đóng
- Lịch học
- Điểm số
- Số lượng học viên
- Số lượng lớp

Mỗi method kiểm tra câu hỏi có chứa keyword nào không,
rồi trả về dữ liệu hoặc null.
```

**Code Được Tạo**:

```csharp
public async Task<string?> TryHandleFinanceSummaryAsync(string vaiTro, string question)
{
    // Kiểm tra keyword về học phí
    // Trả về summary hoặc null
}

public async Task<string?> TryHandleScheduleClassQueryAsync(string vaiTro, string question)
{
    // Kiểm tra keyword về lịch học
    // Trả về lịch hoặc null
}
```

**Phần Sinh Viên Đã Review & Sửa**:

- ✅ Kiểm tra logic các keyword
- ✅ Thêm validation cho dữ liệu
- ✅ Thêm phân quyền (kiểm tra role)
- ✅ Thêm comment "AI-generated & reviewed"

**Trạng Thái**: ✅ Hoàn Thành

---

### 3. **ChatbotController.cs**

**Mục Đích**: API endpoint để xử lý chat requests

**Prompt AI Gốc**:

```
Tạo một API controller với:
- Endpoint GET /api/chatbot/history - lấy lịch sử chat
- Endpoint POST /api/chatbot - gửi câu hỏi và nhận response
- Validation input
- Error handling
```

**Code Được Tạo**:

```csharp
[ApiController]
[Route("api/chatbot")]
public class ChatbotController : ControllerBase
{
    [HttpGet("history")]
    public async Task<IActionResult> History() { }

    [HttpPost]
    public async Task<IActionResult> Chat(ChatRequest request) { }
}
```

**Phần Sinh Viên Đã Review & Sửa**:

- ✅ Thêm `[AuthorizeRole]` attribute
- ✅ Thêm logging cho mỗi request
- ✅ Thêm exception handling chi tiết
- ✅ Tối ưu error messages
- ✅ Thêm comment "AI-generated & reviewed"

**Trạng Thái**: ✅ Hoàn Thành

---

### 4. **AILoggingService.cs** (100% AI-Generated & Reviewed)

**Mục Đích**: Ghi nhật ký tất cả lần sử dụng AI

**Prompt AI Gốc**:

```
Tạo một service để:
- Ghi log mỗi prompt gửi đi
- Ghi log mỗi response nhận được
- Ghi log lỗi
- Lưu vào file JSON
- Hỗ trợ query logs theo role/date
- Thread-safe
```

**Code Được Tạo**:

```csharp
public class AILoggingService
{
    public void LogPrompt(string prompt, string role, string userId);
    public void LogResponse(string prompt, string response, string role, string userId);
    public void LogError(string prompt, string errorMessage, string role, string userId);
    public List<AILogEntry> GetAllLogs();
    public List<AILogEntry> GetLogsByRole(string role);
}

public class AILogEntry
{
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } // PROMPT, RESPONSE, ERROR
    public string Prompt { get; set; }
    public string Response { get; set; }
    public string Role { get; set; }
    public string UserId { get; set; }
    public bool Success { get; set; }
}
```

**Phần Sinh Viên Đã Review & Sửa**:

- ✅ Kiểm tra thread-safety
- ✅ Thêm error handling
- ✅ Thêm comment chi tiết

**Trạng Thái**: ✅ Hoàn Thành

---

## 📍 Vị Trí Log Files

Tất cả log files được lưu tại:

```
bin/Debug/net10.0/logs/ai/ai_usage_log.json
```

**Format**:

```json
[
  {
    "timestamp": "2025-01-17T10:30:45.123Z",
    "type": "PROMPT",
    "prompt": "Có bao nhiêu học viên chưa đóng học phí?",
    "role": "QuanLy",
    "userId": "1",
    "moduleName": "GeminiService",
    "success": true
  },
  {
    "timestamp": "2025-01-17T10:30:46.456Z",
    "type": "RESPONSE",
    "prompt": "Có bao nhiêu học viên chưa đóng học phí?",
    "response": "Hiện có 15 học viên chưa đóng học phí...",
    "role": "QuanLy",
    "userId": "1",
    "moduleName": "GeminiService",
    "success": true
  }
]
```

---

## 🔍 Cách Xem Logs

### 1. **Qua Code**

```csharp
var aiLogging = serviceProvider.GetRequiredService<AILoggingService>();
var allLogs = aiLogging.GetAllLogs();
var quanLyLogs = aiLogging.GetLogsByRole("QuanLy");
var todayLogs = aiLogging.GetLogsByDate(DateTime.Now);
```

### 2. **Qua File**

- Mở file: `bin/Debug/net10.0/logs/ai/ai_usage_log.json`
- Dùng JSON viewer hoặc text editor

---

## ✅ Checklist Minh Chứng AI

- ✅ **Nhật Ký Prompt**: AILoggingService ghi mỗi prompt
- ✅ **Phản Hồi AI**: AILoggingService ghi mỗi response
- ✅ **Comment Code**: Tất cả files có comment "AI-generated & reviewed"
- ✅ **Sinh Viên Review**: Mỗi file có ghi rõ phần nào AI tạo, phần nào sinh viên sửa
- ✅ **Documentation**: Tài liệu này chi tiết hóa tất cả

---

## 📊 Phần Sinh Viên Đã Kiểm Tra & Chỉnh Sửa

### GeminiService.cs

- **AI Tạo**: Cấu trúc service, method AskAsync cơ bản
- **Sinh Viên Sửa**:
  - Thêm AILoggingService injection
  - Thêm parameters (role, userId) cho logging
  - Thêm try-catch exception handling
  - Thêm logging calls

### ChatbotDataService.cs

- **AI Tạo**: Các handler methods (TryHandleFinanceSummaryAsync, v.v.)
- **Sinh Viên Sửa**:
  - Review logic tìm keyword
  - Thêm phân quyền
  - Tối ưu query performance
  - Thêm validation

### ChatbotController.cs

- **AI Tạo**: API structure, basic endpoints
- **Sinh Viên Sửa**:
  - Thêm `[AuthorizeRole]` attribute
  - Thêm logging call
  - Thêm error handling
  - Tối ưu response format

### AILoggingService.cs

- **AI Tạo**: 100% AI-generated
- **Sinh Viên Sửa**:
  - Review thread-safety
  - Kiểm tra JSON serialization
  - Thêm error handling
  - Test file I/O

---

## 🚀 Cách Sử Dụng Logging

### Bên Trong GeminiService

```csharp
public async Task<string> AskAsync(string message, string role, string userId)
{
    _aiLogging.LogPrompt(message, role, userId);  // Ghi log prompt

    var response = await _client.Models.GenerateContentAsync(...);

    _aiLogging.LogResponse(message, result, role, userId);  // Ghi log response

    return result;
}
```

### Bên Trong ChatbotController

```csharp
answer = await _geminiService.AskAsync(prompt, vaiTro, userId.ToString());
```

---

## 🔐 Bảo Mật

- ✅ **Prompt được ghi lại**: Để kiểm tra nội dung prompt
- ✅ **Response được ghi lại**: Để kiểm tra AI output
- ✅ **Người dùng được ghi**: role & userId
- ✅ **Timestamp chính xác**: UTC format
- ⚠️ **Note**: Nên xóa logs định kỳ để tiết kiệm dung lượng

---

## 📝 Kết Luận

**Tất cả yêu cầu #9 đã hoàn thành**:

- ✅ Có nhật ký prompt (AILoggingService.LogPrompt)
- ✅ Có phản hồi AI (AILoggingService.LogResponse)
- ✅ Phân code được hỗ trợ (Comment "AI-generated & reviewed")
- ✅ Phần sinh viên đã kiểm tra/chỉnh sửa (Tài liệu chi tiết)

**Status**: 🟢 **COMPLETED**

---

**Ngày Cập Nhật**: 2026-08-20  
**Người Cập Nhật**: GitHub Copilot + Student Review
