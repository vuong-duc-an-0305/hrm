using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Models;

namespace QuanLyNhanSu.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<PhongBan> PhongBans { get; set; }
        public DbSet<HopDong> HopDongs { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }
        public DbSet<NghiPhep> NghiPheps { get; set; }
        public DbSet<TuyenDung> TuyenDungs { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<VaiTro> VaiTros { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraint cho TenDangNhap
            modelBuilder.Entity<NguoiDung>()
                .HasIndex(n => n.TenDangNhap)
                .IsUnique();

            // Seed VaiTro
            modelBuilder.Entity<VaiTro>().HasData(
                new VaiTro { MaVaiTro = 1, TenVaiTro = "Admin", MoTa = "Quản trị hệ thống" },
                new VaiTro { MaVaiTro = 2, TenVaiTro = "Manager", MoTa = "Quản lý" },
                new VaiTro { MaVaiTro = 3, TenVaiTro = "User", MoTa = "Nhân viên" }
            );

            // Seed NguoiDung admin (password: Admin@123 - hashed with BCrypt sẽ thêm sau)
            modelBuilder.Entity<NguoiDung>().HasData(
                new NguoiDung
                {
                    MaNguoiDung = 1,
                    TenDangNhap = "admin",
                    MatKhau = "admin",
                    HoTen = "Administrator",
                    Email = "admin@company.com",
                    TrangThai = true,
                    NgayTao = new DateTime(2024, 1, 1),
                    MaVaiTro = 1
                }
            );

            // Seed PhongBan
            modelBuilder.Entity<PhongBan>().HasData(
                new PhongBan { MaPhongBan = 1, TenPhongBan = "Phòng Nhân sự", MoTa = "Quản lý nhân sự", NgayTao = new DateTime(2024, 1, 1) },
                new PhongBan { MaPhongBan = 2, TenPhongBan = "Phòng Kỹ thuật", MoTa = "Phát triển phần mềm", NgayTao = new DateTime(2024, 1, 1) },
                new PhongBan { MaPhongBan = 3, TenPhongBan = "Phòng Kinh doanh", MoTa = "Kinh doanh và bán hàng", NgayTao = new DateTime(2024, 1, 1) }
            );
        }
    }
}
