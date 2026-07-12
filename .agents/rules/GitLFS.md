---
trigger: always_on
---

# QUẢN LÝ VERSION CONTROL (GIT LFS)
* **repository** https://github.com/sakuraminh/3D-Game-Project-t.git
* Áp dụng file `.gitignore` chuẩn của Unity để tự động loại bỏ các thư mục tự sinh như `Library`, `Temp`, `Logs`, `Obj`.
* Chỉ đẩy mã nguồn và siêu dữ liệu nhẹ (`.cs`, `.prefab`, `.asset`, `.meta`) lên Git tracking thông thường.
* Bắt buộc sử dụng Git LFS (Large File Storage) cho toàn bộ tài nguyên đồ họa và âm thanh để lịch sử commit không bị phình to.
* Thiết lập `.gitattributes` tracking LFS cho các định dạng sau: `.fbx`, `.obj`, `.png`, `.jpg`, `.psd`, `.wav`, `.mp3`, `.mp4`, `.blend`, `.tga`.