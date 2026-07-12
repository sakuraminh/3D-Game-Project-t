---
trigger: always_on
---

# CẤU TRÚC THƯ MỤC
Quy hoạch cấu trúc thư mục dự án trong thư mục `Assets/` như sau:

### 1. Cấu trúc Thư mục Gốc `Assets/`

| Thư mục / File | Mục đích sử dụng |
| :--- | :--- |
| `_Datas/` | Thư mục trung tâm chứa toàn bộ mã nguồn, tài nguyên và cấu hình của dự án. |
| `_Scenes/` | Chứa các cảnh mặc định ban đầu hoặc cảnh kiểm thử đơn giản (ví dụ: `SampleScene.unity`). |
| `AddressableAssetsData/` | Cấu hình và dữ liệu quản lý của Unity Addressables System (dùng để tải bất đồng bộ assets). |
| `Editor/` | Chứa các script tiện ích chỉ chạy trong Unity Editor (ví dụ: `NetworkSceneCreator.cs`). |
| `Game Buffs/` | Chứa tài nguyên đồ họa, âm thanh, mô hình 3D (Được quản lý bởi Git LFS). |
| `Settings/` | Cấu hình Universal Render Pipeline (URP). |
| `TextMesh Pro/` | Chứa Font và các tài nguyên của hệ thống TextMesh Pro. |
| `MInputSystem.inputactions` / `.cs` | File định nghĩa và tự sinh cấu hình của Unity Input System mới. |

### 2. Cấu trúc bên trong `Assets/_Datas/` (Thư mục trung tâm)

| Thư mục | Mục đích sử dụng |
| :--- | :--- |
| `Prefabs/Local/` | Chứa các Prefab cục bộ (hiệu ứng tĩnh, môi trường không cần đồng bộ qua mạng). |
| `Prefabs/Network/` | Chứa các Prefab đồng bộ mạng (Player, Enemy...) có đính kèm component `NetworkObject`. |
| `Scenes/` | Chứa các cảnh chơi multiplayer chính thức (ví dụ: `MultiplayerTestScene.unity`). |
| `ScriptableObjects/` | Lưu trữ cấu hình dữ liệu tĩnh (Data-Driven Design), gồm các thư mục con `Player/` và `Enemy/`. |
| `Scripts/Client/` | Code chỉ chạy trên Client (Giao diện HUD UI Toolkit, Overhead UGUI, Input, Camera, VFX, Audio). |
| `Scripts/Server/` | Code chỉ chạy trên Server (Logic di chuyển, AI quái vật FSM, Validate combat, Spawner). |
| `Scripts/Shared/` | Code chứa cấu trúc dữ liệu, NetworkVariable, cấu hình dùng chung cho cả Client và Server. |
| `UI/` | Chứa thiết kế giao diện UI Toolkit (file `.uxml`, `.uss`, PanelSettings). |