# Tổng hợp Dự án: Game 3D Multiplayer (Server-Authoritative)

Tài liệu này tổng hợp toàn bộ các phần việc đã hoàn thành, cấu trúc hệ thống hiện tại và kiến trúc mã nguồn của dự án game sử dụng Unity 6.5, Netcode for GameObjects (NGO).

---

## 1. Cấu trúc Dự án & Phân chia Mã nguồn
Mã nguồn được phân tách rõ ràng theo mô hình mạng và được đặt trong `Assets/_Datas/Scripts/`:

### 🔹 [Shared](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared) (Dữ liệu & Cấu trúc dùng chung)
* [CharacterConfig.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared/CharacterConfig.cs): Cấu hình chỉ số nhân vật dựa trên ScriptableObject.
* [SkillData.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared/SkillData.cs): Định nghĩa dữ liệu kỹ năng mạng.
* [PlayerState.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared/PlayerState.cs): Định nghĩa các trạng thái của người chơi (FSM States enum).
* [PlayerNetworkData.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared/PlayerNetworkData.cs): Quản lý và đồng bộ hóa các `NetworkVariable` chỉ số mạng (HP, MP, Level, v.v.).
* [PlayerNetworkHandler.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared/PlayerNetworkHandler.cs): Trọng tâm kết nối, xử lý sự kiện nạp dữ liệu mạng khi spawn nhân vật.
* [ServerAuthoritativeNetworkPool.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared/ServerAuthoritativeNetworkPool.cs): Cơ chế pooling đối tượng mạng đồng bộ hóa dưới sự kiểm soát của Server.
* [ResourceManager.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared/ResourceManager.cs): Quản lý tải tài nguyên động thông qua Addressables hoặc nạp trực tiếp.

### 🔹 [Server](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server) (Logic chỉ chạy trên Server)
* [NetworkEntityFactory.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server/NetworkEntityFactory.cs): Factory mẫu khởi tạo tập trung các Network Object nhằm bảo vệ tính toàn vẹn của Server-Authoritative.
* [PlayerMovement.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server/PlayerMovement.cs): Logic di chuyển Authoritative trên Server dựa trên chỉ thị vị trí từ Client.
* [PlayerServerCombat.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server/PlayerServerCombat.cs): Validate và xử lý tấn công, trừ máu, áp dụng sát thương và gọi ClientRpc để thực thi hiệu ứng.
* [PlayerStateMachine.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server/PlayerStateMachine.cs) & [PlayerBaseState.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server/PlayerBaseState.cs) & [PlayerStates.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server/PlayerStates.cs): Hệ thống máy trạng thái hữu hạn (FSM) quản lý hành vi của người chơi ở phía server để từ chối các hành động không hợp lệ.

### 🔹 [Client](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client) (Giao diện, Input & Hiệu ứng)
* [ConnectionHUD.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client/ConnectionHUD.cs): Giao diện kết nối mạng đơn giản (Host/Client/Server).
* [InputManager.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client/InputManager.cs) & [PlayerInputHandler.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client/PlayerInputHandler.cs): Đăng ký và lắng nghe dữ liệu từ Input System mới của Unity.
* [PlayerCameraFollow.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client/PlayerCameraFollow.cs): Cơ chế Camera bám theo Player cục bộ.
* [PlayerHUDUIHandler.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client/PlayerHUDUIHandler.cs): HUD chính (máu, năng lượng, thanh kỹ năng) xây dựng bằng UI Toolkit (sử dụng UXML & USS), lắng nghe sự thay đổi chỉ số bằng Observer Pattern thay vì polling.
* [PlayerOverheadUIHandler.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client/PlayerOverheadUIHandler.cs): Thanh máu nổi trên đầu nhân vật (World-Space UGUI).
* [DamageTextPoolManager.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client/DamageTextPoolManager.cs) & [FloatingDamageText.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client/FloatingDamageText.cs): Hệ thống số nhảy sát thương sử dụng Object Pooling tối ưu RAM và GC.

---

## 2. Các Mốc Phát triển (Lịch sử Commit chính)
1. **Khởi tạo & LFS**: Thiết lập `.gitattributes` để track các file dung lượng lớn qua LFS và chuẩn hóa cấu trúc thư mục ban đầu.
2. **Quy tắc phát triển**: Thêm tài liệu `.agents` và `code.md` làm nền tảng kiểm soát clean code, SOLID và quy định thiết kế UI lai (UI Toolkit cho Screen-Space, UGUI cho World-Space).
3. **Môi trường & Assets**: Import tài nguyên texture miễn phí phục vụ dựng cảnh.
4. **Hệ thống Multiplayer cơ bản**: Cấu hình NetworkManager, xây dựng HUD kết nối, tạo NetworkSceneCreator editor utility và thiết lập cảnh test multiplayer.
5. **Logic cốt lõi (FSM & Movement)**: Triển khai di chuyển đồng bộ mạng và điều hướng FSM.
6. **Hệ thống Chiến đấu & UI**:
   - Tích hợp chiến đấu authoritative phía server, áp dụng sát thương đồng bộ.
   - Xây dựng UI HUD qua UI Toolkit (`PlayerHUDUIHandler.cs`) lắng nghe sự kiện mạng Owner-Only.
   - Thêm số sát thương nhảy (`FloatingDamageText`) thông qua cơ chế Pooling.
   - Tích hợp các ví dụ TextMesh Pro và tối ưu cấu hình.

---

## 3. Các Nguyên tắc Thiết kế được Áp dụng
* **Mô hình Server-Authoritative**: Mọi thao tác tính toán di chuyển, sát thương, trạng thái nhân vật đều được tính toán và phê duyệt bởi Server. Client chỉ gửi tín hiệu yêu cầu (`ServerRpc`) và cập nhật giao diện hiển thị thông qua các sự kiện mạng (`ClientRpc` hoặc thay đổi `NetworkVariable`).
* **Không Polling ở Update**: UI cập nhật thông tin qua cơ chế Đăng ký Sự kiện (Observer Pattern) thay vì gọi liên tục trong `Update()`.
* **Object Pooling**: Hạn chế `Instantiate()` và `Destroy()` cho các thực thể ngắn hạn như số sát thương nhằm triệt tiêu GC Spikes.
