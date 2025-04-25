# 🎹 VirtKeyb - Virtual Keyboard App

VirtKeyb là một ứng dụng bàn phím ảo đa nền tảng được phát triển bằng [AvaloniaUI](https://avaloniaui.net/). Ứng dụng cho phép hiển thị bàn phím trực quan, nhận diện phím nhấn từ bàn phím vật lý, hỗ trợ thao tác với bàn phím ảo, và có hiệu ứng animation mô phỏng thao tác nhấn phím.

---

## 🚀 Tính năng nổi bật

- **Hiển thị bàn phím ảo tương tác**
  - Layout QWERTY mô phỏng bàn phím thật.
  - Các phím có thể nhấn bằng chuột.

- **Nhận diện phím nhấn vật lý**
  - Khi người dùng nhấn phím trên bàn phím thật, phím đó được đánh dấu (hiệu ứng sáng lên).

- **Animation nhấn phím**
  - Animation rõ ràng mô phỏng thao tác nhấn và thả phím.

- **Đa nền tảng**
  - Hỗ trợ Windows và Linux (đã test trên Ubuntu 25.04).
  - Có tiềm năng chạy trên macOS nhờ AvaloniaUI.

---

## 🧠 Kiến thức đã áp dụng

### Windows:
- Nhận phím: `SetWindowsHookEx`, `WM_KEYDOWN`, `WM_KEYUP`
- Gửi phím: `SendInput`, `keybd_event`, `SendMessage`

### Linux (Ubuntu):
- Nhận phím: Đọc từ `/dev/input/eventX` (cần quyền root hoặc udev rules)
- Gửi phím: `uinput`, `xdotool`, `X11` API

> ⚠️ Do cơ chế xử lý phím khác biệt giữa Windows và Linux, phần gửi phím ra ngoài vẫn đang được phát triển thêm theo hướng viết các adapter riêng cho từng nền tảng.

---

## 📦 Cài đặt & chạy thử

### Yêu cầu:
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- AvaloniaUI (qua NuGet)
- Hệ điều hành hỗ trợ: **Windows**, **Linux (Ubuntu 25.04 trở lên)**

### Chạy thử:

```bash
git clone https://github.com/yourusername/VirtKeyb.git
cd VirtKeyb
dotnet run --project VirtKeyb

