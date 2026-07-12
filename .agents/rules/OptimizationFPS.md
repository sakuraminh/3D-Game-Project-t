---
trigger: always_on
---

# TỐI ƯU HIỆU NĂNG VÀ FPS
* Bật Network Culling trên engine mạng để ngừng gửi dữ liệu tọa độ của các vật thể nằm ngoài tầm nhìn người chơi.
* Áp dụng Static Batching cho toàn bộ vật thể môi trường tĩnh nhằm giảm lượng Draw Calls.
* Thiết lập hệ thống LOD (Level of Detail) cho lưới 3D của nhân vật và quái vật để giảm tải tính toán đa giác khi chúng ở cách xa Camera.
* Sử dụng giải pháp chiếu sáng kết hợp: Bake Lightmap cho cảnh tĩnh và Adaptive Probe Volumes (APV) của Unity 6.5 cho các đối tượng mạng di chuyển.