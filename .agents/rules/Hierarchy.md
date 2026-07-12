---
trigger: always_on
---

# CẤU TRÚC CÂY HIERARCHY
* Gom nhóm các GameObject vào các root rỗng có tọa độ gốc (0,0,0) như: `[Managers]`, `[Envs]`, `[NetworkEntitys]`, `[LocalEffects]`.
* Giới hạn độ sâu của cây cha-con (parent-child hierarchy) tối đa ở mức 4 tầng để tránh nghẽn cổ chai CPU khi tính toán ma trận Transform động qua mạng.
* Đánh dấu cờ `Static` cho toàn bộ các vật thể thuộc nhóm `[Envs]` để engine tự động tối ưu hóa.

## Quy tắc Gộp nhóm thực thể chơi mạng (Network Entities Parenting)
* **Tuyệt đối không dùng `transform.SetParent` thông thường**: Unity Netcode chặn và báo lỗi nếu gán thực thể mạng (`NetworkObject`) vào GameObject thường không có `NetworkObject`.
* **Sử dụng `TrySetParent`**: Bắt buộc sử dụng phương thức `TrySetParent(parentNetObj, false)` của Netcode sau khi thực thể đã được spawn để gộp nhóm.
* **Gộp nhóm động ở Runtime qua Prefab**:
  - Nhóm cha gốc `[NetworkEntitys]`, nhóm con `[Players]`, `[Enemies]` phải được sinh tự động ở runtime từ một Prefab trống chứa `NetworkObject` và component `NetworkGroupParent` (để đồng bộ hóa tên hiển thị của nhóm từ Server xuống Client).
  - Sử dụng hàm `GetOrCreateRuntimeParent` trong `NetworkEntityFactory.cs` để quản lý việc tìm/tạo tự động các nhóm cha này từ Object Pool trước khi gán thực thể.
* **Tự động kích hoạt đối với Player (Auto Spawn)**: Đối với các thực thể tự động sinh khi kết nối (như Player), bắt buộc phải gọi logic gộp nhóm tại callback `OnNetworkSpawn` của Client/Server (trong `PlayerNetworkHandler.cs`) để tự động hóa hoàn toàn Hierarchy.

## Quy tắc Gộp nhóm hiệu ứng cục bộ (Local Effects Parenting)
* **Gom nhóm dưới `[LocalEntitys]`**: Toàn bộ các hiệu ứng Client-only không đồng bộ mạng (ví dụ: chữ số nhảy sát thương `Floating Damage Text`, các hạt VFX ngắn hạn) phải được gộp nhóm dưới root rỗng `[LocalEntitys]`.
* **Tránh rác Hierarchy từ Object Pool**: Các đối tượng hiệu ứng lấy từ Object Pool (như `DamageTextPoolManager.cs`) phải được khởi tạo trực tiếp làm con của sub-root tương ứng (ví dụ: `[LocalEntitys] / [DamageTexts]`) ngay tại thời điểm Instantiate. Điều này đảm bảo cả lúc hoạt động (Active) lẫn lúc thu hồi về Pool (Inactive) chúng đều nằm gọn gàng bên trong thư mục, giữ root level của Scene luôn sạch sẽ.