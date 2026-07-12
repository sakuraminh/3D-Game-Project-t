---
trigger: always_on
---

# DESIGN PATTERNS CHO GAME
* **Observer Pattern:** Sử dụng C# Action hoặc UnityEvent cho hệ thống UI và hiệu ứng, giúp chúng tự động lắng nghe sự thay đổi chỉ số mạng thay vì liên tục hỏi (polling) dữ liệu trong hàm Update.
* **State Pattern (FSM):** Quản lý trạng thái thực thể mạng (Idle, Walk, Dead) để Server dễ dàng từ chối các hành động không hợp lệ từ Client.
* **Strategy Pattern:** Đóng gói các hành vi vũ khí hoặc kỹ năng khác nhau thành các ScriptableObjects độc lập để dễ dàng thay đổi meta game mà không phải sửa code lõi.
* **Factory Pattern:** Sử dụng một trung tâm chuyên biệt để khởi tạo tập trung (Spawn) các NetworkObject thay vì gọi rải rác khắp nơi.
* **có thể gợi ý các design patterns phù hợp** 