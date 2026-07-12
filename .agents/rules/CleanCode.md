---
trigger: always_on
---

# NGUYÊN TẮC CODE & CLEAN CODE
* **Hệ thống nguyên tắc SOLID**
* **Single Responsibility (Đơn nhiệm):** Một class chỉ giữ một trách nhiệm duy nhất. Đừng viết một script Player.cs ôm đồm từ di chuyển, máu, âm thanh đến kết nối mạng. Hãy chia nhỏ thành PlayerMovement.cs, PlayerHealth.cs, và PlayerNetworkHandler.cs.
* **Open/Closed (Mở nhưng Đóng):** Thoải mái mở rộng (thêm tính năng) nhưng hạn chế sửa đổi code lõi. Sử dụng Interface hoặc Kế thừa; ví dụ khi thêm kỹ năng mạng mới, hãy tạo class kế thừa từ BaseSkill thay vì vào class chính thêm một câu lệnh switch-case dài dằng dặc.
* **L, I, D:** Các class con phải thay thế được class cha không gây lỗi (Liskov) ; tách nhỏ các Interface chuyên biệt thay vì tạo một Interface khổng lồ (Interface Segregation) ; và các module cấp cao không nên phụ thuộc trực tiếp vào module cấp thấp mà qua lớp trừu tượng Abstraction (Dependency Inversion).
*Các nguyên tắc tối giản:
* **KISS (Keep It Simple, Stupid):** Giữ code đơn giản nhất có thể, tránh phức tạp hóa thuật toán vô ích.
* **DRY (Don't Repeat Yourself):** Đừng lặp lại chính mình; đóng gói code dùng lại từ 2 lần trở lên thành hàm/class.


* **YAGNI (You Aren't Gonna Need It):** Đừng viết trước những tính năng "nghĩ là tương lai sẽ cần", hãy tập trung giải quyết bài toán hiện tại.
* Tuân thủ kiến trúc Server-Authoritative: Mọi logic thay đổi trạng thái game (trừ máu, tính sát thương, vật phẩm) bắt buộc thực thi trên Server. 
* Tách biệt rõ ràng script xử lý logic (Server) và script xử lý Hiệu ứng/Giao diện (Client).
* Đặt tên Class, Struct, Method theo chuẩn PascalCase (ví dụ: `PlayerNetworkHandler`).
* Đặt tên biến private hoặc protected theo chuẩn camelCase kèm gạch dưới (ví dụ: `_currentHealth`).
* Gắn hậu tố định danh mạng rõ ràng cho các hàm RPC (ví dụ: `FireServerRpc`, `PlayHitEffectClientRpc`).

* không được viết trực tiếp logic game vào các hàm mặc định của unity