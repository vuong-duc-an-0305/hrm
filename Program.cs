using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Models;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Entity Framework - SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication - Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Home/Forbidden";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("NotManager", policy => policy.RequireAssertion(context => !context.User.IsInRole("Manager")));
});

// Memory Cache
builder.Services.AddMemoryCache();

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// Seed tài khoản mặc định theo phòng ban: tenphongban/tenphongban
await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var managerRoleId = await context.VaiTros
        .Where(v => v.TenVaiTro == "Manager")
        .Select(v => v.MaVaiTro)
        .FirstOrDefaultAsync();

    if (managerRoleId > 0)
    {
        var phongBans = await context.PhongBans.ToListAsync();

        foreach (var phongBan in phongBans)
        {
            var hasDepartmentManager = await context.NguoiDungs.AnyAsync(u =>
                u.MaVaiTro == managerRoleId &&
                u.MaPhongBan == phongBan.MaPhongBan);

            if (hasDepartmentManager)
                continue;

            var baseCredential = NormalizeCredentialName(phongBan.TenPhongBan);
            if (string.IsNullOrWhiteSpace(baseCredential))
                baseCredential = $"phongban{phongBan.MaPhongBan}";

            var credential = baseCredential;
            var index = 1;
            while (await context.NguoiDungs.AnyAsync(u => u.TenDangNhap == credential))
            {
                credential = $"{baseCredential}{index}";
                index++;
            }

            context.NguoiDungs.Add(new NguoiDung
            {
                TenDangNhap = credential,
                MatKhau = credential,
                HoTen = $"Quản lý {phongBan.TenPhongBan}",
                Email = $"{credential}@company.local",
                TrangThai = true,
                NgayTao = DateTime.Now,
                MaVaiTro = managerRoleId,
                MaPhongBan = phongBan.MaPhongBan
            });
        }

        await context.SaveChangesAsync();
    }
}

app.Run();

static string NormalizeCredentialName(string input)
{
    if (string.IsNullOrWhiteSpace(input))
        return string.Empty;

    var normalized = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
    var builder = new StringBuilder();

    foreach (var ch in normalized)
    {
        var category = CharUnicodeInfo.GetUnicodeCategory(ch);
        if (category == UnicodeCategory.NonSpacingMark)
            continue;

        if (char.IsLetterOrDigit(ch))
            builder.Append(ch);
    }

    return builder.ToString().Normalize(NormalizationForm.FormC);
}
