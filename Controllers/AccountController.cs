using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.ViewModels;
using System.Security.Claims;

namespace QuanLyNhanSu.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Manager") && !User.IsInRole("Admin"))
                    return RedirectToAction("Index", "NhanVien");

                return RedirectToAction("Index", "Dashboard");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.NguoiDungs
                .Include(u => u.VaiTro)
                .Include(u => u.PhongBan)
                .FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap && u.TrangThai);

            if (user == null || user.MatKhau != model.MatKhau)
            {
                ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không chính xác.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
                new Claim(ClaimTypes.Name, user.HoTen ?? user.TenDangNhap),
                new Claim("TenDangNhap", user.TenDangNhap),
                new Claim(ClaimTypes.Role, user.VaiTro?.TenVaiTro ?? "User")
            };

            if (user.MaPhongBan.HasValue)
                claims.Add(new Claim("MaPhongBan", user.MaPhongBan.Value.ToString()));
            if (!string.IsNullOrWhiteSpace(user.PhongBan?.TenPhongBan))
                claims.Add(new Claim("TenPhongBan", user.PhongBan.TenPhongBan));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.GhiNho,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (user.VaiTro?.TenVaiTro == "Manager")
                return RedirectToAction("Index", "NhanVien");

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult LogoutConfirm()
        {
            return View("Logout");
        }

        // API: Login (Ajax)
        [HttpPost]
        public async Task<IActionResult> LoginApi([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Vui lòng nhập đầy đủ thông tin." });

            var user = await _context.NguoiDungs
                .Include(u => u.VaiTro)
                .Include(u => u.PhongBan)
                .FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap && u.TrangThai);

            if (user == null || user.MatKhau != model.MatKhau)
                return Unauthorized(new { success = false, message = "Thông tin đăng nhập không chính xác." });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
                new Claim(ClaimTypes.Name, user.HoTen ?? user.TenDangNhap),
                new Claim("TenDangNhap", user.TenDangNhap),
                new Claim(ClaimTypes.Role, user.VaiTro?.TenVaiTro ?? "User")
            };

            if (user.MaPhongBan.HasValue)
                claims.Add(new Claim("MaPhongBan", user.MaPhongBan.Value.ToString()));
            if (!string.IsNullOrWhiteSpace(user.PhongBan?.TenPhongBan))
                claims.Add(new Claim("TenPhongBan", user.PhongBan.TenPhongBan));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.GhiNho,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            var redirectUrl = user.VaiTro?.TenVaiTro == "Manager" ? "/NhanVien" : "/Dashboard";
            return Json(new { success = true, redirectUrl });
        }

        // API: Logout (Ajax)
        [HttpPost]
        public async Task<IActionResult> LogoutApi()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return Json(new { success = true, redirectUrl = "/Account/Login" });
        }
    }
}
