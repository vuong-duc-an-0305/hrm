using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Models;

namespace QuanLyNhanSu.Controllers
{
    [Authorize(Policy = "NotManager")]
    public class PhongBanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PhongBanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PhongBan
        public async Task<IActionResult> Index()
        {
            var phongBans = await _context.PhongBans
                .Include(p => p.NhanViens)
                .OrderBy(p => p.TenPhongBan)
                .ToListAsync();

            return View(phongBans);
        }

        // GET: PhongBan/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PhongBan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhongBan phongBan)
        {
            if (ModelState.IsValid)
            {
                phongBan.NgayTao = DateTime.Now;
                _context.Add(phongBan);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm phòng ban thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(phongBan);
        }

        // GET: PhongBan/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var phongBan = await _context.PhongBans.FindAsync(id);
            if (phongBan == null) return NotFound();

            return View(phongBan);
        }

        // POST: PhongBan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PhongBan phongBan)
        {
            if (id != phongBan.MaPhongBan) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(phongBan);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật phòng ban thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(phongBan);
        }

        // POST: PhongBan/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var phongBan = await _context.PhongBans.Include(p => p.NhanViens).FirstOrDefaultAsync(p => p.MaPhongBan == id);
            if (phongBan != null)
            {
                if (phongBan.NhanViens.Any())
                {
                    TempData["Error"] = "Không thể xóa phòng ban đang có nhân viên!";
                    return RedirectToAction(nameof(Index));
                }
                _context.PhongBans.Remove(phongBan);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa phòng ban thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // API: Create PhongBan (Ajax)
        [HttpPost]
        public async Task<IActionResult> CreateApi([FromBody] PhongBan phongBan)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });

            phongBan.NgayTao = DateTime.Now;
            _context.Add(phongBan);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Thêm phòng ban thành công!", id = phongBan.MaPhongBan });
        }

        // API: Edit PhongBan (Ajax)
        [HttpPut]
        public async Task<IActionResult> EditApi(int id, [FromBody] PhongBan phongBan)
        {
            if (id != phongBan.MaPhongBan)
                return BadRequest(new { success = false, message = "ID không khớp" });

            var existing = await _context.PhongBans.FindAsync(id);
            if (existing == null)
                return NotFound(new { success = false, message = "Không tìm thấy phòng ban" });

            existing.TenPhongBan = phongBan.TenPhongBan;
            existing.MoTa = phongBan.MoTa;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Cập nhật phòng ban thành công!" });
        }

        // API: Delete PhongBan (Ajax)
        [HttpDelete]
        public async Task<IActionResult> DeleteApi(int id)
        {
            var phongBan = await _context.PhongBans.Include(p => p.NhanViens).FirstOrDefaultAsync(p => p.MaPhongBan == id);
            if (phongBan == null)
                return NotFound(new { success = false, message = "Không tìm thấy phòng ban" });

            if (phongBan.NhanViens.Any())
                return BadRequest(new { success = false, message = "Không thể xóa phòng ban đang có nhân viên!" });

            _context.PhongBans.Remove(phongBan);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Xóa phòng ban thành công!" });
        }
    }
}
