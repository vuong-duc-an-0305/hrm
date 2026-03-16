using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Models;

namespace QuanLyNhanSu.Controllers
{
    [Authorize]
    public class NghiPhepController : Controller
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

        public NghiPhepController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: NghiPhep
        public async Task<IActionResult> Index(string? search, string? trangThai)
        {
            var query = _context.NghiPheps
                .Include(n => n.NhanVien)
                .AsQueryable();
            var managerPhongBanId = GetManagerPhongBanId();

            if (managerPhongBanId.HasValue)
                query = query.Where(n => n.NhanVien != null && n.NhanVien.MaPhongBan == managerPhongBanId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(n => n.NhanVien != null && n.NhanVien.HoTen.Contains(search));
                ViewData["Search"] = search;
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(n => n.TrangThai == trangThai);
                ViewData["TrangThai"] = trangThai;
            }

            var data = await query
                .OrderByDescending(n => n.NgayTao)
                .ToListAsync();

            return View(data);
        }

        // GET: NghiPhep/Create
        public async Task<IActionResult> Create()
        {
            var managerPhongBanId = GetManagerPhongBanId();
            var nhanViensQuery = _context.NhanViens.AsQueryable();
            if (managerPhongBanId.HasValue)
                nhanViensQuery = nhanViensQuery.Where(n => n.MaPhongBan == managerPhongBanId.Value);

            ViewData["NhanViens"] = new SelectList(await nhanViensQuery.OrderBy(n => n.HoTen).ToListAsync(), "MaNhanVien", "HoTen");
            return View(new NghiPhep
            {
                TuNgay = DateTime.Today,
                DenNgay = DateTime.Today,
                TrangThai = "Chờ duyệt"
            });
        }

        // POST: NghiPhep/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NghiPhep nghiPhep)
        {
            var managerPhongBanId = GetManagerPhongBanId();

            if (nghiPhep.DenNgay.Date < nghiPhep.TuNgay.Date)
            {
                ModelState.AddModelError(nameof(NghiPhep.DenNgay), "Đến ngày phải lớn hơn hoặc bằng Từ ngày");
            }

            if (managerPhongBanId.HasValue)
            {
                var isValidNhanVien = await _context.NhanViens.AnyAsync(n => n.MaNhanVien == nghiPhep.MaNhanVien && n.MaPhongBan == managerPhongBanId.Value);
                if (!isValidNhanVien)
                    return Forbid();
            }

            if (ModelState.IsValid)
            {
                nghiPhep.NgayTao = DateTime.Now;
                _context.Add(nghiPhep);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tạo đơn nghỉ phép thành công!";
                return RedirectToAction(nameof(Index));
            }

            var nhanViensQuery = _context.NhanViens.AsQueryable();
            if (managerPhongBanId.HasValue)
                nhanViensQuery = nhanViensQuery.Where(n => n.MaPhongBan == managerPhongBanId.Value);
            ViewData["NhanViens"] = new SelectList(await nhanViensQuery.OrderBy(n => n.HoTen).ToListAsync(), "MaNhanVien", "HoTen", nghiPhep.MaNhanVien);
            return View(nghiPhep);
        }

        // GET: NghiPhep/Edit/5
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nghiPhep = await _context.NghiPheps.FindAsync(id);
            if (nghiPhep == null)
            {
                return NotFound();
            }

            var managerPhongBanId = GetManagerPhongBanId();
            if (managerPhongBanId.HasValue)
            {
                var isAllowed = await _context.NhanViens.AnyAsync(n => n.MaNhanVien == nghiPhep.MaNhanVien && n.MaPhongBan == managerPhongBanId.Value);
                if (!isAllowed)
                    return Forbid();
            }

            var nhanViensQuery = _context.NhanViens.AsQueryable();
            if (managerPhongBanId.HasValue)
                nhanViensQuery = nhanViensQuery.Where(n => n.MaPhongBan == managerPhongBanId.Value);
            ViewData["NhanViens"] = new SelectList(await nhanViensQuery.OrderBy(n => n.HoTen).ToListAsync(), "MaNhanVien", "HoTen", nghiPhep.MaNhanVien);
            return View(nghiPhep);
        }

        // POST: NghiPhep/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> Edit(int id, NghiPhep nghiPhep)
        {
            if (id != nghiPhep.MaNghiPhep)
            {
                return NotFound();
            }

            var managerPhongBanId = GetManagerPhongBanId();

            if (nghiPhep.DenNgay.Date < nghiPhep.TuNgay.Date)
            {
                ModelState.AddModelError(nameof(NghiPhep.DenNgay), "Đến ngày phải lớn hơn hoặc bằng Từ ngày");
            }

            var existing = await _context.NghiPheps.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (managerPhongBanId.HasValue)
            {
                var isAllowed = await _context.NhanViens.AnyAsync(n => n.MaNhanVien == existing.MaNhanVien && n.MaPhongBan == managerPhongBanId.Value)
                                && await _context.NhanViens.AnyAsync(n => n.MaNhanVien == nghiPhep.MaNhanVien && n.MaPhongBan == managerPhongBanId.Value);
                if (!isAllowed)
                    return Forbid();
            }

            if (existing.TrangThai != "Chờ duyệt")
            {
                TempData["Error"] = "Chỉ có thể chỉnh sửa đơn đang chờ duyệt";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                existing.MaNhanVien = nghiPhep.MaNhanVien;
                existing.TuNgay = nghiPhep.TuNgay;
                existing.DenNgay = nghiPhep.DenNgay;
                existing.LyDo = nghiPhep.LyDo;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật đơn nghỉ phép thành công!";
                return RedirectToAction(nameof(Index));
            }

            var nhanViensQuery = _context.NhanViens.AsQueryable();
            if (managerPhongBanId.HasValue)
                nhanViensQuery = nhanViensQuery.Where(n => n.MaPhongBan == managerPhongBanId.Value);
            ViewData["NhanViens"] = new SelectList(await nhanViensQuery.OrderBy(n => n.HoTen).ToListAsync(), "MaNhanVien", "HoTen", nghiPhep.MaNhanVien);
            return View(nghiPhep);
        }

        // POST: NghiPhep/UpdateTrangThai/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> UpdateTrangThai(int id, string trangThai)
        {
            var validStatuses = new[] { "Chờ duyệt", "Đã duyệt", "Từ chối" };
            if (!validStatuses.Contains(trangThai))
            {
                TempData["Error"] = "Trạng thái không hợp lệ";
                return RedirectToAction(nameof(Index));
            }

            var nghiPhep = await _context.NghiPheps.FindAsync(id);
            if (nghiPhep == null)
            {
                TempData["Error"] = "Không tìm thấy đơn nghỉ phép";
                return RedirectToAction(nameof(Index));
            }

            var managerPhongBanId = GetManagerPhongBanId();
            if (managerPhongBanId.HasValue)
            {
                var isAllowed = await _context.NhanViens.AnyAsync(n => n.MaNhanVien == nghiPhep.MaNhanVien && n.MaPhongBan == managerPhongBanId.Value);
                if (!isAllowed)
                    return Forbid();
            }

            nghiPhep.TrangThai = trangThai;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật trạng thái thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: NghiPhep/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var nghiPhep = await _context.NghiPheps.FindAsync(id);
            if (nghiPhep != null)
            {
                var managerPhongBanId = GetManagerPhongBanId();
                if (managerPhongBanId.HasValue)
                {
                    var isAllowed = await _context.NhanViens.AnyAsync(n => n.MaNhanVien == nghiPhep.MaNhanVien && n.MaPhongBan == managerPhongBanId.Value);
                    if (!isAllowed)
                        return Forbid();
                }

                _context.NghiPheps.Remove(nghiPhep);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa đơn nghỉ phép thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
