using System;
using System.IO;
using System.Text.Json;

namespace Trungtamai.Services
{
    /// <summary>
    /// Service để ghi nhật ký tất cả lần sử dụng AI (Gemini)
    /// Mục đích: Minh chứng sử dụng AI, prompt, response, và phần sinh viên đã review/sửa
    /// </summary>
    public class AILoggingService
    {
        private readonly string _logFilePath;
        private readonly object _lockObject = new object();

        public AILoggingService(IConfiguration configuration)
        {
            var logsDirectory = Path.Combine(AppContext.BaseDirectory, "logs", "ai");
            Directory.CreateDirectory(logsDirectory);
            _logFilePath = Path.Combine(logsDirectory, "ai_usage_log.json");
        }

        /// <summary>
        /// Ghi log khi gọi Gemini API
        /// </summary>
        public void LogPrompt(string prompt, string role, string userId)
        {
            try
            {
                lock (_lockObject)
                {
                    var logEntry = new AILogEntry
                    {
                        Timestamp = DateTime.UtcNow,
                        Type = "PROMPT",
                        Prompt = prompt,
                        Role = role,
                        UserId = userId,
                        ModuleName = "GeminiService"
                    };

                    AppendLogEntry(logEntry);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Lỗi ghi log Prompt: {ex}");
            }
        }

        /// <summary>
        /// Ghi log khi nhận response từ Gemini API
        /// </summary>
        public void LogResponse(string prompt, string response, string role, string userId, bool success = true)
        {
            try
            {
                lock (_lockObject)
                {
                    var logEntry = new AILogEntry
                    {
                        Timestamp = DateTime.UtcNow,
                        Type = "RESPONSE",
                        Prompt = prompt,
                        Response = response,
                        Role = role,
                        UserId = userId,
                        Success = success,
                        ModuleName = "GeminiService"
                    };

                    AppendLogEntry(logEntry);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Lỗi ghi log Response: {ex}");
            }
        }

        /// <summary>
        /// Ghi log khi có lỗi từ AI
        /// </summary>
        public void LogError(string prompt, string errorMessage, string role, string userId)
        {
            try
            {
                lock (_lockObject)
                {
                    var logEntry = new AILogEntry
                    {
                        Timestamp = DateTime.UtcNow,
                        Type = "ERROR",
                        Prompt = prompt,
                        Response = errorMessage,
                        Role = role,
                        UserId = userId,
                        Success = false,
                        ModuleName = "GeminiService"
                    };

                    AppendLogEntry(logEntry);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Lỗi ghi log Error: {ex}");
            }
        }

        /// <summary>
        /// Lưu log entry vào file JSON
        /// </summary>
        private void AppendLogEntry(AILogEntry entry)
        {
            try
            {
                var json = JsonSerializer.Serialize(entry, new JsonSerializerOptions { WriteIndented = true });
                
                // Append vào file
                if (!File.Exists(_logFilePath))
                {
                    File.WriteAllText(_logFilePath, "[\n");
                }

                // Đọc file hiện tại, xóa dấu "]" cuối cùng, thêm entry mới
                var content = File.ReadAllText(_logFilePath);
                if (content.TrimEnd().EndsWith("]"))
                {
                    content = content.TrimEnd();
                    content = content.Substring(0, content.Length - 1); // Xóa ]
                    content += ",\n  " + json + "\n]";
                }
                else
                {
                    content += "  " + json + "\n]";
                }

                File.WriteAllText(_logFilePath, content);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Lỗi lưu AI log: {ex}");
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả log entries
        /// </summary>
        public List<AILogEntry> GetAllLogs()
        {
            try
            {
                if (!File.Exists(_logFilePath))
                    return new List<AILogEntry>();

                var json = File.ReadAllText(_logFilePath);
                var logs = JsonSerializer.Deserialize<List<AILogEntry>>(json) ?? new List<AILogEntry>();
                return logs.OrderByDescending(x => x.Timestamp).ToList();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Lỗi đọc AI logs: {ex}");
                return new List<AILogEntry>();
            }
        }

        /// <summary>
        /// Lấy log entries theo vai trò
        /// </summary>
        public List<AILogEntry> GetLogsByRole(string role)
        {
            var allLogs = GetAllLogs();
            return allLogs.Where(x => x.Role == role).ToList();
        }

        /// <summary>
        /// Lấy log entries theo ngày
        /// </summary>
        public List<AILogEntry> GetLogsByDate(DateTime date)
        {
            var allLogs = GetAllLogs();
            return allLogs.Where(x => x.Timestamp.Date == date.Date).ToList();
        }
    }

    /// <summary>
    /// Model đại diện cho một log entry
    /// </summary>
    public class AILogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = string.Empty; // PROMPT, RESPONSE, ERROR
        public string Prompt { get; set; } = string.Empty;
        public string? Response { get; set; }
        public string Role { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string ModuleName { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {Type} - Role: {Role} - Module: {ModuleName}";
        }
    }
}
