---
trigger: always_on
---

# TỐI ƯU BỘ NHỚ RAM VÀ GARBAGE COLLECTION (GC)
* Bắt buộc sử dụng hệ thống Object Pooling cho các đối tượng sinh ra và mất đi liên tục (đạn bắn, máu nổ, âm thanh va chạm).
* Tuyệt đối cấm sử dụng `Instantiate()` và `Destroy()` trong thời gian thực (runtime gameplay loop).
* Lưu trữ bộ đệm (cache) cho tất cả các Component cần thiết tại hàm `Awake()` hoặc `Start()`.
* Cấm sử dụng `GetComponent()`, `GameObject.Find()` hoặc khởi tạo đối tượng vùng nhớ mới (`new array`, `new string`) bên trong các vòng lặp khung hình như `Update()`.