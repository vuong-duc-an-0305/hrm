using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Models;

namespace QuanLyNhanSu.Controllers
{
    [Authorize]
    public class HopDongController : Controller
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

        public HopDongController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: HopDong
        public async Task<IActionResult> Index(string? search, bool sapHetHan = false)
        {
            var query = _context.HopDongs.Include(h => h.NhanVien).AsQueryable();
            var managerPhongBanId = GetManagerPhongBanId();

            if (managerPhongBanId.HasValue)
                query = query.Where(h => h.NhanVien != null && h.NhanVien.MaPhongBan == managerPhongBanId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(h => h.NhanVien != null && h.NhanVien.HoTen.Contains(search));
                ViewData["Search"] = search;
            }

            if (sapHetHan)
            {
                var now = DateTime.Today;
                var deadline = now.AddDays(30);
                query = query.Where(h => h.NgayKetThuc.HasValue
                                         && h.NgayKetThuc.Value >= now
                                         && h.NgayKetThuc.Value <= deadline);
                ViewData["SapHetHan"] = true;
            }

            var hopDongs = await query.OrderByDescending(h => h.NgayBatDau).ToListAsync();
            return View(hopDongs);
        }

        // GET: HopDong/Create
        public async Task<IActionResult> Create()
        {
            var managerPhongBanId = GetManagerPhongBanId();
            var nhanVienQuery = _context.NhanViens.AsQueryable();
            if (managerPhongBanId.HasValue)
                nhanVienQuery = nhanVienQuery.Where(n => n.MaPhongBan == managerPhongBanId.Value);

            ViewData["NhanViens"] = new SelectList(await nhanVienQuery.ToListAsync(), "MaNhanVien", "HoTen");
            return View();
        }

        // POST: HopDong/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HopDong hopDong)
        {
            var managerPhongBanId = GetManagerPhongBanId();
            if (managerPhongBanId.HasValue)
            {
                var isAllowed = await _context.NhanViens.AnyAsync(n => n.MaNhanVien == hopDong.MaNhanVien && n.MaPhongBan == managerPhongBanId.Value);
                if (!isAllowed)
                    return Forbid();
            }

            if (ModelState.IsValid)
            {
                _context.Add(hopDong);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm hợp đồng thành công!";
                return RedirectToAction(nameof(Index));
            }

            var nhanVienQuery = _context.NhanViens.AsQueryable();
            if (managerPhongBanId.HasValue)
                nhanVienQuery = nhanVienQuery.Where(n => n.MaPhongBan == managerPhongBanId.Value);
            ViewData["NhanViens"] = new SelectList(await nhanVienQuery.ToListAsync(), "MaNhanVien", "HoTen", hopDong.MaNhanVien);
            return View(hopDong);
        }

        // POST: HopDong/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var hopDong = await _context.HopDongs.FindAsync(id);
            if (hopDong != null)
            {
                var managerPhongBanId = GetManagerPhongBanId();
                if (managerPhongBanId.HasValue)
                {
                    var isAllowed = await _context.NhanViens.AnyAsync(n => n.MaNhanVien == hopDong.MaNhanVien && n.MaPhongBan == managerPhongBanId.Value);
                    if (!isAllowed)
                        return Forbid();
                }

                _context.HopDongs.Remove(hopDong);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa hợp đồng thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // API: Search HopDong (Ajax)
        [HttpGet]
        public async Task<IActionResult> SearchApi(string? search, bool sapHetHan = false)
        {
            var query = _context.HopDongs.Include(h => h.NhanVien).AsQueryable();
            var managerPhongBanId = GetManagerPhongBanId();
            if (managerPhongBanId.HasValue)
                query = query.Where(h => h.NhanVien != null && h.NhanVien.MaPhongBan == managerPhongBanId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(h => h.NhanVien != null && h.NhanVien.HoTen.Contains(search));
            if (sapHetHan)
            {
                var now = DateTime.Today;
                var deadline = now.AddDays(30);
                query = query.Where(h => h.NgayKetThuc.HasValue
                                         && h.NgayKetThuc.Value >= now
                                         && h.NgayKetThuc.Value <= deadline);
            }

            var data = await query.OrderByDescending(h => h.NgayBatDau)
                .Select(h => new {
                    h.MaHopDong,
                    NhanVien = h.NhanVien != null ? h.NhanVien.HoTen : "",
                    h.LoaiHopDong,
                    NgayBatDau = h.NgayBatDau.ToString("dd/MM/yyyy"),
                    NgayKetThuc = h.NgayKetThuc.HasValue ? h.NgayKetThuc.Value.ToString("dd/MM/yyyy") : "",
                    h.TrangThai
                }).ToListAsync();
            return Json(data);
        }

        // API: Create HopDong (Ajax)
        [HttpPost]
        public async Task<IActionResult> CreateApi([FromBody] HopDong hopDong)
        {
            var managerPhongBanId = GetManagerPhongBanId();
            if (managerPhongBanId.HasValue)
            {
                var isAllowed = await _context.NhanViens.AnyAsync(n => n.MaNhanVien == hopDong.MaNhanVien && n.MaPhongBan == managerPhongBanId.Value);
                if (!isAllowed)
                    return StatusCode(403, new { success = false, message = "Không có quyền tạo hợp đồng cho phòng ban khác" });
            }

            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });

            _context.Add(hopDong);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Thêm hợp đồng thành công!", id = hopDong.MaHopDong });
        }

        // API: Delete HopDong (Ajax)
        [HttpDelete]
        public async Task<IActionResult> DeleteApi(int id)
        {
            var hopDong = await _context.HopDongs.FindAsync(id);
            if (hopDong == null)
                return NotFound(new { success = false, message = "Không tìm thấy hợp đồng" });

            var managerPhongBanId = GetManagerPhongBanId();
            if (managerPhongBanId.HasValue)
            {
                var isAllowed = await _context.NhanViens.AnyAsync(n => n.MaNhanVien == hopDong.MaNhanVien && n.MaPhongBan == managerPhongBanId.Value);
                if (!isAllowed)
                    return StatusCode(403, new { success = false, message = "Không có quyền xóa hợp đồng phòng ban khác" });
            }

            _context.HopDongs.Remove(hopDong);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Xóa hợp đồng thành công!" });
        }
    }
}
