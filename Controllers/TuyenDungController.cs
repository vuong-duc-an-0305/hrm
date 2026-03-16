using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Models;

namespace QuanLyNhanSu.Controllers
{
    [Authorize(Policy = "NotManager")]
    public class TuyenDungController : Controller
    {
        private readonly ApplicationDbContext _context;

        private int? GetManagerPhongBanId()
        {
            if (User.IsInRole("Admin") || !User.IsInRole("Manager"))
                return null;

            var claim = User.FindFirst("MaPhongBan")?.Value;
            if (int.TryParse(claim, out var phongBanId))
                return phongBanId;

            return null;
        }

        public TuyenDungController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TuyenDung
        public async Task<IActionResult> Index(string? search, string? trangThai)
        {
            var query = _context.TuyenDungs
                .Include(t => t.PhongBan)
                .AsQueryable();
            var managerPhongBanId = GetManagerPhongBanId();

            if (managerPhongBanId.HasValue)
                query = query.Where(t => t.MaPhongBan == managerPhongBanId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.ViTri.Contains(search));
                ViewData["Search"] = search;
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(t => t.TrangThai == trangThai);
                ViewData["TrangThai"] = trangThai;
            }

            var data = await query
                .OrderByDescending(t => t.NgayDangTin)
                .ToListAsync();

            return View(data);
        }

        // GET: TuyenDung/Create
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> Create()
        {
            var managerPhongBanId = GetManagerPhongBanId();
            var phongBansQuery = _context.PhongBans.AsQueryable();
            if (managerPhongBanId.HasValue)
                phongBansQuery = phongBansQuery.Where(p => p.MaPhongBan == managerPhongBanId.Value);

            ViewData["PhongBans"] = new SelectList(await phongBansQuery.OrderBy(p => p.TenPhongBan).ToListAsync(), "MaPhongBan", "TenPhongBan", managerPhongBanId);
            return View(new TuyenDung
            {
                NgayDangTin = DateTime.Today,
                HanNopHoSo = DateTime.Today.AddDays(30),
                TrangThai = "DangTuyen"
            });
        }

        // POST: TuyenDung/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> Create(TuyenDung tuyenDung)
        {
            var managerPhongBanId = GetManagerPhongBanId();
            if (managerPhongBanId.HasValue)
                tuyenDung.MaPhongBan = managerPhongBanId.Value;

            if (tuyenDung.HanNopHoSo.HasValue && tuyenDung.HanNopHoSo.Value.Date < tuyenDung.NgayDangTin.Date)
            {
                ModelState.AddModelError(nameof(TuyenDung.HanNopHoSo), "Hạn nộp hồ sơ phải lớn hơn hoặc bằng ngày đăng tin");
            }

            if (ModelState.IsValid)
            {
                _context.Add(tuyenDung);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tạo tin tuyển dụng thành công!";
                return RedirectToAction(nameof(Index));
            }

            var phongBansQuery = _context.PhongBans.AsQueryable();
            if (managerPhongBanId.HasValue)
                phongBansQuery = phongBansQuery.Where(p => p.MaPhongBan == managerPhongBanId.Value);
            ViewData["PhongBans"] = new SelectList(await phongBansQuery.OrderBy(p => p.TenPhongBan).ToListAsync(), "MaPhongBan", "TenPhongBan", tuyenDung.MaPhongBan);
            return View(tuyenDung);
        }

        // GET: TuyenDung/Edit/5
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tuyenDung = await _context.TuyenDungs.FindAsync(id);
            if (tuyenDung == null)
            {
                return NotFound();
            }

            var managerPhongBanId = GetManagerPhongBanId();
            if (managerPhongBanId.HasValue && tuyenDung.MaPhongBan != managerPhongBanId.Value)
                return Forbid();

            var phongBansQuery = _context.PhongBans.AsQueryable();
            if (managerPhongBanId.HasValue)
                phongBansQuery = phongBansQuery.Where(p => p.MaPhongBan == managerPhongBanId.Value);
            ViewData["PhongBans"] = new SelectList(await phongBansQuery.OrderBy(p => p.TenPhongBan).ToListAsync(), "MaPhongBan", "TenPhongBan", tuyenDung.MaPhongBan);
            return View(tuyenDung);
        }

        // POST: TuyenDung/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> Edit(int id, TuyenDung tuyenDung)
        {
            if (id != tuyenDung.MaTuyenDung)
            {
                return NotFound();
            }

            var managerPhongBanId = GetManagerPhongBanId();
            if (managerPhongBanId.HasValue)
            {
                var existing = await _context.TuyenDungs.AsNoTracking().FirstOrDefaultAsync(t => t.MaTuyenDung == id);
                if (existing == null || existing.MaPhongBan != managerPhongBanId.Value)
                    return Forbid();

                tuyenDung.MaPhongBan = managerPhongBanId.Value;
            }

            if (tuyenDung.HanNopHoSo.HasValue && tuyenDung.HanNopHoSo.Value.Date < tuyenDung.NgayDangTin.Date)
            {
                ModelState.AddModelError(nameof(TuyenDung.HanNopHoSo), "Hạn nộp hồ sơ phải lớn hơn hoặc bằng ngày đăng tin");
            }

            if (ModelState.IsValid)
            {
                _context.Update(tuyenDung);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật tin tuyển dụng thành công!";
                return RedirectToAction(nameof(Index));
            }

            var phongBansQuery = _context.PhongBans.AsQueryable();
            if (managerPhongBanId.HasValue)
                phongBansQuery = phongBansQuery.Where(p => p.MaPhongBan == managerPhongBanId.Value);
            ViewData["PhongBans"] = new SelectList(await phongBansQuery.OrderBy(p => p.TenPhongBan).ToListAsync(), "MaPhongBan", "TenPhongBan", tuyenDung.MaPhongBan);
            return View(tuyenDung);
        }

        // POST: TuyenDung/UpdateTrangThai/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> UpdateTrangThai(int id, string trangThai)
        {
            var validStatuses = new[] { "DangTuyen", "DaTuyenDu", "DungTuyen" };
            if (!validStatuses.Contains(trangThai))
            {
                TempData["Error"] = "Trạng thái không hợp lệ";
                return RedirectToAction(nameof(Index));
            }

            var tuyenDung = await _context.TuyenDungs.FindAsync(id);
            if (tuyenDung == null)
            {
                TempData["Error"] = "Không tìm thấy tin tuyển dụng";
                return RedirectToAction(nameof(Index));
            }

            var managerPhongBanId = GetManagerPhongBanId();
            if (managerPhongBanId.HasValue && tuyenDung.MaPhongBan != managerPhongBanId.Value)
                return Forbid();

            tuyenDung.TrangThai = trangThai;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật trạng thái thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: TuyenDung/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var tuyenDung = await _context.TuyenDungs.FindAsync(id);
            if (tuyenDung != null)
            {
                var managerPhongBanId = GetManagerPhongBanId();
                if (managerPhongBanId.HasValue && tuyenDung.MaPhongBan != managerPhongBanId.Value)
                    return Forbid();

                _context.TuyenDungs.Remove(tuyenDung);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa tin tuyển dụng thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
