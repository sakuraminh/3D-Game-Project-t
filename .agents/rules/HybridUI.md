---
trigger: always_on
---

# Quản lý UI 
## BỘ QUY TẮC PHÁT TRIỂN KIẾN TRÚC GIAO DIỆN LAI (HYBRID UI PROMPT)
*Áp dụng nghiêm ngặt cho phiên bản Unity 6.5 (6000.5.1f1), Netcode for GameObjects (NGO), Universal Render Pipeline (URP) và Mô hình mạng Server-Authoritative.*

---

### NGUYÊN TẮC PHÂN BỔ CÔNG NGHỆ CHUẨN ARCHITECTURE
Mọi màn hình giao diện, bảng chức năng hoặc hiệu ứng hình ảnh phẳng trong game bắt buộc phải tuân theo sự phân chia hạ tầng kỹ thuật dưới đây dựa trên tính chất không gian và hiệu năng render:

##### Nhóm UI Toolkit (Ép buộc cho Screen-Space UI - Giao diện bám màn hình)
* **Danh sách áp dụng:** HUD chính (HP/MP/EXP), Phím tắt kỹ năng, Khung Chat mạng, Bảng Tổ đội/Bạn bè, Cửa sổ Shop/Giao dịch, Cây Kỹ năng, Nhật ký nhiệm vụ, Menu tĩnh (Main Menu, Setting, Lobby).
* **Cấu trúc lưu trữ:** Thiết kế bố cục bằng file `.uxml`, định dạng thẩm mỹ và layout (Flexbox) bằng file `.uss`.
* **Mục tiêu tối ưu:** Triệt tiêu hoàn toàn hiện tượng nghẽn cổ chai CPU do Canvas Rebuild của hệ thống cũ. Đảm bảo Responsive tự động co giãn theo tỉ lệ màn hình (16:9, 21:9, Mobile).

#### Nhóm UGUI (Ép buộc cho World-Space UI - Giao diện trong không gian 3D)
* **Danh sách áp dụng:** Thanh máu trên đầu nhân vật/quái vật (Overhead Nameplates/Healthbars), Số nhảy sát thương/hồi phục (Floating Damage Text).
* **Mục tiêu tối ưu:** Sử dụng World-Space Canvas kết hợp thuật toán xoay Billboard về phía Camera. Mỗi cụm thực thể mạng di chuyển phải sở hữu một Canvas độc lập hoặc sử dụng giải pháp tối ưu hóa Atlas để tránh việc một vật thể biến động chỉ số làm tính toán lại lưới đỉnh (Re-mesh) của toàn bộ các vật thể khác.

##### Nhóm Decal / Custom Mesh (Tuyệt đối cấm dùng Canvas)
* **Danh sách áp dụng:** Vòng tròn đỏ/vùng chỉ định cảnh báo nguy hiểm dưới đất (AoE Skill Indicators).
* **Giải pháp:** Sử dụng Decal Projector của URP hoặc sinh lưới phẳng (Plane Mesh) bọc Custom Shader Graph nằm trong root rỗng `[LOCAL_EFFECTS]`.

---

### KIẾN TRÚC ĐỒNG BỘ MẠNG UI (SERVER-AUTHORITATIVE & CLEAN CODE)

* **Server-Authoritative Tuyệt đối:** UI Client chỉ đóng vai trò hiển thị và gửi tín hiệu kích hoạt hành động thông qua `ServerRpc` (Ví dụ: `BuyItemServerRpc(int itemId)`, `CastSkillServerRpc(int skillId)`). Toàn bộ logic kiểm tra điều kiện (đủ tiền, đủ năng lượng, không bị khống chế) và trừ tài nguyên *bắt buộc thực thi 100% trên Server* trước khi nạp dữ liệu ngược lại xuống UI.
* **Quy tắc Owner-Only:** Đối với UI thông tin cá nhân (Túi đồ Inventory, Thanh MP, Quest Tracker), script UI Client bắt buộc phải kiểm tra điều kiện `IsOwner == true` nhằm cô lập dữ liệu hiển thị, tránh việc người chơi nhìn thấy hoặc bị đè dữ liệu từ máy của người chơi khác.
* **Chuẩn đặt tên File và Class:**
    * Đặt tên Class UI điều khiển theo chuẩn PascalCase kèm hậu tố `UIHandler` (Ví dụ: `PlayerHUDUIHandler.cs`, `InventoryUIHandler.cs`).
    * Tách biệt hoàn toàn File Logic Mạng (`Scripts/Server/` hoặc `Scripts/Shared/`) khỏi File Giao diện (`Scripts/Client/`).

---

### NGUYÊN TẮC BẢO VỆ BỘ NHỚ RAM VÀ HIỆU NĂNG FPS (GARBAGE COLLECTION)

##### Triệt tiêu hàm Update (Observer Pattern)
* **Nghiêm cấm:** Không sử dụng hàm `Update()` hoặc cơ chế Polling để liên tục hỏi dữ liệu chỉ số mạng về cập nhật UI (Ví dụ: `slider.value = player.health`).
* **Giải pháp bắt buộc:** Đăng ký lắng nghe sự kiện thay đổi trực tiếp từ `NetworkVariable.OnValueChanged` của Netcode hoặc các C# `Action` sự kiện tại hàm `OnNetworkSpawn()` hoặc `OnEnable()`. Hủy đăng ký tại `OnNetworkDespawn()` hoặc `OnDisable()` để tránh rò rỉ bộ nhớ (Memory Leak).

#### Cơ chế Caching & Truy vấn UI Element
* **Đối với UI Toolkit:** Thực hiện truy vấn tìm kiếm các thành phần giao diện (`VisualElement`, `Button`, `ProgressBar`) bằng các cú pháp query Selector dạng `.Q<T>("element-name")` **một lần duy nhất** tại hàm `OnEnable()` hoặc `Awake()` và lưu trữ vào biến đệm (Cache). Tuyệt đối cấm gọi hàm `.Q<T>()` bên trong vòng lặp thời gian thực hoặc các hàm cập nhật liên tục.
* **Đối với UGUI:** Tuyệt đối cấm sử dụng `GameObject.Find()`, `GameObject.FindWithTag()` hoặc `GetComponent()` tại Runtime. Toàn bộ các tham chiếu đến `Slider`, `Image`, `TextMeshProUGUI` phải được kéo sẵn thông qua trường thuộc tính `[SerializeField]` trong Inspector của Prefab.

#### Tối ưu hóa bảng danh sách cuộn (UI Virtualization)
* Đối với các giao diện dạng danh sách có số lượng phần tử lớn hoặc biến động không lường trước (Túi đồ - Inventory, Bảng xếp hạng, Khung chat), bắt buộc sử dụng component `ListView` hoặc `ScrollView` có tích hợp tính năng **Virtualization (Ảo hóa phần tử)** của UI Toolkit.
* Hệ thống chỉ được phép sinh ra (Spawn) một lượng ô hiển thị tối thiểu vừa vặn vùng nhìn thấy của màn hình và thực hiện nạp đè dữ liệu (Bind data) liên tục khi cuộn, triệt tiêu hoàn toàn hành vi liên tục `Instantiate()` ô vật phẩm mới gây tụt FPS (GC Spikes).

#### Hệ thống Số nhảy và Thanh máu 3D
* Toàn bộ Giao diện Số nhảy Sát thương (Damage Text) và Thanh máu nổi trên đầu quái vật bắt buộc phải được quản lý tập trung thông qua hệ thống **Object Pooling** đặt trong nhóm root rỗng `[LOCAL_EFFECTS]` (Tọa độ gốc 0,0,0, độ sâu Hierarchy tối đa 4 tầng).
* Tuyệt đối cấm sử dụng lệnh `Instantiate()` và `Destroy()` khi sinh số damage hoặc khi quái vật xuất hiện/nằm xuống. Toàn bộ hành vi phải là lấy từ Pool ra (`SetActive(true)`), chạy Tween hoạt ảnh, và trả về Pool (`SetActive(false)`).