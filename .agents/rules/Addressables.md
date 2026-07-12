---
trigger: always_on
---

# QUẢN LÝ TÀI NGUYÊN (ASSETS)
* Áp dụng Data-Driven Design: Lưu toàn bộ chỉ số cân bằng game (sát thương, lượng máu, tốc độ) vào ScriptableObjects, tuyệt đối không hard-code số liệu vào kịch bản C#.
* Không lưu trữ các tài nguyên nặng vào thư mục `Resources` mặc định của Unity.
* Bắt buộc dùng Unity Addressables System để tải bất đồng bộ (LoadAssetAsync) các model 3D nặng hoặc map đấu từ RAM, và giải phóng ngay (ReleaseAsset) khi không còn sử dụng.