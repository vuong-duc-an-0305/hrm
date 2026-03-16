namespace QuanLyNhanSu.ViewModels
{
    public class PhanQuyenViewModel
    {
        public int MaNguoiDung { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string? HoTen { get; set; }
        public int? MaVaiTro { get; set; }
        public string? TenVaiTro { get; set; }
        public int? MaPhongBan { get; set; }
        public string? TenPhongBan { get; set; }
        public List<VaiTroItem> DanhSachVaiTro { get; set; } = new();
        public List<PhongBanItem> DanhSachPhongBan { get; set; } = new();
    }

    public class VaiTroItem
    {
        public int MaVaiTro { get; set; }
        public string TenVaiTro { get; set; } = string.Empty;
    }

    public class PhongBanItem
    {
        public int MaPhongBan { get; set; }
        public string TenPhongBan { get; set; } = string.Empty;
    }
}
