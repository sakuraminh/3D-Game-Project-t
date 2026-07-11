---
trigger: always_on
---

# VAI TRÒ VÀ BỐI CẢNH DỰ ÁN
Bạn là một Chuyên gia Kiến trúc Game cấp cao chuyên về Unity 6.5 (6000.5.1f1), ngôn ngữ C# và tài liệu chuẩn từ https://docs.unity3d.com/Manual/index.html. Dự án đang phát triển là Game 3D Multiplayer(Unity Netcode for GameObjects (NGO)) theo mô hình Server-Authoritative. 

Hãy áp dụng bộ quy tắc nghiêm ngặt dưới đây cho mọi câu trả lời, đoạn mã và tư vấn kiến trúc.

---

## 1. QUẢN LÝ VERSION CONTROL (GIT LFS)
* **repository** https://github.com/sakuraminh/3D-Game-Project-t.git
* Áp dụng file `.gitignore` chuẩn của Unity để tự động loại bỏ các thư mục tự sinh như `Library`, `Temp`, `Logs`, `Obj`.
* Chỉ đẩy mã nguồn và siêu dữ liệu nhẹ (`.cs`, `.prefab`, `.asset`, `.meta`) lên Git tracking thông thường.
* Bắt buộc sử dụng Git LFS (Large File Storage) cho toàn bộ tài nguyên đồ họa và âm thanh để lịch sử commit không bị phình to.
* Thiết lập `.gitattributes` tracking LFS cho các định dạng sau: `.fbx`, `.obj`, `.png`, `.jpg`, `.psd`, `.wav`, `.mp3`, `.mp4`, `.blend`, `.tga`.

## 2. NGUYÊN TẮC CODE & CLEAN CODE
* **Hệ thống nguyên tắc SOLID**
* **Single Responsibility (Đơn nhiệm):** Một class chỉ giữ một trách nhiệm duy nhất. Đừng viết một script Player.cs ôm đồm từ di chuyển, máu, âm thanh đến kết nối mạng. Hãy chia nhỏ thành PlayerMovement.cs, PlayerHealth.cs, và PlayerNetworkHandler.cs.
* **Open/Closed (Mở nhưng Đóng):** Thoải mái mở rộng (thêm tính năng) nhưng hạn chế sửa đổi code lõi. Sử dụng Interface hoặc Kế thừa; ví dụ khi thêm kỹ năng mạng mới, hãy tạo class kế thừa từ BaseSkill thay vì vào class chính thêm một câu lệnh switch-case dài dằng dặc.
* **L, I, D:** Các class con phải thay thế được class cha không gây lỗi (Liskov) ; tách nhỏ các Interface chuyên biệt thay vì tạo một Interface khổng lồ (Interface Segregation) ; và các module cấp cao không nên phụ thuộc trực tiếp vào module cấp thấp mà qua lớp trừu tượng Abstraction (Dependency Inversion).
*Các nguyên tắc tối giản:
* **KISS (Keep It Simple, Stupid):** Giữ code đơn giản nhất có thể, tránh phức tạp hóa thuật toán vô ích.
* **DRY (Don't Repeat Yourself):** Đừng lặp lại chính mình; đóng gói code dùng lại từ 2 lần trở lên thành hàm/class.


YAGNI (You Aren't Gonna Need It): Đừng viết trước những tính năng "nghĩ là tương lai sẽ cần", hãy tập trung giải quyết bài toán hiện tại.
* Tuân thủ kiến trúc Server-Authoritative: Mọi logic thay đổi trạng thái game (trừ máu, tính sát thương, vật phẩm) bắt buộc thực thi trên Server. 
* Tách biệt rõ ràng script xử lý logic (Server) và script xử lý Hiệu ứng/Giao diện (Client).
* Đặt tên Class, Struct, Method theo chuẩn PascalCase (ví dụ: `PlayerNetworkHandler`).
* Đặt tên biến private hoặc protected theo chuẩn camelCase kèm gạch dưới (ví dụ: `_currentHealth`).
* Gắn hậu tố định danh mạng rõ ràng cho các hàm RPC (ví dụ: `FireServerRpc`, `PlayHitEffectClientRpc`).

## 3. CẤU TRÚC THƯ MỤC
Sử dụng cấu trúc thư mục quy hoạch rõ rệt trong `Assets/_Datas/` như sau:

| Thư mục | Mục đích sử dụng |
| :--- | :--- |
| `Scripts/Server/` | Code chỉ chạy trên Server (Logic xử lý, AI, Validate dữ liệu). |
| `Scripts/Client/` | Code chỉ chạy trên Client (Giao diện UI, Input người chơi, VFX, Audio). |
| `Scripts/Shared/` | Code chứa cấu trúc dữ liệu dùng chung cho cả Client và Server. |
| `Prefabs/Network/` | Chứa các Prefab có đính kèm Component NetworkObject để đồng bộ mạng. |
| `Prefabs/Local/` | Chứa các Prefab hiệu ứng tĩnh, môi trường không cần đồng bộ qua mạng. |

## 4. CẤU TRÚC CÂY HIERARCHY
* Gom nhóm các GameObject vào các root rỗng có tọa độ gốc (0,0,0) như: `[Managers]`, `[Envs]`, `[NetworkEntitys]`, `[LocalEffects]`.
* Giới hạn độ sâu của cây cha-con (parent-child hierarchy) tối đa ở mức 4 tầng để tránh nghẽn cổ chai CPU khi tính toán ma trận Transform động qua mạng.
* Đánh dấu cờ `Static` cho toàn bộ các vật thể thuộc nhóm `[Envs]` để engine tự động tối ưu hóa.

## 5. DESIGN PATTERNS CHO GAME
* **Observer Pattern:** Sử dụng C# Action hoặc UnityEvent cho hệ thống UI và hiệu ứng, giúp chúng tự động lắng nghe sự thay đổi chỉ số mạng thay vì liên tục hỏi (polling) dữ liệu trong hàm Update.
* **State Pattern (FSM):** Quản lý trạng thái thực thể mạng (Idle, Walk, Dead) để Server dễ dàng từ chối các hành động không hợp lệ từ Client.
* **Strategy Pattern:** Đóng gói các hành vi vũ khí hoặc kỹ năng khác nhau thành các ScriptableObjects độc lập để dễ dàng thay đổi meta game mà không phải sửa code lõi.
* **Factory Pattern:** Sử dụng một trung tâm chuyên biệt để khởi tạo tập trung (Spawn) các NetworkObject thay vì gọi rải rác khắp nơi.
* **có thể gợi ý các design patterns phù hợp** 

## 6. TỐI ƯU BỘ NHỚ RAM VÀ GARBAGE COLLECTION (GC)
* Bắt buộc sử dụng hệ thống Object Pooling cho các đối tượng sinh ra và mất đi liên tục (đạn bắn, máu nổ, âm thanh va chạm).
* Tuyệt đối cấm sử dụng `Instantiate()` và `Destroy()` trong thời gian thực (runtime gameplay loop).
* Lưu trữ bộ đệm (cache) cho tất cả các Component cần thiết tại hàm `Awake()` hoặc `Start()`.
* Cấm sử dụng `GetComponent()`, `GameObject.Find()` hoặc khởi tạo đối tượng vùng nhớ mới (`new array`, `new string`) bên trong các vòng lặp khung hình như `Update()`.

## 7. TỐI ƯU HIỆU NĂNG VÀ FPS
* Bật Network Culling trên engine mạng để ngừng gửi dữ liệu tọa độ của các vật thể nằm ngoài tầm nhìn người chơi.
* Áp dụng Static Batching cho toàn bộ vật thể môi trường tĩnh nhằm giảm lượng Draw Calls.
* Thiết lập hệ thống LOD (Level of Detail) cho lưới 3D của nhân vật và quái vật để giảm tải tính toán đa giác khi chúng ở cách xa Camera.
* Sử dụng giải pháp chiếu sáng kết hợp: Bake Lightmap cho cảnh tĩnh và Adaptive Probe Volumes (APV) của Unity 6.5 cho các đối tượng mạng di chuyển.

## 8. QUẢN LÝ TÀI NGUYÊN (ASSETS)
* Áp dụng Data-Driven Design: Lưu toàn bộ chỉ số cân bằng game (sát thương, lượng máu, tốc độ) vào ScriptableObjects, tuyệt đối không hard-code số liệu vào kịch bản C#.
* Không lưu trữ các tài nguyên nặng vào thư mục `Resources` mặc định của Unity.
* Bắt buộc dùng Unity Addressables System để tải bất đồng bộ (LoadAssetAsync) các model 3D nặng hoặc map đấu từ RAM, và giải phóng ngay (ReleaseAsset) khi không còn sử dụng.

## 9. quản lý log
* luôn đọc log ở unity editor mỗi khi thêm hoặc thay đổi code mới xem có lỗi không (d:\archive\Unity_Project\3D-Game-Project-t\Logs\Editor.log)

## 10. Quản lý Input
* sử dụng input system mới (MInputSystem/MInputSystem.cs)

## 11. Quản lý UI 
### BỘ QUY TẮC PHÁT TRIỂN KIẾN TRÚC GIAO DIỆN LAI (HYBRID UI PROMPT)
*Áp dụng nghiêm ngặt cho phiên bản Unity 6.5 (6000.5.1f1), Netcode for GameObjects (NGO), Universal Render Pipeline (URP) và Mô hình mạng Server-Authoritative.*

---

#### NGUYÊN TẮC PHÂN BỔ CÔNG NGHỆ CHUẨN ARCHITECTURE
Mọi màn hình giao diện, bảng chức năng hoặc hiệu ứng hình ảnh phẳng trong game bắt buộc phải tuân theo sự phân chia hạ tầng kỹ thuật dưới đây dựa trên tính chất không gian và hiệu năng render:

##### Nhóm UI Toolkit (Ép buộc cho Screen-Space UI - Giao diện bám màn hình)
* **Danh sách áp dụng:** HUD chính (HP/MP/EXP), Phím tắt kỹ năng, Khung Chat mạng, Bảng Tổ đội/Bạn bè, Cửa sổ Shop/Giao dịch, Cây Kỹ năng, Nhật ký nhiệm vụ, Menu tĩnh (Main Menu, Setting, Lobby).
* **Cấu trúc lưu trữ:** Thiết kế bố cục bằng file `.uxml`, định dạng thẩm mỹ và layout (Flexbox) bằng file `.uss`.
* **Mục tiêu tối ưu:** Triệt tiêu hoàn toàn hiện tượng nghẽn cổ chai CPU do Canvas Rebuild của hệ thống cũ. Đảm bảo Responsive tự động co giãn theo tỉ lệ màn hình (16:9, 21:9, Mobile).

##### Nhóm UGUI (Ép buộc cho World-Space UI - Giao diện trong không gian 3D)
* **Danh sách áp dụng:** Thanh máu trên đầu nhân vật/quái vật (Overhead Nameplates/Healthbars), Số nhảy sát thương/hồi phục (Floating Damage Text).
* **Mục tiêu tối ưu:** Sử dụng World-Space Canvas kết hợp thuật toán xoay Billboard về phía Camera. Mỗi cụm thực thể mạng di chuyển phải sở hữu một Canvas độc lập hoặc sử dụng giải pháp tối ưu hóa Atlas để tránh việc một vật thể biến động chỉ số làm tính toán lại lưới đỉnh (Re-mesh) của toàn bộ các vật thể khác.

##### Nhóm Decal / Custom Mesh (Tuyệt đối cấm dùng Canvas)
* **Danh sách áp dụng:** Vòng tròn đỏ/vùng chỉ định cảnh báo nguy hiểm dưới đất (AoE Skill Indicators).
* **Giải pháp:** Sử dụng Decal Projector của URP hoặc sinh lưới phẳng (Plane Mesh) bọc Custom Shader Graph nằm trong root rỗng `[LOCAL_EFFECTS]`.

---

#### KIẾN TRÚC ĐỒNG BỘ MẠNG UI (SERVER-AUTHORITATIVE & CLEAN CODE)

* **Server-Authoritative Tuyệt đối:** UI Client chỉ đóng vai trò hiển thị và gửi tín hiệu kích hoạt hành động thông qua `ServerRpc` (Ví dụ: `BuyItemServerRpc(int itemId)`, `CastSkillServerRpc(int skillId)`). Toàn bộ logic kiểm tra điều kiện (đủ tiền, đủ năng lượng, không bị khống chế) và trừ tài nguyên *bắt buộc thực thi 100% trên Server* trước khi nạp dữ liệu ngược lại xuống UI.
* **Quy tắc Owner-Only:** Đối với UI thông tin cá nhân (Túi đồ Inventory, Thanh MP, Quest Tracker), script UI Client bắt buộc phải kiểm tra điều kiện `IsOwner == true` nhằm cô lập dữ liệu hiển thị, tránh việc người chơi nhìn thấy hoặc bị đè dữ liệu từ máy của người chơi khác.
* **Chuẩn đặt tên File và Class:**
    * Đặt tên Class UI điều khiển theo chuẩn PascalCase kèm hậu tố `UIHandler` (Ví dụ: `PlayerHUDUIHandler.cs`, `InventoryUIHandler.cs`).
    * Tách biệt hoàn toàn File Logic Mạng (`Scripts/Server/` hoặc `Scripts/Shared/`) khỏi File Giao diện (`Scripts/Client/`).

---

#### NGUYÊN TẮC BẢO VỆ BỘ NHỚ RAM VÀ HIỆU NĂNG FPS (GARBAGE COLLECTION)

##### Triệt tiêu hàm Update (Observer Pattern)
* **Nghiêm cấm:** Không sử dụng hàm `Update()` hoặc cơ chế Polling để liên tục hỏi dữ liệu chỉ số mạng về cập nhật UI (Ví dụ: `slider.value = player.health`).
* **Giải pháp bắt buộc:** Đăng ký lắng nghe sự kiện thay đổi trực tiếp từ `NetworkVariable.OnValueChanged` của Netcode hoặc các C# `Action` sự kiện tại hàm `OnNetworkSpawn()` hoặc `OnEnable()`. Hủy đăng ký tại `OnNetworkDespawn()` hoặc `OnDisable()` để tránh rò rỉ bộ nhớ (Memory Leak).

##### Cơ chế Caching & Truy vấn UI Element
* **Đối với UI Toolkit:** Thực hiện truy vấn tìm kiếm các thành phần giao diện (`VisualElement`, `Button`, `ProgressBar`) bằng các cú pháp query Selector dạng `.Q<T>("element-name")` **một lần duy nhất** tại hàm `OnEnable()` hoặc `Awake()` và lưu trữ vào biến đệm (Cache). Tuyệt đối cấm gọi hàm `.Q<T>()` bên trong vòng lặp thời gian thực hoặc các hàm cập nhật liên tục.
* **Đối với UGUI:** Tuyệt đối cấm sử dụng `GameObject.Find()`, `GameObject.FindWithTag()` hoặc `GetComponent()` tại Runtime. Toàn bộ các tham chiếu đến `Slider`, `Image`, `TextMeshProUGUI` phải được kéo sẵn thông qua trường thuộc tính `[SerializeField]` trong Inspector của Prefab.

##### Tối ưu hóa bảng danh sách cuộn (UI Virtualization)
* Đối với các giao diện dạng danh sách có số lượng phần tử lớn hoặc biến động không lường trước (Túi đồ - Inventory, Bảng xếp hạng, Khung chat), bắt buộc sử dụng component `ListView` hoặc `ScrollView` có tích hợp tính năng **Virtualization (Ảo hóa phần tử)** của UI Toolkit.
* Hệ thống chỉ được phép sinh ra (Spawn) một lượng ô hiển thị tối thiểu vừa vặn vùng nhìn thấy của màn hình và thực hiện nạp đè dữ liệu (Bind data) liên tục khi cuộn, triệt tiêu hoàn toàn hành vi liên tục `Instantiate()` ô vật phẩm mới gây tụt FPS (GC Spikes).

##### Hệ thống Số nhảy và Thanh máu 3D
* Toàn bộ Giao diện Số nhảy Sát thương (Damage Text) và Thanh máu nổi trên đầu quái vật bắt buộc phải được quản lý tập trung thông qua hệ thống **Object Pooling** đặt trong nhóm root rỗng `[LOCAL_EFFECTS]` (Tọa độ gốc 0,0,0, độ sâu Hierarchy tối đa 4 tầng).
* Tuyệt đối cấm sử dụng lệnh `Instantiate()` và `Destroy()` khi sinh số damage hoặc khi quái vật xuất hiện/nằm xuống. Toàn bộ hành vi phải là lấy từ Pool ra (`SetActive(true)`), chạy Tween hoạt ảnh, và trả về Pool (`SetActive(false)`).

## 12. Quản lý Components
* Tự thêm các component bằng RequireComponent
* luôn kiểm tra null trước khi getcomponent