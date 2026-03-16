using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Models;
using QuanLyNhanSu.ViewModels;

namespace QuanLyNhanSu.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/NguoiDung - Quản lý người dùng
        public async Task<IActionResult> NguoiDung()
        {
            var users = await _context.NguoiDungs
                .Include(u => u.VaiTro)
                .Include(u => u.PhongBan)
                .OrderBy(u => u.TenDangNhap)
                .ToListAsync();

            ViewData["PhongBans"] = await _context.PhongBans
                .OrderBy(p => p.TenPhongBan)
                .ToListAsync();

            return View(users);
        }

        // POST: Admin/CreateNguoiDung
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNguoiDung(NguoiDung nguoiDung)
        {
            if (ModelState.IsValid)
            {
                // Check duplicate username
                if (await _context.NguoiDungs.AnyAsync(u => u.TenDangNhap == nguoiDung.TenDangNhap))
                {
                    TempData["Error"] = "Tên đăng nhập đã tồn tại!";
                    return RedirectToAction(nameof(NguoiDung));
                }

                nguoiDung.NgayTao = DateTime.Now;
                nguoiDung.MaPhongBan = nguoiDung.MaVaiTro == 2 ? nguoiDung.MaPhongBan : null;
                _context.Add(nguoiDung);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm người dùng thành công!";
            }
            return RedirectToAction(nameof(NguoiDung));
        }

        // POST: Admin/DeleteNguoiDung/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNguoiDung(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user != null)
            {
                _context.NguoiDungs.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa người dùng thành công!";
            }
            return RedirectToAction(nameof(NguoiDung));
        }

        // GET: Admin/PhanQuyen
        public async Task<IActionResult> PhanQuyen()
        {
            var danhSachVaiTro = await _context.VaiTros
                .Select(v => new VaiTroItem { MaVaiTro = v.MaVaiTro, TenVaiTro = v.TenVaiTro })
                .ToListAsync();

            var danhSachPhongBan = await _context.PhongBans
                .Select(p => new PhongBanItem { MaPhongBan = p.MaPhongBan, TenPhongBan = p.TenPhongBan })
                .ToListAsync();

            var users = await _context.NguoiDungs
                .Include(u => u.VaiTro)
                .Include(u => u.PhongBan)
                .Select(u => new PhanQuyenViewModel
                {
                    MaNguoiDung = u.MaNguoiDung,
                    TenDangNhap = u.TenDangNhap,
                    HoTen = u.HoTen,
                    MaVaiTro = u.MaVaiTro,
                    TenVaiTro = u.VaiTro != null ? u.VaiTro.TenVaiTro : null,
                    MaPhongBan = u.MaPhongBan,
                    TenPhongBan = u.PhongBan != null ? u.PhongBan.TenPhongBan : null
                })
                .ToListAsync();

            foreach (var user in users)
            {
                user.DanhSachVaiTro = danhSachVaiTro;
                user.DanhSachPhongBan = danhSachPhongBan;
            }

            return View(users);
        }

        // POST: Admin/UpdatePhanQuyen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePhanQuyen(int maNguoiDung, int maVaiTro, int? maPhongBan)
        {
            var user = await _context.NguoiDungs.FindAsync(maNguoiDung);
            if (user != null)
            {
                user.MaVaiTro = maVaiTro;
                user.MaPhongBan = maVaiTro == 2 ? maPhongBan : null;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật phân quyền thành công!";
            }
            return RedirectToAction(nameof(PhanQuyen));
        }

        // API: Create NguoiDung (Ajax)
        [HttpPost]
        public async Task<IActionResult> CreateNguoiDungApi([FromBody] NguoiDung nguoiDung)
        {
            if (await _context.NguoiDungs.AnyAsync(u => u.TenDangNhap == nguoiDung.TenDangNhap))
                return BadRequest(new { success = false, message = "Tên đăng nhập đã tồn tại!" });

            nguoiDung.NgayTao = DateTime.Now;
            nguoiDung.TrangThai = true;
            nguoiDung.MaVaiTro = nguoiDung.MaVaiTro > 0 ? nguoiDung.MaVaiTro : 3;
            if (nguoiDung.MaVaiTro == 2 && !nguoiDung.MaPhongBan.HasValue)
                return BadRequest(new { success = false, message = "Manager phải được gán phòng ban" });

            nguoiDung.MaPhongBan = nguoiDung.MaVaiTro == 2 ? nguoiDung.MaPhongBan : null;
            _context.Add(nguoiDung);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Thêm người dùng thành công!", id = nguoiDung.MaNguoiDung });
        }

        // API: Delete NguoiDung (Ajax)
        [HttpDelete]
        public async Task<IActionResult> DeleteNguoiDungApi(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null)
                return NotFound(new { success = false, message = "Không tìm thấy người dùng" });

            _context.NguoiDungs.Remove(user);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Xóa người dùng thành công!" });
        }

        // API: Update PhanQuyen (Ajax)
        [HttpPut]
        public async Task<IActionResult> UpdatePhanQuyenApi([FromBody] PhanQuyenUpdateModel model)
        {
            var user = await _context.NguoiDungs.FindAsync(model.MaNguoiDung);
            if (user == null)
                return NotFound(new { success = false, message = "Không tìm thấy người dùng" });

            if (model.MaVaiTro == 2 && !model.MaPhongBan.HasValue)
                return BadRequest(new { success = false, message = "Manager phải được gán phòng ban" });

            user.MaVaiTro = model.MaVaiTro;
            user.MaPhongBan = model.MaVaiTro == 2 ? model.MaPhongBan : null;
            await _context.SaveChangesAsync();

            var vaiTro = await _context.VaiTros.FindAsync(model.MaVaiTro);
            var phongBan = user.MaPhongBan.HasValue ? await _context.PhongBans.FindAsync(user.MaPhongBan.Value) : null;
            return Json(new
            {
                success = true,
                message = "Cập nhật phân quyền thành công!",
                tenVaiTro = vaiTro?.TenVaiTro,
                tenPhongBan = phongBan?.TenPhongBan
            });
        }
    }

    public class PhanQuyenUpdateModel
    {
        public int MaNguoiDung { get; set; }
        public int MaVaiTro { get; set; }
        public int? MaPhongBan { get; set; }
    }
}
