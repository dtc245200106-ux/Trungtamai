using Google.GenAI;

namespace Trungtamai.Services
{
    /// <summary>
    /// Service để gọi Gemini API
    /// AI-generated & reviewed: Tạo cấu trúc cơ bản, sinh viên đã review và thêm logging
    /// </summary>
    public class GeminiService
    {
        private readonly Client _client;
        private readonly string _model;
        private readonly AILoggingService _aiLogging;

        public GeminiService(IConfiguration configuration, AILoggingService aiLogging)
        {
            var apiKey = configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable(
                    "GEMINI_API_KEY");
            }

            _model = configuration["Gemini:Model"] ?? "gemini-2.5-flash";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new Exception("Chưa cấu hình Gemini API Key.");
            }

            _client = new Client(apiKey: apiKey);
            _aiLogging = aiLogging;
        }

        /// <summary>
        /// Gọi Gemini API và ghi log request/response
        /// </summary>
        public async Task<string> AskAsync(string message, string role = "Unknown", string userId = "Unknown")
        {
            try
            {
                // AI-generated & reviewed: Ghi log prompt
                _aiLogging.LogPrompt(message, role, userId);

                var response = await _client.Models.GenerateContentAsync(
                    model: _model,
                    contents: message
                );

                var result = response.Text ?? "AI không trả về nội dung.";

                // AI-generated & reviewed: Ghi log response
                _aiLogging.LogResponse(message, result, role, userId, success: true);

                return result;
            }
            catch (Exception ex)
            {
                // AI-generated & reviewed: Ghi log lỗi
                _aiLogging.LogError(message, ex.Message, role, userId);
                throw;
            }
        }
    }
}