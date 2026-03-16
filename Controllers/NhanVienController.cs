using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Models;

namespace QuanLyNhanSu.Controllers
{
    [Authorize]
    public class NhanVienController : Controller
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

        public NhanVienController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: NhanVien
        public async Task<IActionResult> Index(string? search, int? phongBanId, bool nhanVienMoi = false, int page = 1)
        {
            int pageSize = 10;
            var query = _context.NhanViens.Include(n => n.PhongBan).AsQueryable();
            var managerPhongBanId = GetManagerPhongBanId();

            if (managerPhongBanId.HasValue)
            {
                query = query.Where(n => n.MaPhongBan == managerPhongBanId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(n => n.HoTen.Contains(search) || n.MaNhanVien.ToString().Contains(search));
                ViewData["Search"] = search;
            }

            if (phongBanId.HasValue)
            {
                query = query.Where(n => n.MaPhongBan == phongBanId);
                ViewData["PhongBanId"] = phongBanId;
            }
            else if (managerPhongBanId.HasValue)
            {
                ViewData["PhongBanId"] = managerPhongBanId.Value;
            }

            if (nhanVienMoi)
            {
                var fromDate = DateTime.Today.AddDays(-30);
                query = query.Where(n => n.NgayVaoLam.HasValue && n.NgayVaoLam.Value >= fromDate);
                ViewData["NhanVienMoi"] = true;
            }

            var phongBans = managerPhongBanId.HasValue
                ? await _context.PhongBans.Where(p => p.MaPhongBan == managerPhongBanId.Value).ToListAsync()
                : await _context.PhongBans.ToListAsync();
            ViewData["PhongBans"] = new SelectList(phongBans, "MaPhongBan", "TenPhongBan");

            var totalItems = await query.CountAsync();
            var nhanViens = await query
                .OrderBy(n => n.HoTen)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(nhanViens);
        }

        // GET: NhanVien/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var nhanVien = await _context.NhanViens
                .Include(n => n.PhongBan)
                .Include(n => n.HopDongs)
                .Include(n => n.DanhGias)
                .FirstOrDefaultAsync(n => n.MaNhanVien == id);

            if (nhanVien == null) return NotFound();

            var managerPhongBanId = GetManagerPhongBanId();
            if (managerPhongBanId.HasValue && nhanVien.MaPhongBan != managerPhongBanId.Value)
                return Forbid();

            return View(nhanVien);
        }

        // GET: NhanVien/Create
        public async Task<IActionResult> Create()
        {
            var managerPhongBanId = GetManagerPhongBanId();
            var phongBans = managerPhongBanId.HasValue
                ? await _context.PhongBans.Where(p => p.MaPhongBan == managerPhongBanId.Value).ToListAsync()
                : await _context.PhongBans.ToListAsync();
            ViewData["PhongBans"] = new SelectList(phongBans, "MaPhongBan", "TenPhongBan", managerPhongBanId);
            return View();
        }

        // POST: NhanVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhanVien nhanVien)
        {
            var managerPhongBanId = GetManagerPhongBanId();
            if (managerPhongBanId.HasValue)
                nhanVien.MaPhongBan = managerPhongBanId.Value;

            if (ModelState.IsValid)
            {
                _context.Add(nhanVien);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm nhân viên thành công!";
                return RedirectToAction(nameof(Index));
            }

            var phongBans = managerPhongBanId.HasValue
                ? await _context.PhongBans.Where(p => p.MaPhongBan == managerPhongBanId.Value).ToListAsync()
                : await _context.PhongBans.ToListAsync();
            ViewData["PhongBans"] = new SelectList(phongBans, "MaPhongBan", "TenPhongBan", nhanVien.MaPhongBan);
            return View(nhanVien);
        }

        // GET: NhanVien/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null) return NotFound();

            var managerPhongBanId = GetManagerPhongBanId();
            if (managerPhongBanId.HasValue && nhanVien.MaPhongBan != managerPhongBanId.Value)
                return Forbid();

            var phongBans = managerPhongBanId.HasValue
                ? await _context.PhongBans.Where(p => p.MaPhongBan == managerPhongBanId.Value).ToListAsync()
                : await _context.PhongBans.ToListAsync();
            ViewData["PhongBans"] = new SelectList(phongBans, "MaPhongBan", "TenPhongBan", nhanVien.MaPhongBan);
            return View(nhanVien);
        }

        // POST: NhanVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NhanVien nhanVien)
        {
            if (id != nhanVien.MaNhanVien) return NotFound();

            var managerPhongBanId = GetManagerPhongBanId();
            if (managerPhongBanId.HasValue)
            {
                var existing = await _context.NhanViens.AsNoTracking().FirstOrDefaultAsync(n => n.MaNhanVien == id);
                if (existing == null || existing.MaPhongBan != managerPhongBanId.Value)
                    return Forbid();

                nhanVien.MaPhongBan = managerPhongBanId.Value;
            }

            if (ModelState.IsValid)
            {
                _context.Update(nhanVien);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật nhân viên thành công!";
                return RedirectToAction(nameof(Index));
            }

            var phongBans = managerPhongBanId.HasValue
                ? await _context.PhongBans.Where(p => p.MaPhongBan == managerPhongBanId.Value).ToListAsync()
                : await _context.PhongBans.ToListAsync();
            ViewData["PhongBans"] = new SelectList(phongBans, "MaPhongBan", "TenPhongBan", nhanVien.MaPhongBan);
            return View(nhanVien);
        }

        // POST: NhanVien/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien != null)
            {
                var managerPhongBanId = GetManagerPhongBanId();
                if (managerPhongBanId.HasValue && nhanVien.MaPhongBan != managerPhongBanId.Value)
                    return Forbid();

                _context.NhanViens.Remove(nhanVien);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa nhân viên thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // API cho Ajax/Fetch - Tìm kiếm nhân viên
        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var managerPhongBanId = GetManagerPhongBanId();

            var results = await _context.NhanViens
                .Include(n => n.PhongBan)
                .Where(n => n.HoTen.Contains(term) || n.MaNhanVien.ToString().Contains(term))
                .Where(n => !managerPhongBanId.HasValue || n.MaPhongBan == managerPhongBanId.Value)
                .Select(n => new
                {
                    n.MaNhanVien,
                    n.HoTen,
                    n.Email,
                    n.ChucVu,
                    PhongBan = n.PhongBan != null ? n.PhongBan.TenPhongBan : "Chưa phân"
                })
                .Take(10)
                .ToListAsync();

            return Json(results);
        }

        // API cho Ajax/Fetch - Xóa nhân viên
        [HttpDelete]
        public async Task<IActionResult> DeleteApi(int id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null)
                return Json(new { success = false, message = "Không tìm thấy nhân viên" });

            var managerPhongBanId = GetManagerPhongBanId();
            if (managerPhongBanId.HasValue && nhanVien.MaPhongBan != managerPhongBanId.Value)
                return StatusCode(403, new { success = false, message = "Không có quyền xóa nhân viên phòng ban khác" });

            _context.NhanViens.Remove(nhanVien);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Xóa nhân viên thành công!" });
        }

        // API: Create NhanVien (Ajax)
        [HttpPost]
        public async Task<IActionResult> CreateApi([FromBody] NhanVien nhanVien)
        {
            var managerPhongBanId = GetManagerPhongBanId();
            if (managerPhongBanId.HasValue)
                nhanVien.MaPhongBan = managerPhongBanId.Value;

            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });

            _context.Add(nhanVien);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Thêm nhân viên thành công!", id = nhanVien.MaNhanVien });
        }

        // API: Edit NhanVien (Ajax)
        [HttpPut]
        public async Task<IActionResult> EditApi(int id, [FromBody] NhanVien nhanVien)
        {
            if (id != nhanVien.MaNhanVien)
                return BadRequest(new { success = false, message = "ID không khớp" });

            var existing = await _context.NhanViens.FindAsync(id);
            if (existing == null)
                return NotFound(new { success = false, message = "Không tìm thấy nhân viên" });

            var managerPhongBanId = GetManagerPhongBanId();
            if (managerPhongBanId.HasValue && existing.MaPhongBan != managerPhongBanId.Value)
                return StatusCode(403, new { success = false, message = "Không có quyền sửa nhân viên phòng ban khác" });

            existing.HoTen = nhanVien.HoTen;
            existing.NgaySinh = nhanVien.NgaySinh;
            existing.GioiTinh = nhanVien.GioiTinh;
            existing.DiaChi = nhanVien.DiaChi;
            existing.SoDienThoai = nhanVien.SoDienThoai;
            existing.Email = nhanVien.Email;
            existing.ChucVu = nhanVien.ChucVu;
            existing.MaPhongBan = managerPhongBanId ?? nhanVien.MaPhongBan;
            existing.NgayVaoLam = nhanVien.NgayVaoLam;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Cập nhật nhân viên thành công!" });
        }

        // API: Filter NhanVien (Ajax) - danh sách có phân trang
        [HttpGet]
        public async Task<IActionResult> FilterApi(string? search, int? phongBanId, bool nhanVienMoi = false, int page = 1)
        {
            int pageSize = 10;
            var query = _context.NhanViens.Include(n => n.PhongBan).AsQueryable();
            var managerPhongBanId = GetManagerPhongBanId();

            if (managerPhongBanId.HasValue)
                query = query.Where(n => n.MaPhongBan == managerPhongBanId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(n => n.HoTen.Contains(search) || n.MaNhanVien.ToString().Contains(search));
            if (phongBanId.HasValue)
                query = query.Where(n => n.MaPhongBan == phongBanId);
            if (nhanVienMoi)
            {
                var fromDate = DateTime.Today.AddDays(-30);
                query = query.Where(n => n.NgayVaoLam.HasValue && n.NgayVaoLam.Value >= fromDate);
            }

            var totalItems = await query.CountAsync();
            var data = await query.OrderBy(n => n.HoTen)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(n => new {
                    n.MaNhanVien, n.HoTen, n.Email, n.ChucVu,
                    NgayOnboard = n.NgayVaoLam.HasValue ? n.NgayVaoLam.Value.ToString("dd/MM/yyyy") : "",
                    PhongBan = n.PhongBan != null ? n.PhongBan.TenPhongBan : "Chưa phân"
                }).ToListAsync();

            return Json(new { data, totalPages = (int)Math.Ceiling((double)totalItems / pageSize), currentPage = page });
        }
    }
}
