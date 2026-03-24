# Quản Lý Nhân Sự Trong Doanh Nghiệp Nhỏ

Ứng dụng web quản lý nhân sự xây dựng trên **ASP.NET 8 MVC** + **SQL Server** (LocalDB). Hỗ trợ quản lý nhân viên, phòng ban, hợp đồng, đánh giá, nghỉ phép, tuyển dụng và phân quyền theo vai trò.

---

## Yêu cầu hệ thống

| Phần mềm | Phiên bản |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | 8.0 trở lên |
| SQL Server LocalDB *(đi kèm Visual Studio)* | hoặc SQL Server Express / Full |
| Visual Studio 2022 | hoặc VS Code + C# extension |

> **Kiểm tra LocalDB có sẵn chưa:** Mở terminal, gõ `sqllocaldb info` — nếu thấy `MSSQLLocalDB` thì dùng được ngay.

---

## Cài đặt & Chạy

### 1. Clone repository

```bash
git clone <repo-url>
cd Quan_ly_nhan_su_trong_doanh_nghiep_nho
```

### 2. Cấu hình Database

**Mặc định (LocalDB — không cần cấu hình thêm):**  
Project dùng `Server=(localdb)\MSSQLLocalDB` — hoạt động ngay trên mọi máy có Visual Studio / SQL Server LocalDB.

**Nếu dùng SQL Server riêng (tùy chọn):**  
Tạo file `appsettings.Development.json` từ file mẫu:

```bash
copy appsettings.Development.json.example appsettings.Development.json
```

Sau đó mở file vừa tạo và sửa connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TÊN_MÁY_CỦA_BẠN;Database=QuanLyNhanSu;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

> **Lưu ý:** `appsettings.Development.json` đã được thêm vào `.gitignore` — file này là riêng tư của từng máy, không bị commit lên git.

### 3. Chạy ứng dụng

```bash
dotnet run
```

Lần đầu chạy, ứng dụng sẽ **tự động**:
- Tạo database `QuanLyNhanSu`
- Chạy tất cả migrations
- Seed dữ liệu mặc định (vai trò, tài khoản admin, phòng ban)

Mở browser truy cập: `https://localhost:PORT` *(PORT hiển thị trong terminal)*

---

## Tài khoản mặc định

| Vai trò | Tên đăng nhập | Mật khẩu | Quyền |
|---|---|---|---|
| Admin | `admin` | `admin` | Toàn quyền hệ thống |
| Manager | `phongnhansu` | `phongnhansu` | Quản lý Phòng Nhân sự |
| Manager | `phongkythuat` | `phongkythuat` | Quản lý Phòng Kỹ thuật |
| Manager | `phongkinhdoanh` | `phongkinhdoanh` | Quản lý Phòng Kinh doanh |

*(Tài khoản Manager được tạo tự động theo tên phòng ban đã seed)*

---

## Cấu trúc project

```
├── Controllers/          # MVC Controllers (11 controllers)
├── Data/                 # ApplicationDbContext (EF Core)
├── Migrations/           # EF Core Migrations
├── Models/               # Entity Models
├── ViewModels/           # ViewModels cho từng view
├── Views/                # Razor Views
├── wwwroot/              # Static files (CSS, JS, images)
├── appsettings.json                   # Config mặc định (LocalDB)
├── appsettings.Development.json       # Config override cá nhân (bị gitignore)
├── appsettings.Development.json.example  # Template để tạo file trên
└── Program.cs            # Entry point, DI, middleware, DB seed
```

---

## Chức năng chính

- **Dashboard** — Thống kê tổng quan, biểu đồ nhân viên theo phòng ban
- **Nhân viên** — CRUD, tìm kiếm, lọc theo phòng ban, phân trang
- **Phòng ban** — Quản lý phòng ban, xem số lượng nhân viên
- **Hợp đồng** — Quản lý hợp đồng, trạng thái còn hạn / hết hạn
- **Đánh giá** — Đánh giá nhân viên theo kỳ, điểm và nhận xét
- **Nghỉ phép** — Quản lý đơn nghỉ phép
- **Tuyển dụng** — Quản lý hồ sơ ứng viên
- **Báo cáo** — Thống kê theo phòng ban
- **Admin** — Quản lý tài khoản, phân quyền vai trò

---

## Phân quyền

| Role | Quyền |
|---|---|
| `Admin` | Toàn quyền: quản lý user, tất cả module |
| `Manager` | Quản lý nhân viên trong phòng ban, hợp đồng, đánh giá |
| `User` | Xem thông tin cá nhân, đơn nghỉ phép của mình |

---

## Troubleshooting

**Lỗi kết nối database:**
- Kiểm tra LocalDB: `sqllocaldb start MSSQLLocalDB`
- Hoặc tạo `appsettings.Development.json` và điền đúng connection string

**Lỗi migration:**
- Xóa database và chạy lại: `dotnet ef database drop --force && dotnet run`
- Hoặc trong môi trường Development, app tự xử lý khi phát hiện schema lệch
