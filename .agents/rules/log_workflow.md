---
trigger: always_on
---

# QUY TRÌNH KIỂM TRA LOG (LOG WORKFLOW)
*Áp dụng bắt buộc cho cả AI Agent và Developer nhằm phát hiện sớm lỗi biên dịch (Compilation Error), cảnh báo (Warning), lỗi mạng (Netcode Exception) hoặc rò rỉ bộ nhớ (Memory Leak).*

---

## 1. Nguyên Tắc Chung
* **Vị trí Log chính**: [Editor.log](file:///d:/archive/Unity_Project/3D-Game-Project-t/Logs/Editor.log) (Lưu trữ toàn bộ nhật ký biên dịch và hoạt động của Unity Editor).
* **Thời điểm kiểm tra**:
  1. khi tôi yêu cầu
  2. Khi Unity Editor vừa hoàn thành biên dịch (Compile).
  3. Khi phát hiện hành vi bất thường trong game (mất đồng bộ, nhân vật không di chuyển, UI không cập nhật).

---

## 2. Quy Trình Kiểm Tra Cho AI Agent (Tự động/Bán tự động)
Mỗi khi chỉnh sửa code hoặc hoàn thành một thay đổi, Agent **bắt buộc** phải thực hiện kiểm tra log mới theo cơ chế đánh dấu vị trí dòng như sau:

```mermaid
graph TD
    A[Chỉnh sửa / Thêm Code] --> B[Đọc logendnumber.txt để lấy mốc dòng log cũ]
    B --> C[Đọc Editor.log từ dòng mốc + 1 đến cuối file và ngay lập tức ghi mốc dòng cuối cùng vào file logendnumber.txt]
    C --> D{Có Lỗi/Cảnh Báo?}
    D -- Có --> E[Phân tích lỗi & Không sửa code ngay mà hãy đưa ra các phương án giải quyết để tôi(Developer) có thể lựa chọn hoạc để tôi nhập viết một phương án khác.]
    E --> A
    D -- Không --> F[Ghi tổng số dòng mới vào logendnumber.txt]
    F --> G[Xác nhận code sạch & Bàn giao]
```

### Chi tiết các bước cho Agent:
1. **Xác định điểm bắt đầu**: Đọc số dòng đã lưu trong file [logendnumber.txt](file:///d:/archive/Unity_Project/3D-Game-Project-t/logendnumber.txt) (nếu tồn tại) để xác định mốc dòng log cũ đã qua.
2. **Ghi lại mốc dòng cuối cùng vào file logendnumber.txt
3. **Truy cập File Log**: Sử dụng công cụ `view_file` để đọc file [Editor.log](file:///d:/archive/Unity_Project/3D-Game-Project-t/Logs/Editor.log) bắt đầu từ dòng `logendnumber + 1` cho tới dòng cuối cùng của file.
4. **Tìm kiếm từ khóa lỗi (Error Patterns)**:
   * `error CS`: Lỗi biên dịch C#.
   * `Exception`: Lỗi runtime nguy hiểm (NullReferenceException, ArgumentNullException, v.v.).
   * `Assertion failed`: Lỗi kiểm tra logic khẳng định.
   * `NullReferenceException`: Lỗi tham chiếu Null (đặc biệt quan trọng với Component Management).
5. **Hành động & Cập nhật**:
   * Nếu phát hiện lỗi: Không sửa code ngay mà hãy đưa ra các phương án giải quyết để tôi(Developer) có thể lựa chọn hoạc để tôi nhập viết một phương án khác.
   * Nếu không phát hiện lỗi: Đọc tổng số dòng hiện tại của [Editor.log](file:///d:/archive/Unity_Project/3D-Game-Project-t/Logs/Editor.log) và cập nhật số dòng này vào file [logendnumber.txt](file:///d:/archive/Unity_Project/3D-Game-Project-t/logendnumber.txt) để làm mốc cho lần kiểm tra tiếp theo trước khi bàn giao.

---