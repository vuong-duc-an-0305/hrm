using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Models;

namespace QuanLyNhanSu.Controllers
{
    [Authorize]
    public class DanhGiaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DanhGiaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DanhGia
        public async Task<IActionResult> Index()
        {
            var danhGias = await _context.DanhGias
                .Include(d => d.NhanVien)
                .OrderByDescending(d => d.NgayDanhGia)
                .ToListAsync();

            return View(danhGias);
        }

        // GET: DanhGia/Create
        public async Task<IActionResult> Create()
        {
            ViewData["NhanViens"] = new SelectList(await _context.NhanViens.ToListAsync(), "MaNhanVien", "HoTen");
            return View();
        }

        // POST: DanhGia/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhGia danhGia)
        {
            if (ModelState.IsValid)
            {
                danhGia.NgayDanhGia = DateTime.Now;
                _context.Add(danhGia);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm đánh giá thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["NhanViens"] = new SelectList(await _context.NhanViens.ToListAsync(), "MaNhanVien", "HoTen", danhGia.MaNhanVien);
            return View(danhGia);
        }

        // POST: DanhGia/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var danhGia = await _context.DanhGias.FindAsync(id);
            if (danhGia != null)
            {
                _context.DanhGias.Remove(danhGia);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa đánh giá thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // API: DELETE DanhGia (Ajax)
        [HttpDelete]
        public async Task<IActionResult> DeleteApi(int id)
        {
            var danhGia = await _context.DanhGias.FindAsync(id);
            if (danhGia == null)
                return NotFound(new { message = "Không tìm thấy đánh giá" });

            _context.DanhGias.Remove(danhGia);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Xóa đánh giá thành công!" });
        }

        // API: Create DanhGia (Ajax)
        [HttpPost]
        public async Task<IActionResult> CreateApi([FromBody] DanhGia danhGia)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });

            danhGia.NgayDanhGia = DateTime.Now;
            _context.Add(danhGia);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Thêm đánh giá thành công!", id = danhGia.MaDanhGia });
        }
    }
}
