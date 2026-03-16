using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.ViewModels;

namespace QuanLyNhanSu.Controllers
{
    [Authorize(Policy = "NotManager")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        private int? GetManagerPhongBanId()
        {
            if (User.IsInRole("Admin") || !User.IsInRole("Manager"))
                return null;

            var claim = User.FindFirst("MaPhongBan")?.Value;
            if (int.TryParse(claim, out var phongBanId))
                return phongBanId;

            return null;
        }

        public DashboardController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            var managerPhongBanId = GetManagerPhongBanId();
            var cacheKey = managerPhongBanId.HasValue ? $"dashboard_data_manager_{managerPhongBanId.Value}" : "dashboard_data";
            if (!_cache.TryGetValue(cacheKey, out DashboardViewModel? model))
            {
                var now = DateTime.Now;
                var currentMonthStart = new DateTime(now.Year, now.Month, 1);
                var upcomingContractDate = now.Date.AddDays(30);
                var nhanVienQuery = _context.NhanViens.AsQueryable();
                var hopDongQuery = _context.HopDongs.Include(h => h.NhanVien).AsQueryable();
                var nghiPhepQuery = _context.NghiPheps.Include(n => n.NhanVien).AsQueryable();
                var tuyenDungQuery = _context.TuyenDungs.AsQueryable();
                var phongBanQuery = _context.PhongBans.AsQueryable();

                if (managerPhongBanId.HasValue)
                {
                    nhanVienQuery = nhanVienQuery.Where(n => n.MaPhongBan == managerPhongBanId.Value);
                    hopDongQuery = hopDongQuery.Where(h => h.NhanVien != null && h.NhanVien.MaPhongBan == managerPhongBanId.Value);
                    nghiPhepQuery = nghiPhepQuery.Where(n => n.NhanVien != null && n.NhanVien.MaPhongBan == managerPhongBanId.Value);
                    tuyenDungQuery = tuyenDungQuery.Where(t => t.MaPhongBan == managerPhongBanId.Value);
                    phongBanQuery = phongBanQuery.Where(p => p.MaPhongBan == managerPhongBanId.Value);
                }

                model = new DashboardViewModel
                {
                    TongNhanVien = await nhanVienQuery.CountAsync(),
                    TongPhongBan = await phongBanQuery.CountAsync(),
                    TongHopDong = await hopDongQuery.CountAsync(),
                    HopDongConHan = await hopDongQuery
                        .CountAsync(h => !h.NgayKetThuc.HasValue || h.NgayKetThuc.Value >= now),
                    HopDongHetHan = await hopDongQuery
                        .CountAsync(h => h.NgayKetThuc.HasValue && h.NgayKetThuc.Value < now),
                    HopDongSapHetHan = await hopDongQuery
                        .CountAsync(h => h.NgayKetThuc.HasValue
                                         && h.NgayKetThuc.Value >= now.Date
                                         && h.NgayKetThuc.Value <= upcomingContractDate),
                    TongNguoiDung = await _context.NguoiDungs.CountAsync(),
                    TongDonNghiPhep = await nghiPhepQuery.CountAsync(),
                    DonNghiPhepChoDuyet = await nghiPhepQuery.CountAsync(n => n.TrangThai == "Chờ duyệt"),
                    DonNghiPhepDaDuyet = await nghiPhepQuery.CountAsync(n => n.TrangThai == "Đã duyệt"),
                    ViTriDangTuyen = await tuyenDungQuery.CountAsync(t => t.TrangThai == "DangTuyen"),
                    ViTriDaTuyenDu = await tuyenDungQuery.CountAsync(t => t.TrangThai == "DaTuyenDu"),
                    ViTriDungTuyen = await tuyenDungQuery.CountAsync(t => t.TrangThai == "DungTuyen"),
                    NhanVienMoiThangNay = await nhanVienQuery.CountAsync(n => n.NgayVaoLam.HasValue && n.NgayVaoLam.Value >= currentMonthStart),
                    ThongKePhongBan = await phongBanQuery
                        .Select(p => new PhongBanThongKeViewModel
                        {
                            TenPhongBan = p.TenPhongBan,
                            SoNhanVien = p.NhanViens.Count
                        })
                        .OrderByDescending(p => p.SoNhanVien)
                        .ToListAsync()
                };

                _cache.Set(cacheKey, model, TimeSpan.FromMinutes(5));
            }

            return View(model);
        }

        // API Fetch - Lấy thống kê Dashboard (JSON)
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            var now = DateTime.Now;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
            var upcomingContractDate = now.Date.AddDays(30);
            var managerPhongBanId = GetManagerPhongBanId();

            var nhanVienQuery = _context.NhanViens.AsQueryable();
            var hopDongQuery = _context.HopDongs.Include(h => h.NhanVien).AsQueryable();
            var nghiPhepQuery = _context.NghiPheps.Include(n => n.NhanVien).AsQueryable();
            var tuyenDungQuery = _context.TuyenDungs.AsQueryable();
            var phongBanQuery = _context.PhongBans.AsQueryable();

            if (managerPhongBanId.HasValue)
            {
                nhanVienQuery = nhanVienQuery.Where(n => n.MaPhongBan == managerPhongBanId.Value);
                hopDongQuery = hopDongQuery.Where(h => h.NhanVien != null && h.NhanVien.MaPhongBan == managerPhongBanId.Value);
                nghiPhepQuery = nghiPhepQuery.Where(n => n.NhanVien != null && n.NhanVien.MaPhongBan == managerPhongBanId.Value);
                tuyenDungQuery = tuyenDungQuery.Where(t => t.MaPhongBan == managerPhongBanId.Value);
                phongBanQuery = phongBanQuery.Where(p => p.MaPhongBan == managerPhongBanId.Value);
            }

            var data = new
            {
                tongNhanVien = await nhanVienQuery.CountAsync(),
                tongPhongBan = await phongBanQuery.CountAsync(),
                tongNguoiDung = await _context.NguoiDungs.CountAsync(),
                hopDongConHan = await hopDongQuery.CountAsync(h => !h.NgayKetThuc.HasValue || h.NgayKetThuc.Value >= now),
                hopDongHetHan = await hopDongQuery.CountAsync(h => h.NgayKetThuc.HasValue && h.NgayKetThuc.Value < now),
                hopDongSapHetHan = await hopDongQuery.CountAsync(h => h.NgayKetThuc.HasValue
                    && h.NgayKetThuc.Value >= now.Date
                    && h.NgayKetThuc.Value <= upcomingContractDate),
                tongDonNghiPhep = await nghiPhepQuery.CountAsync(),
                donNghiPhepChoDuyet = await nghiPhepQuery.CountAsync(n => n.TrangThai == "Chờ duyệt"),
                donNghiPhepDaDuyet = await nghiPhepQuery.CountAsync(n => n.TrangThai == "Đã duyệt"),
                viTriDangTuyen = await tuyenDungQuery.CountAsync(t => t.TrangThai == "DangTuyen"),
                viTriDaTuyenDu = await tuyenDungQuery.CountAsync(t => t.TrangThai == "DaTuyenDu"),
                viTriDungTuyen = await tuyenDungQuery.CountAsync(t => t.TrangThai == "DungTuyen"),
                nhanVienMoiThangNay = await nhanVienQuery.CountAsync(n => n.NgayVaoLam.HasValue && n.NgayVaoLam.Value >= currentMonthStart),
                thongKePhongBan = await phongBanQuery
                    .Select(p => new { p.TenPhongBan, soNhanVien = p.NhanViens.Count })
                    .OrderByDescending(p => p.soNhanVien)
                    .ToListAsync()
            };
            return Json(data);
        }
    }
}
