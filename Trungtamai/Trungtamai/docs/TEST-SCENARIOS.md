# Dữ liệu demo và kịch bản kiểm thử

## Dữ liệu demo

Khi chạy ứng dụng với môi trường `Development`, nếu tất cả các bảng ứng dụng đang trống, ứng dụng tự tạo dữ liệu demo. Nếu đã có dữ liệu ở bất kỳ bảng nào, seeder sẽ bỏ qua toàn bộ để tránh trộn hoặc ghi đè dữ liệu. Không sử dụng tài khoản bên dưới trên môi trường thật.

Mật khẩu tài khoản học viên và giáo viên là 4 số cuối số điện thoại. Tài khoản quản lý và tư vấn viên dùng mật khẩu `Demo123!`.

| Vai trò | Tài khoản |
|---|---|
| Quản lý | `admin@trungtamai.local` |
| Giáo viên tiếng Anh | `minhanh.gv@trungtamai.local` |
| Giáo viên tiếng Nhật | `sakura.gv@trungtamai.local` |
| Tư vấn viên | `tuvan@trungtamai.local` |
| Học viên | `nam.hv@trungtamai.local` |

Học viên demo và mật khẩu:

| Tài khoản | Số điện thoại demo | Mật khẩu |
|---|---|---|
| `nam.hv@trungtamai.local` | `0911111001` | `1001` |
| `han.hv@trungtamai.local` | `0911111002` | `1002` |
| `minh.hv@trungtamai.local` | `0911111003` | `1003` |
| `duong.hv@trungtamai.local` | `0911111004` | `1004` |
| `chi.hv@trungtamai.local` | `0911111005` | `1005` |

Tài khoản giáo viên cũng dùng 4 số cuối số điện thoại: `minhanh.gv@trungtamai.local` / `0001`, `sakura.gv@trungtamai.local` / `0002`.

Dữ liệu có ba khóa học, ba lớp ở các trạng thái đang học/sắp khai giảng/đã kết thúc, năm học viên với trạng thái và học phí khác nhau, điểm danh có mặt/muộn/vắng, cùng điểm đạt/không đạt/chưa thi. Lịch lớp đang học được đặt vào thứ hiện tại để hiển thị trên dashboard và trang điểm danh.

## Kịch bản kiểm thử theo trang

Chạy ứng dụng ở `Development`, đăng nhập bằng vai trò ghi trong từng nhóm. Mỗi thao tác thêm/sửa/xóa dưới đây nên thực hiện với bản ghi kiểm thử mới; không xóa dữ liệu demo nền.

### Đăng nhập và trang dùng chung

| Trang / chức năng | Bước kiểm thử | Kết quả mong đợi |
|---|---|---|
| `/Account/Login` | Đăng nhập bằng tài khoản demo quản lý, giáo viên, học viên và tư vấn viên | Mỗi vai trò chuyển tới dashboard/trang mặc định phù hợp |
| `/Account/Login` | Bỏ trống một trường; sau đó thử mật khẩu sai | Hiển thị thông báo lỗi, không tạo phiên đăng nhập |
| Phân quyền | Đăng nhập học viên rồi mở URL quản lý; thử gọi trang giáo viên bằng vai trò khác | Không hiển thị dữ liệu trái quyền; chuyển về đăng nhập hoặc bị từ chối |
| Đăng xuất | Nhấn đăng xuất rồi mở lại URL nội bộ | Phiên bị xóa; yêu cầu đăng nhập lại |
| Chatbot (các dashboard) | Gửi câu hỏi khi đã cấu hình Gemini API key; thử mở lịch sử chat | Bot trả lời và lịch sử hiển thị; nếu chưa cấu hình key, ứng dụng thông báo lỗi dịch vụ rõ ràng |

### Quản lý

| Trang | Bước kiểm thử | Kết quả mong đợi |
|---|---|---|
| `/QuanLy/Index` — tổng quan | Mở trang với dữ liệu demo | Hiển thị số học viên/giáo viên/khóa học/lớp và số liệu học phí; có lớp trong lịch theo thứ hiện tại |
| `/QuanLy/KhoaHoc` — khóa học | Xem danh sách; thêm khóa học hợp lệ; thử gửi thiếu tên/ngôn ngữ; sửa rồi xóa khóa học kiểm thử | Danh sách và thông báo cập nhật đúng; dữ liệu không hợp lệ bị từ chối |
| `/QuanLy/HocVien` — học viên | Tìm bản ghi; thêm học viên gắn lớp hợp lệ; thử email đã tồn tại và thêm vào lớp đã đủ sĩ số; sửa trạng thái/lớp rồi xóa học viên kiểm thử | Tạo mã/tài khoản, sĩ số lớp, trạng thái và dữ liệu học viên cập nhật đúng; trường hợp trùng/đầy lớp bị từ chối |
| `/QuanLy/GiaoVien` — giáo viên | Xem hai giáo viên; thêm giáo viên hợp lệ; thử email trùng; sửa rồi xóa bản ghi kiểm thử | Tài khoản giáo viên gắn email; email trùng không được tạo |
| `/QuanLy/LopHoc` — lớp học | Kiểm tra các trạng thái; tạo/sửa lớp hợp lệ; thử trùng phòng hoặc trùng lịch giáo viên; xóa lớp kiểm thử | Lớp, lịch và sĩ số hiển thị đúng; lịch trùng được báo lỗi |
| `/QuanLy/LichHoc` — lịch học | Xem lịch theo buổi sáng/chiều/tối; lọc hoặc chuyển ngày nếu giao diện hỗ trợ | Lớp đang hoạt động được hiển thị, lớp đã kết thúc không xuất hiện |
| `/QuanLy/HocPhi` — học phí | Tìm `HV001`; lọc `Chưa đủ`, `Chưa đóng`, `Đã đóng`; ghi nhận khoản thanh toán; thử số tiền bằng 0 hoặc vượt số dư | Danh sách/tổng thu/còn nợ và trạng thái thanh toán phản ánh dữ liệu; số tiền không hợp lệ bị xử lý đúng |
| `/QuanLy/KetQuaHocTap` — kết quả | Lọc theo lớp, tên/mã học viên, kết quả `Đạt`, `Không đạt`, `Chưa thi` | `HV001`, `HV002`, `HV003` lần lượt thuộc các trạng thái đạt, không đạt, chưa thi |
| `/QuanLy/ThongKe` — thống kê | Đối chiếu số lượng học viên từng trạng thái, sĩ số lớp và học phí với dữ liệu demo | Tổng số, tỷ lệ hoàn thành, doanh thu và công nợ hiển thị nhất quán |

### Giáo viên

Đăng nhập `minhanh.gv@trungtamai.local`.

| Trang | Bước kiểm thử | Kết quả mong đợi |
|---|---|---|
| `/GiaoVien/Index` — tổng quan | Mở dashboard | Chỉ thấy lớp được giao; lớp có lịch hôm nay được hiển thị |
| `/GiaoVien/LichDay` — lịch dạy | Mở danh sách lớp/lịch dạy | Hiển thị lớp của Nguyễn Minh Anh và tiến độ buổi học |
| `/GiaoVien/LopHocCuaToi` — lớp của tôi | Mở chi tiết lớp; tải danh sách điểm danh hôm nay | Thấy học viên thuộc lớp; trạng thái có mặt/muộn/vắng khớp dữ liệu mẫu |
| Điểm danh | Đổi trạng thái một học viên rồi lưu; tải lại cùng ngày | Thay đổi được lưu; giáo viên không thể điểm danh lớp không phụ trách |
| `/GiaoVien/DiemSoKetQua` — điểm và kết quả | Mở lớp; xem điểm từng học viên; lưu điểm kiểm tra | Điểm trung bình/kết quả đúng; điểm cuối khóa từ 8 trở lên đánh dấu học viên hoàn thành theo quy tắc ứng dụng |
| API giới hạn lớp | Gọi chi tiết lớp thuộc giáo viên Sakura bằng tài khoản Minh Anh | Yêu cầu bị từ chối, không lộ học viên lớp khác |

### Học viên

Đăng nhập `nam.hv@trungtamai.local`.

| Trang | Bước kiểm thử | Kết quả mong đợi |
|---|---|---|
| `/HocVien/Index` — tổng quan | Mở trang | Hiện lớp đang học, lịch hôm nay, điểm trung bình và công nợ của HV001 |
| `/HocVien/LichHoc` — lịch học | Mở trang | Lịch/giáo viên/phòng học khớp lớp được xếp |
| `/HocVien/LopCuaToi` — lớp của tôi | Mở chi tiết lớp | Lịch sử điểm danh và kết quả học tập của chính HV001 hiển thị |
| API giới hạn dữ liệu | Thử tải chi tiết lớp không thuộc học viên hoặc đăng xuất rồi gọi API | Không truy cập được dữ liệu lớp/học viên khác |
| Trạng thái không có lớp | Đăng nhập học viên demo đang tạm ngưng hoặc đã kết thúc | Trang tải ổn định, không lỗi khi lịch/lớp đang học không có dữ liệu |

### Tư vấn viên và trang công khai

Đăng nhập `tuvan@trungtamai.local`.

| Trang | Bước kiểm thử | Kết quả mong đợi |
|---|---|---|
| `/TuVanVien` | Mở trang mặc định | Chuyển hướng sang danh sách khóa học/lớp đang mở |
| `/TuVanVien/KhoaHocMoLop` | Chọn Tiếng Anh rồi Tiếng Nhật; chọn khóa học; thử khóa học không có lớp mở | Khóa học và lớp phù hợp được nạp; lớp đã kết thúc bị ẩn |
| `/Home/Index` | Mở trang công khai | Trang trả về thành công |
| `/Home/Privacy` | Mở trang chính sách | Trang trả về thành công |
| Lỗi ứng dụng | Truy cập URL lỗi trong môi trường không Development | Hiển thị trang lỗi thân thiện, không lộ stack trace |
