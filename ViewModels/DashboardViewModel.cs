namespace QuanLyNhanSu.ViewModels
{
    public class DashboardViewModel
    {
        public int TongNhanVien { get; set; }
        public int TongPhongBan { get; set; }
        public int TongHopDong { get; set; }
        public int HopDongConHan { get; set; }
        public int HopDongHetHan { get; set; }
        public int HopDongSapHetHan { get; set; }
        public int TongNguoiDung { get; set; }
        public int TongDonNghiPhep { get; set; }
        public int DonNghiPhepChoDuyet { get; set; }
        public int DonNghiPhepDaDuyet { get; set; }
        public int ViTriDangTuyen { get; set; }
        public int ViTriDaTuyenDu { get; set; }
        public int ViTriDungTuyen { get; set; }
        public int NhanVienMoiThangNay { get; set; }
        public List<PhongBanThongKeViewModel> ThongKePhongBan { get; set; } = new();
    }

    public class PhongBanThongKeViewModel
    {
        public string TenPhongBan { get; set; } = string.Empty;
        public int SoNhanVien { get; set; }
    }
}
