# Tổng hợp Dự án: Game 3D Multiplayer (Server-Authoritative)

Tài liệu này tổng hợp toàn bộ các phần việc đã hoàn thành, cấu trúc hệ thống hiện tại và kiến trúc mã nguồn của dự án game sử dụng Unity 6.5, Netcode for GameObjects (NGO).

---

## 1. Cấu trúc Dự án & Phân chia Mã nguồn
Mã nguồn được phân tách rõ ràng theo mô hình mạng và được đặt trong `Assets/_Datas/Scripts/`:

### 🔹 [Shared](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared) (Dữ liệu & Cấu trúc dùng chung)
* [CharacterConfig.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared/CharacterConfig.cs): Cấu hình chỉ số nhân vật dựa trên ScriptableObject.
* [EnemyConfig.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared/EnemyConfig.cs): Cấu hình chỉ số cơ bản của quái vật dựa trên ScriptableObject và danh sách Loot rơi ra.
* [EnemyState.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared/EnemyState.cs): Định nghĩa các trạng thái của quái vật (FSM States enum).
* [EnemyNetworkData.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared/EnemyNetworkData.cs): Quản lý và đồng bộ hóa lượng máu cùng trạng thái của quái vật trên mạng. Tích hợp bộ lọc hiển thị mạng (Network Culling 50m).
* [SkillData.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared/SkillData.cs): Định nghĩa dữ liệu kỹ năng mạng.
* [PlayerState.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared/PlayerState.cs): Định nghĩa các trạng thái của người chơi (FSM States enum).
* [PlayerNetworkData.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared/PlayerNetworkData.cs): Quản lý và đồng bộ hóa các `NetworkVariable` chỉ số mạng (HP, MP, Level, v.v.).
* [PlayerNetworkHandler.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared/PlayerNetworkHandler.cs): Trọng tâm kết nối, xử lý sự kiện nạp dữ liệu mạng khi spawn nhân vật. Tích hợp Network Culling (30m).
* [ServerAuthoritativeNetworkPool.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared/ServerAuthoritativeNetworkPool.cs): Cơ chế pooling đối tượng mạng đồng bộ hóa dưới sự kiểm soát của Server.
* [ResourceManager.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared/ResourceManager.cs): Quản lý tải tài nguyên động thông qua Addressables hoặc nạp trực tiếp.
* [GameplayManager.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Shared/GameplayManager.cs): Quản lý trạng thái chung và xử lý yêu cầu hồi sinh từ Client không sở hữu thực thể (RequireOwnership = false).


### 🔹 [Server](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server) (Logic chỉ chạy trên Server)
* [NetworkEntityFactory.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server/NetworkEntityFactory.cs): Factory mẫu khởi tạo tập trung các Network Object nhằm bảo vệ tính toàn vẹn của Server-Authoritative. Tự động phân nhóm các thực thể Player vào `[Players]` và Enemy vào `[Enemies]` lồng bên dưới GameObject cha gốc `[NetworkEntitys]`.
* [PlayerMovement.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server/PlayerMovement.cs): Logic di chuyển Authoritative trên Server dựa trên chỉ thị vị trí từ Client.
* [PlayerServerCombat.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server/PlayerServerCombat.cs): Validate và xử lý tấn công, trừ máu, áp dụng sát thương và gọi ClientRpc để thực thi hiệu ứng.
* [PlayerStateMachine.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server/PlayerStateMachine.cs) & [PlayerBaseState.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server/PlayerBaseState.cs) & [PlayerStates.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server/PlayerStates.cs): Hệ thống máy trạng thái hữu hạn (FSM) quản lý hành vi của người chơi ở phía server để từ chối các hành động không hợp lệ.
* [EnemyStateMachine.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server/EnemyStateMachine.cs) & [EnemyBaseState.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server/EnemyBaseState.cs) & [EnemyStates.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server/EnemyStates.cs): Máy trạng thái (FSM) quản lý hành vi di chuyển, săn đuổi, tấn công của AI trên Server.
* [EnemyMovement.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server/EnemyMovement.cs): Điều khiển NavMeshAgent di chuyển của quái vật trên Server.
* [EnemyServerCombat.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server/EnemyServerCombat.cs): Logic quái vật nhận sát thương, đánh người chơi và phát hiệu ứng số nhảy ClientRpc.
* [EnemySpawnerManager.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server/EnemySpawnerManager.cs): Quản lý hệ thống sinh quái vật qua mạng sử dụng Factory và Pooling trên Server.

### 🔹 [Client](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client) (Giao diện, Input & Hiệu ứng)
* [ConnectionHUD.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client/ConnectionHUD.cs): Giao diện kết nối mạng đơn giản (Host/Client/Server).
* [InputManager.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client/InputManager.cs) & [PlayerInputHandler.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client/PlayerInputHandler.cs): Đăng ký và lắng nghe dữ liệu từ Input System mới của Unity.
* [PlayerCameraFollow.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client/PlayerCameraFollow.cs): Cơ chế Camera bám theo Player cục bộ.
* [PlayerHUDUIHandler.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client/PlayerHUDUIHandler.cs): HUD chính (máu, năng lượng, thanh kỹ năng) xây dựng bằng UI Toolkit (sử dụng UXML & USS), lắng nghe sự độ thay đổi chỉ số bằng Observer Pattern thay vì polling.
* [PlayerOverheadUIHandler.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client/Player/PlayerOverheadUIHandler.cs): Thanh máu nổi trên đầu nhân vật (World-Space UGUI).
* [ResurrectionUIHandler.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client/Player/ResurrectionUIHandler.cs): Giao diện thông báo chết và nút Hồi sinh sinh tự động tại runtime bằng UI Toolkit.
* [EnemyOverheadUIHandler.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client/Enemy/EnemyOverheadUIHandler.cs): Thanh máu nổi trên đầu quái vật (World-Space UGUI) kế thừa NetworkBehaviour để đồng bộ đúng với Object Pooling. Tích hợp cơ chế phòng vệ chống đệ quy lặp vô hạn khi tải mô hình.
* [DamageTextPoolManager.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client/DamageTextPoolManager.cs) & [FloatingDamageText.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Client/FloatingDamageText.cs): Hệ thống số nhảy sát thương sử dụng Object Pooling tối ưu RAM và GC.

---

## 2. Các Mốc Phát triển (Lịch sử Commit chính)
1. **Khởi tạo & LFS**: Thiết lập `.gitattributes` để track các file dung lượng lớn qua LFS và chuẩn hóa cấu trúc thư mục ban đầu.
2. **Quy tắc phát triển**: Thêm tài liệu `.agents` và `code.md` làm nền tảng kiểm soát clean code, SOLID và quy định thiết kế UI lai (UI Toolkit cho Screen-Space, UGUI cho World-Space).
3. **Môi trường & Assets**: Import tài nguyên texture miễn phí phục vụ dựng cảnh.
4. **Hệ thống Multiplayer cơ bản**: Cấu hình NetworkManager, xây dựng HUD kết nối, tạo NetworkSceneCreator editor utility và thiết lập cảnh test multiplayer.
5. **Logic cốt lõi (FSM & Movement)**: Triển khai di chuyển đồng bộ mạng và điều hướng FSM.
6. **Hành vi Enemy & Spawner**: Thiết kế FSM tuần tra/săn đuổi và Spawner Pool trên Server.
7. **Tối ưu hóa & Client Visuals**:
   - Thêm `EnemyOverheadUIHandler` tự động tải mô hình Addressables và đồng bộ máu.
   - Triển khai **Network Culling (50m)** cho quái vật giúp tiết kiệm băng thông mạng.
8. **Đồng bộ hóa Tài liệu**: Cập nhật tài liệu quy hoạch cấu trúc thư mục chi tiết `FolderStructure.md` để phản ánh đầy đủ cấu trúc thư mục thực tế của dự án.
9. **Khắc phục lỗi đệ quy Enemy & Tối ưu hóa Hierarchy**: 
   - Thêm cơ chế phòng ngự (Guard Clause) trong `EnemyOverheadUIHandler` để chặn tải đệ quy vô hạn khi Key trỏ nhầm vào Prefab chính.
   - Sửa đổi `GoblinConfig.asset` làm trống `_modelAddressableKey` để tránh tự nhân bản.
   - Cập nhật `NetworkEntityFactory.cs` tự động thu gom các thực thể Player vào `[Players]` và Enemy vào `[Enemies]` lồng bên trong GameObject cha gốc `[NetworkEntitys]`. Bổ sung hook gộp nhóm trong `OnNetworkSpawn` của `PlayerNetworkHandler.cs` để hỗ trợ cả trường hợp Player được Netcode tự động sinh ra (Auto Spawn).
   - Cấu trúc lại `DamageTextPoolManager.cs` tự động gom nhóm toàn bộ các chữ số nhảy sát thương (Floating Damage Text) dưới root cha rỗng `[LOCAL_EFFECTS] / [DamageTexts]`.
10. **Xoay camera, Chọn mục tiêu, Kỹ năng đánh quái, Chết & Hồi sinh qua Pool, Đồng bộ HP**:
    - Tích hợp tính năng nhấn giữ chuột phải để xoay camera mượt mà quanh Player.
    - Click chuột trái vào quái vật để chọn mục tiêu tấn công và truyền target lên Server qua RPC kỹ năng (Lựa chọn A).
    - Cấu hình Player chết tự động despawn đưa về Pool sau 2 giây trễ. Sinh Resurrection UI lúc runtime hiển thị nút Hồi sinh, gửi RPC qua `GameplayManager` để lấy lại Player mới từ Pool và thiết lập lại PlayerObject.
    - Chuyển `EnemyOverheadUIHandler` sang NetworkBehaviour, đồng bộ chuẩn xác sự kiện thay đổi máu để slider HP nổi trên đầu tụt mượt mà theo lượng máu khi chịu sát thương.
11. **Xoay nhân vật theo Camera (Camera-Relative Movement)**:
    - Truyền thêm tham số `cameraYaw` (góc xoay Y của camera chính trên Client) qua RPC di chuyển lên Server.
    - Cấu trúc lại logic di chuyển trong `PlayerMovement.cs` để tính toán hướng di chuyển WASD tương đối theo hướng nhìn ngang của Camera (Camera-Relative Movement).
    - Nhân vật tự động quay mặt theo hướng di chuyển tương đối này khi người chơi nhấn di chuyển (WASD), giữ nhân vật đứng yên khi chỉ xoay camera (thiết kế kiểu Phiêu lưu RPG 3D).
12. **Quy trình kiểm tra Log**:
    - Tạo file tài liệu quy tắc [.agents/rules/log_workflow.md](file:///d:/archive/Unity_Project/3D-Game-Project-t/.agents/rules/log_workflow.md) chuẩn hóa quy trình kiểm tra log tự động cho Agent và hướng dẫn kiểm tra thủ công thời gian thực cho Developer nhằm phát hiện sớm lỗi biên dịch (Compilation Error) hoặc Exception.
13. **Khắc phục lỗi GameplayManager.Instance is null trên Client**:
    - Thêm trường `_gameplayManagerPrefab` và hàm `OnValidate()` vào [NetworkEntityFactory.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/_Datas/Scripts/Server/NetworkEntityFactory.cs) để tự động gán prefab `GameplayManager` trong Editor.
    - Cấu trúc lại `SpawnGameplayManagerIfNeeded()` để sinh `GameplayManager` từ Prefab này và gọi `Spawn()` để đồng bộ qua mạng xuống Client.
    - Cập nhật [NetworkSceneCreator.cs](file:///d:/archive/Unity_Project/3D-Game-Project-t/Assets/Editor/NetworkSceneCreator.cs) tự động sinh Prefab `GameplayManager` và đăng ký nó vào `NetworkPrefabs` của `NetworkManager`. Bổ sung MenuItem `"Architecture/Setup GameplayManager Prefab"` để thiết lập nhanh trên Scene hiện tại.
77: 14. **Cấu hình Workspace (Ẩn file .meta & thư mục hệ thống)**:
78:     - Cấu hình file Workspace [.vscode/settings.json](file:///d:/archive/Unity_Project/3D-Game-Project-t/.vscode/settings.json) để ẩn các file `.meta`, thư mục tự sinh của Unity (`Library/`, `Temp/`, `obj/`, `UserSettings/`), thư mục log (`Logs/`), file solution (`*.sln`, `*.csproj`), file lưu mốc log của Agent (`logendnumber.txt`), và các file cấu hình Git để tối ưu hóa hiển thị trong Explorer, giúp giao diện gọn gàng và tránh rối mắt.
79: 
80: ---

## 3. Các Nguyên tắc Thiết kế được Áp dụng
* **Mô hình Server-Authoritative**: Mọi thao tác tính toán di chuyển, sát thương, trạng thái nhân vật đều được tính toán và phê duyệt bởi Server. Client chỉ gửi tín hiệu yêu cầu (`ServerRpc`) và cập nhật giao diện hiển thị thông qua các sự kiện mạng (`ClientRpc` hoặc thay đổi `NetworkVariable`).
* **Không Polling ở Update**: UI cập nhật thông tin qua cơ chế Đăng ký Sự kiện (Observer Pattern) thay vì gọi liên tục trong `Update()`.
* **Object Pooling**: Hạn chế `Instantiate()` và `Destroy()` cho các thực thể ngắn hạn như số sát thương nhằm triệt tiêu GC Spikes.
