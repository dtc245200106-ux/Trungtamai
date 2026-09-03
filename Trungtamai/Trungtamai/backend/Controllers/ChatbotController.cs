using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trungtamai.Attributes;
using Trungtamai.Data;
using Trungtamai.Services;

namespace Trungtamai.Controllers
{
    /// <summary>
    /// Chatbot API sử dụng Gemini AI
    /// AI-generated & reviewed: Cấu trúc handlers được AI tạo, sinh viên đã review logic và thêm logging
    /// </summary>
    [ApiController]
    [Route("api/chatbot")]
    [AuthorizeRole("QuanLy", "GiaoVien", "HocVien", "TuVanVien")]
    public class ChatbotController : ControllerBase
    {
        private readonly GeminiService _geminiService;
        private readonly ChatbotDataService _chatbotDataService;
        private readonly AppDbContext _context;

        public ChatbotController(
            GeminiService geminiService,
            ChatbotDataService chatbotDataService,
            AppDbContext context)
        {
            _geminiService = geminiService;
            _chatbotDataService = chatbotDataService;
            _context = context;
        }

        [HttpGet("history")]
        public async Task<IActionResult> History()
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
                return Unauthorized(new { message = "Thông tin người dùng không hợp lệ." });

            var history = await _context.ChatMessages
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.CreatedAt)
                .Select(x => new { userMessage = x.UserMessage, botResponse = x.BotResponse })
                .ToListAsync();

            return Ok(history);
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new
                {
                    message = "Vui lòng nhập câu hỏi."
                });
            }

            // Lấy thông tin người đang đăng nhập
            var userIdString = HttpContext.Session.GetString("UserId");
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            var hoTen = HttpContext.Session.GetString("HoTen");

            // Chưa đăng nhập
            if (string.IsNullOrWhiteSpace(userIdString) ||
                string.IsNullOrWhiteSpace(vaiTro))
            {
                return Unauthorized(new
                {
                    message = "Thông tin người dùng không hợp lệ."
                });
            }

            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new
                {
                    message = "Thông tin tài khoản không hợp lệ."
                });
            }

            if (!_chatbotDataService.IsSupportedRole(vaiTro))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Vai trò tài khoản không được phép sử dụng chatbot."
                });            }

            try
            {
                var directFinanceAnswer = await _chatbotDataService.TryHandleFinanceSummaryAsync(vaiTro, request.Message);
                if (!string.IsNullOrWhiteSpace(directFinanceAnswer))
                {
                    _context.ChatMessages.Add(new Models.ChatMessage
                    {
                        UserId = userId,
                        UserMessage = request.Message.Trim(),
                        BotResponse = directFinanceAnswer
                    });
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        answer = directFinanceAnswer,
                        vaiTro = vaiTro
                    });
                }

                var directScheduleAnswer = await _chatbotDataService.TryHandleScheduleClassQueryAsync(vaiTro, request.Message);
                if (!string.IsNullOrWhiteSpace(directScheduleAnswer))
                {
                    _context.ChatMessages.Add(new Models.ChatMessage
                    {
                        UserId = userId,
                        UserMessage = request.Message.Trim(),
                        BotResponse = directScheduleAnswer
                    });
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        answer = directScheduleAnswer,
                        vaiTro = vaiTro
                    });
                }

                var directStudentCountAnswer = await _chatbotDataService.TryHandleStudentCountQueryAsync(vaiTro, request.Message);
                if (!string.IsNullOrWhiteSpace(directStudentCountAnswer))
                {
                    _context.ChatMessages.Add(new Models.ChatMessage
                    {
                        UserId = userId,
                        UserMessage = request.Message.Trim(),
                        BotResponse = directStudentCountAnswer
                    });
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        answer = directStudentCountAnswer,
                        vaiTro = vaiTro
                    });
                }

                var directPassingAnswer = await _chatbotDataService.TryHandlePassingStudentsQueryAsync(vaiTro, request.Message);
                if (!string.IsNullOrWhiteSpace(directPassingAnswer))
                {
                    _context.ChatMessages.Add(new Models.ChatMessage
                    {
                        UserId = userId,
                        UserMessage = request.Message.Trim(),
                        BotResponse = directPassingAnswer
                    });
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        answer = directPassingAnswer,
                        vaiTro = vaiTro
                    });
                }

                var directClassCountAnswer = await _chatbotDataService.TryHandleClassCountQueryAsync(vaiTro, request.Message);
                if (!string.IsNullOrWhiteSpace(directClassCountAnswer))
                {
                    _context.ChatMessages.Add(new Models.ChatMessage
                    {
                        UserId = userId,
                        UserMessage = request.Message.Trim(),
                        BotResponse = directClassCountAnswer
                    });
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        answer = directClassCountAnswer,
                        vaiTro = vaiTro
                    });
                }

                // Lấy đúng dữ liệu mà vai trò này được phép xem
                var allowedData =
                    await _chatbotDataService.GetAllowedDataAsync(
                        userId,
                        vaiTro);

                // Phân loại câu hỏi để知道 nên xử lý theo chủ đề nào trước khi gọi AI.
                // Ví dụ: học phí, lịch học, điểm số, danh sách học viên, bài luyện tập.
                var questionTopic = ClassifyQuestionTopic(request.Message);
                var topicHint = questionTopic switch
                {
                    "finance" => "Câu hỏi thuộc chủ đề tài chính/học phí: ưu tiên dùng số tiền, trạng thái đóng học phí, còn nợ, tổng số người chưa đóng.",
                    "schedule" => "Câu hỏi thuộc chủ đề lịch học: ưu tiên xem NgayHoc, GioBatDau, GioKetThuc, PhongHoc, TenLop, GiaoVien.",
                    "student_count" => "Câu hỏi thuộc chủ đề thống kê học viên: ưu tiên đếm số lượng và tổng hợp theo trạng thái.",
                    "student_list" => "Câu hỏi thuộc chủ đề danh sách học viên: ưu tiên liệt kê tên và mã thật có trong dữ liệu.",
                    "score" => "Câu hỏi thuộc chủ đề điểm số: ưu tiên dùng DiemKT1, DiemKT2, DiemKT3, DiemKiemTraLon, DiemTrungBinh, KetQua.",
                    "attendance" => "Câu hỏi thuộc chủ đề điểm danh: ưu tiên dùng dữ liệu DiemDanh và trạng thái vắng, có mặt.",
                    "class_count" => "Câu hỏi thuộc chủ đề thống kê lớp: ưu tiên đếm số lớp theo ngôn ngữ và trạng thái hoạt động.",
                    "exercise" => "Câu hỏi thuộc chủ đề bài luyện tập: ưu tiên tạo bài tập phù hợp với khóa học, lớp và ngôn ngữ của học viên.",
                    "general" => "Câu hỏi tổng quan: ưu tiên tóm tắt đúng dữ liệu được phép xem theo vai trò.",
                    _ => "Câu hỏi chưa xác định rõ chủ đề: hãy trả lời dựa trên dữ liệu được phép và không suy đoán."
                };

                var today = DateTime.Today;
                var currentDate = today.ToString("dd/MM/yyyy");
                var tomorrow = today.AddDays(1);
                var tomorrowDate = tomorrow.ToString("dd/MM/yyyy");
                var currentWeekday = GetVietnameseWeekday(today.DayOfWeek);
                var tomorrowWeekday = GetVietnameseWeekday(tomorrow.DayOfWeek);

                // Tạo prompt cho Gemini
                var exerciseInstructions = vaiTro == "HocVien"
                     ? """
                        8. Nếu học viên yêu cầu bài luyện tập, hãy tự tạo bài phù hợp
                           với khóa học, lớp và ngôn ngữ của chính học viên.
                           Phải hỗ trợ mọi ngôn ngữ có trong dữ liệu, không chỉ tiếng Anh,
                           ví dụ tiếng Trung, Nhật, Hàn, Pháp hoặc ngôn ngữ khác.
                           Nội dung bài tập, câu hỏi và đáp án phải viết bằng ngôn ngữ
                           học viên yêu cầu hoặc ngôn ngữ của khóa học.
                        9. Nếu học viên nêu trình độ cụ thể như A1, A2, B1, B2,
                           cơ bản, trung cấp hoặc nâng cao, phải ưu tiên đúng trình độ đó.
                        10. Nếu học viên yêu cầu bài phù hợp với trình độ của mình,
                            hãy ước lượng thận trọng từ khóa học, lớp và kết quả học tập
                            được cung cấp; không khẳng định đây là trình độ chính thức.
                        11. Có thể tạo nhiều dạng bài: trắc nghiệm, tự luận, viết đoạn văn,
                            dịch, trả lời câu hỏi mở hoặc bài luyện nói theo yêu cầu.
                            Với bài tự luận, hãy nêu đề bài, yêu cầu độ dài và tiêu chí chấm.
                        12. Bài luyện tập cần có đề bài rõ ràng, số lượng câu hỏi phù hợp
                            và không tiết lộ dữ liệu của học viên khác.
                        13. Chỉ đưa đáp án, bài mẫu, gợi ý hoặc lời giải khi học viên yêu cầu.
                        14. Nếu học viên yêu cầu "tạo bài tập" hoặc "làm bài tập" hoặc "soạn bài tập"
                            hoặc bất kỳ câu hỏi nào có ý định sinh bài tập, hãy luôn trả lời theo
                            cấu trúc dạng markdown dễ đọc, không viết dạng đoạn văn dài:
                            ### Bài tập: [Tên bài]
                            ### Mục tiêu
                            - ...
                            ### Thời lượng
                            - ...
                            ### Đề bài
                            1. ...
                            2. ...
                            ### Yêu cầu
                            - ...
                            ### Đáp án gợi ý
                            - ...
                            ### Gợi ý chấm điểm
                            - ...
                        15. Mỗi phần phải nằm trên dòng riêng, rõ ràng; tránh gộp nhiều mục vào một đoạn.
                        16. Khi có thể, dùng danh sách gạch đầu dòng hoặc số thứ tự để dễ đọc trên UI.
                        17. Không dùng chữ “mơ hồ”, “giả định” hoặc “chung chung” trong phần bài tập.
                        """
                     : "";

            var rolePages = vaiTro switch
            {
                "QuanLy" => "Tổng quan, Học viên, Giáo viên, Khóa học, Lớp học, Lịch học, Học phí, Kết quả học tập, Thống kê",
                "GiaoVien" => "Tổng quan, Lớp của tôi, Lịch dạy, Điểm số, Kết quả học tập",
                "HocVien" => "Tổng quan, Lớp của tôi, Lịch học, Điểm danh, Điểm số",
                "TuVanVien" => "Tổng quan, Khóa học, Lớp học, Học viên, Tư vấn chương trình",
                _ => "Trang cơ bản"
            };

            var rolePolicy = vaiTro switch
            {
                "QuanLy" => "Bạn là trợ lý cho trang Quản lý. Bạn được phép xem tổng quan trung tâm, học viên, giáo viên, khóa học, lớp học, lịch học, học phí, kết quả học tập và thống kê. Không được tiết lộ dữ liệu của người dùng ngoài quyền truy cập được phép.",
                "GiaoVien" => "Bạn là trợ lý cho trang Giáo viên. Bạn chỉ được phép xem thông tin lớp do mình phụ trách, lịch dạy, học viên trong lớp, điểm số và kết quả học tập của lớp mình. Không được xem dữ liệu của giáo viên khác hoặc dữ liệu của các lớp không được giao.",
                "HocVien" => "Bạn là trợ lý cho trang Học viên. Bạn chỉ được phép xem thông tin cá nhân của học viên đang đăng nhập, lớp học của mình, lịch học, điểm danh, điểm số của chính mình. Không được xem dữ liệu học viên khác.",
                "TuVanVien" => "Bạn là trợ lý cho trang Tư vấn viên. Bạn được phép xem khóa học, lớp học, thông tin tổng quan và dữ liệu tư vấn, nhưng không được xem học phí cá nhân hoặc điểm số riêng tư nếu không nằm trong phạm vi tư vấn hợp lệ.",
                _ => "Bạn là trợ lý hỗ trợ hệ thống."
            };

            var pageRules = vaiTro switch
            {
                "QuanLy" => "Trang Quản lý: Tổng quan trung tâm, danh sách học viên, giáo viên, khóa học, lớp học, lịch học, học phí, điểm số, thống kê. Khi ai hỏi 'bao nhiêu', 'ai', 'liệt kê', 'điểm', 'nợ', 'đóng', 'thống kê', hãy trả lời bằng dữ liệu thực trong hệ thống. Nếu câu hỏi yêu cầu danh sách tên, bạn phải liệt kê tên học viên hoặc giáo viên có trong dữ liệu, không được trả lời chung chung.",
                "GiaoVien" => "Trang Giáo viên: Chỉ trả lời về lớp được phân công, học viên trong lớp, lịch dạy, điểm số. Nếu người dùng hỏi về giáo viên khác hoặc lớp khác, hãy nói rõ rằng bạn chỉ có quyền xem lớp đang phụ trách.",
                "HocVien" => "Trang Học viên: Chỉ trả lời về thông tin cá nhân, lớp của mình, lịch học, điểm danh, điểm số cá nhân. Nếu hỏi về học viên khác, hãy từ chối và giải thích bạn chỉ có quyền xem dữ liệu của chính mình.",
                "TuVanVien" => "Trang Tư vấn viên: Chỉ trả lời về khóa học, lớp học, thông tin chương trình, xu hướng học tập và hỗ trợ tư vấn. Không được trả lời dữ liệu nhạy cảm cá nhân như học phí riêng hoặc điểm số riêng của học viên nếu không cần thiết cho tư vấn.",
                _ => "Bạn chỉ trả lời theo dữ liệu hệ thống được phép xem."
            };

            var roleInstructions = vaiTro switch
            {
                "QuanLy" => "Bạn phải ưu tiên trả lời theo các dữ liệu: tổng quan, học viên, giáo viên, khóa học, lớp học, lịch học, học phí, điểm số và thống kê. Khi người dùng hỏi 'có bao nhiêu', 'ai chưa', 'liệt kê', 'nợ', 'đóng', 'đạt hay không', hãy trả về con số và danh sách tên nếu có trong dữ liệu.",
                "GiaoVien" => "Bạn phải ưu tiên trả lời theo các dữ liệu: lớp của tôi, lịch dạy, học viên trong lớp, điểm số và kết quả học tập. Nếu người dùng hỏi về dữ liệu khác, hãy trả lời rằng bạn chỉ có quyền xem lớp do mình phụ trách.",
                "HocVien" => "Bạn phải ưu tiên trả lời theo dữ liệu cá nhân: lớp của tôi, lịch học, điểm danh, điểm số của mình. Nếu câu hỏi về học viên khác, trả lời rằng bạn chỉ có quyền xem thông tin của bản thân.",
                "TuVanVien" => "Bạn phải ưu tiên trả lời theo các dữ liệu: khóa học, lớp học, lịch học theo ngày, ngôn ngữ, chương trình học và tư vấn. Nếu hỏi về riêng một học viên hoặc học phí riêng, chỉ trả lời khi dữ liệu cần thiết cho tư vấn và thuộc phạm vi được phép.",
                _ => "Bạn phải trả lời dựa trên dữ liệu được phép xem."
            };

            var prompt = $"""
            Bạn là chatbot chính thức của Trung tâm LinguistAI.

            Thông tin người dùng:
            - Họ tên: {hoTen}
            - UserId: {userId}
            - Vai trò: {vaiTro}
            - Các trang được phép truy cập: {rolePages}

            Chính sách quyền truy cập:
            {rolePolicy}

            Quy định theo trang:
            {pageRules}

            Hướng dẫn theo vai trò:
            {roleInstructions}

            Chủ đề câu hỏi được phân loại:
            - Chủ đề: {questionTopic}
            - Gợi ý xử lý: {topicHint}

            Nhiệm vụ:
            - Trả lời câu hỏi dựa hoàn toàn trên dữ liệu được cung cấp trong phần DỮ LIỆU ĐƯỢC PHÉP TRUY CẬP.
            - Nếu hỏi về số lượng, số phần trăm, tổng, điểm, doanh thu, học phí, lịch học, điểm danh, điểm số, hãy tính từ dữ liệu thực có trong nguồn.
            - Nếu hỏi “ai”, “ai là”, “liệt kê”, “bao nhiêu người”, hãy trả về tên hoặc danh sách tên có trong dữ liệu, không được nói chung chung.
            - Nếu có dữ liệu đầy đủ, trả lời trực tiếp, ngắn gọn, rõ ràng, theo tiếng Việt.
            - Nếu không có dữ liệu phù hợp, trả lời đúng định dạng: “Hệ thống chưa có dữ liệu phù hợp cho câu hỏi này.”

            Quy tắc bắt buộc:
            1. Chỉ dùng dữ liệu từ phần DỮ LIỆU ĐƯỢC PHÉP TRUY CẬP bên dưới.
            2. Không bịa dữ liệu, không suy đoán, không thêm tên, lớp, hoặc số liệu không có trong dữ liệu.
            3. Không tiết lộ dữ liệu không thuộc phạm vi quyền truy cập của vai trò hiện tại.
            4. Nếu người dùng hỏi về danh sách, hãy liệt kê tên hoặc mã nếu dữ liệu có sẵn.
            5. Nếu câu hỏi có yếu tố “bao nhiêu”, hãy đánh số lượng thật từ dữ liệu; nếu “ai”, hãy liệt kê tên thật từ dữ liệu.
            6. Nếu hỏi về điểm số, ưu tiên trả lời theo: KT1, KT2, KT3, điểm kiểm tra lớn, điểm trung bình, kết quả.
            7. Nếu hỏi về học phí, ưu tiên dựa trên: SoTienPhaiDong, SoTienDaDong, ConNo.
            8. Nếu hỏi về lịch học hoặc lịch dạy, ưu tiên theo: NgayHoc, GioBatDau, GioKetThuc, PhongHoc, CaHoc, TenLop, GiaoVien.
            9. Nếu hỏi về thống kê tổng quan, tính toán từ số liệu thực có trong nguồn dữ liệu.
            10. Nếu người dùng hỏi về hôm nay hoặc ngày mai, hãy dùng ngày hiện tại và thứ trong tuần được cung cấp để đối chiếu với NgayHoc.
            11. Hôm nay là {currentDate} ({currentWeekday}); ngày mai là {tomorrowDate} ({tomorrowWeekday}).
            12. NgayHoc dùng mã: T2, T3, T4, T5, T6, T7, CN.
            13. Không trả lời kiểu mơ hồ như “không có dữ liệu phù hợp” nếu dữ liệu thực tế đã có trong nguồn và có thể trả lời được.
            14. Không dùng khẩu ngữ mơ hồ, không lảng tránh, không chống đối người dùng.
            15. Nếu câu hỏi thuộc về dữ liệu có danh sách, hãy trả lời theo cú pháp rõ ràng: “Có X kết quả. Danh sách: ...” hoặc “Hiện có X ... . Danh sách: ...”
            {exerciseInstructions}

            DỮ LIỆU ĐƯỢC PHÉP TRUY CẬP:
            {allowedData}

            CÂU HỎI CỦA NGƯỜI DÙNG:
            {request.Message}

            YÊU CẦU ĐẦU RA:
            - Viết bằng tiếng Việt.
            - Gọn gàng, rõ ràng, có số liệu nếu dữ liệu đủ.
            - Nếu là câu hỏi có danh sách, phải liệt kê tên hoặc mã học viên/khoá/lớp có trong dữ liệu.
            - Nếu là câu hỏi về học phí chưa đóng, trả lời theo định dạng: “Hiện có X học viên chưa đóng học phí. Danh sách: ...”
            - Nếu người dùng yêu cầu tạo bài tập, hãy trả lời theo mẫu rõ ràng và dễ đọc, theo dạng markdown sau:
              ### Bài tập: [Tên bài]
              ### Mục tiêu
              - ...
              ### Thời lượng
              - ...
              ### Đề bài
              1. ...
              2. ...
              ### Yêu cầu
              - ...
              ### Đáp án gợi ý
              - ...
              ### Gợi ý chấm điểm
              - ...
            - Mỗi phần phải nằm trên dòng riêng, không gộp thành một khối văn dài. Dùng heading + bullet list/numbered list để dễ đọc trên UI.
            - Nếu là câu hỏi có dữ liệu đủ, trả lời trực tiếp; nếu thiếu dữ liệu, nói rõ và không bịa.
            """;

            string answer;
            try
            {
                // AI-generated & reviewed: Gọi Gemini API với logging
                answer = await _geminiService.AskAsync(prompt, vaiTro, userId.ToString());
            }
            catch (Exception ex) when (ex.Message.Contains("quota", StringComparison.OrdinalIgnoreCase))
            {
                answer = await _chatbotDataService.GetTeacherScoreComparisonAsync(userId, request.Message)
                    ?? "AI đang tạm hết hạn mức sử dụng. Vui lòng thử lại sau hoặc kiểm tra gói Gemini API.";
            }

            try
            {
                _context.ChatMessages.Add(new Models.ChatMessage
                {
                    UserId = userId,
                    UserMessage = request.Message.Trim(),
                    BotResponse = answer
                });
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Lỗi lưu lịch sử chatbot: {ex}");
            }

                return Ok(new
                {
                    answer = answer,
                    vaiTro = vaiTro
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Lỗi chatbot: {ex}");
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    message = "Chatbot đang gặp lỗi khi truy vấn dữ liệu hoặc kết nối AI. Vui lòng thử lại sau."
                });
            }
        }

        // Hàm này dùng để gán câu hỏi vào một chủ đề nhất định để chọn flow xử lý phù hợp.
        // Ví dụ: "bao nhiêu học viên" => student_count, "lịch học thứ 2" => schedule.
        private static string ClassifyQuestionTopic(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return "general";

            var lower = question.Trim().ToLowerInvariant();

            // Chủ đề tài chính / học phí: nợ, đóng học phí, Thu/Chi liên quan học phí.
            if (lower.Contains("học phí") || lower.Contains("hoc phi") || lower.Contains("đóng") || lower.Contains("dong") || lower.Contains("nợ") || lower.Contains("no"))
                return "finance";

            if (lower.Contains("lịch học") || lower.Contains("lich hoc") || lower.Contains("thứ") || lower.Contains("thu") || lower.Contains("ngày") || lower.Contains("ngay") || lower.Contains("học vào") || lower.Contains("hoc vao"))
                return "schedule";

            if (lower.Contains("bao nhiêu học viên") || lower.Contains("bao nhieu hoc vien") || lower.Contains("tổng số học viên") || lower.Contains("tong so hoc vien") || lower.Contains("có bao nhiêu") || lower.Contains("co bao nhieu"))
                return "student_count";

            if (lower.Contains("ai") || lower.Contains("học viên nào") || lower.Contains("hoc vien nao") || lower.Contains("liệt kê") || lower.Contains("liet ke"))
                return "student_list";

            if (lower.Contains("tạo bài tập") || lower.Contains("tao bai tap") || lower.Contains("bài tập") || lower.Contains("bai tap") || lower.Contains("làm bài tập") || lower.Contains("lam bai tap") || lower.Contains("exercise") || lower.Contains("worksheet") || lower.Contains("đề bài") || lower.Contains("de bai"))
                return "exercise";

            if (lower.Contains("điểm") || lower.Contains("diem") || lower.Contains("đạt") || lower.Contains("dat") || lower.Contains("không đạt") || lower.Contains("khong dat"))
                return "score";

            if (lower.Contains("điểm danh") || lower.Contains("diem danh") || lower.Contains("vắng") || lower.Contains("vang") || lower.Contains("có mặt") || lower.Contains("co mat"))
                return "attendance";

            if (lower.Contains("bao nhiêu lớp") || lower.Contains("bao nhieu lop") || lower.Contains("có bao nhiêu lớp") || lower.Contains("co bao nhieu lop"))
                return "class_count";

            if (lower.Contains("bài luyện tập") || lower.Contains("bai luyen tap") || lower.Contains("luyện tập") || lower.Contains("luyen tap") || lower.Contains("exercise") || lower.Contains("bài tập") || lower.Contains("bai tap"))
                return "exercise";

            return "general";
        }

        private static string GetVietnameseWeekday(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => "Thứ 2 (T2)",
                DayOfWeek.Tuesday => "Thứ 3 (T3)",
                DayOfWeek.Wednesday => "Thứ 4 (T4)",
                DayOfWeek.Thursday => "Thứ 5 (T5)",
                DayOfWeek.Friday => "Thứ 6 (T6)",
                DayOfWeek.Saturday => "Thứ 7 (T7)",
                _ => "Chủ nhật (CN)"
            };
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = "";
    }
}