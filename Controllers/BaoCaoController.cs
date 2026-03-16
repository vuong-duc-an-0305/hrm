using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.ViewModels;

namespace QuanLyNhanSu.Controllers
{
    [Authorize(Policy = "NotManager")]
    public class BaoCaoController : Controller
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

        public BaoCaoController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: BaoCao/NhanVienTheoPhongBan
        public async Task<IActionResult> NhanVienTheoPhongBan(int? phongBanId)
        {
            var managerPhongBanId = GetManagerPhongBanId();
            if (managerPhongBanId.HasValue)
                phongBanId = managerPhongBanId.Value;

            var cacheKey = $"baocao_phongban_{phongBanId}";
            if (!_cache.TryGetValue(cacheKey, out List<PhongBanThongKeViewModel>? thongKe))
            {
                var query = _context.PhongBans.Include(p => p.NhanViens).AsQueryable();

                if (phongBanId.HasValue)
                    query = query.Where(p => p.MaPhongBan == phongBanId);

                thongKe = await query
                    .Select(p => new PhongBanThongKeViewModel
                    {
                        TenPhongBan = p.TenPhongBan,
                        SoNhanVien = p.NhanViens.Count
                    })
                    .OrderByDescending(p => p.SoNhanVien)
                    .ToListAsync();

                _cache.Set(cacheKey, thongKe, TimeSpan.FromMinutes(5));
            }

            var phongBanQuery = _context.PhongBans.AsQueryable();
            if (managerPhongBanId.HasValue)
                phongBanQuery = phongBanQuery.Where(p => p.MaPhongBan == managerPhongBanId.Value);

            ViewData["PhongBans"] = await phongBanQuery.ToListAsync();
            ViewData["PhongBanId"] = phongBanId;
            return View(thongKe);
        }

        // API cho Ajax
        [HttpGet]
        public async Task<IActionResult> GetThongKePhongBan(int? phongBanId)
        {
            var managerPhongBanId = GetManagerPhongBanId();
            if (managerPhongBanId.HasValue)
                phongBanId = managerPhongBanId.Value;

            var query = _context.PhongBans.AsQueryable();
            if (phongBanId.HasValue)
                query = query.Where(p => p.MaPhongBan == phongBanId);

            var data = await query
                .Select(p => new { p.TenPhongBan, SoNhanVien = p.NhanViens.Count })
                .OrderByDescending(p => p.SoNhanVien)
                .ToListAsync();

            return Json(data);
        }
    }
}
