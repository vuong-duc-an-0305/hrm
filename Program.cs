using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Models;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var defaultLocalDbConnection = "Server=(localdb)\\MSSQLLocalDB;Database=QuanLyNhanSu;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
var configuredConnection = builder.Configuration.GetConnectionString("DefaultConnection") ?? defaultLocalDbConnection;
var resolvedConnection = await ResolveConnectionStringAsync(configuredConnection, defaultLocalDbConnection);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Entity Framework - SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(resolvedConnection));

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
    // Yêu cầu đăng nhập cho toàn bộ app theo mặc định (trừ [AllowAnonymous])
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Admin", "Manager"));
    // Phải đăng nhập VÀ không phải Manager (Admin + User đều vào được)
    options.AddPolicy("NotManager", policy => policy.RequireAssertion(context =>
        context.User.Identity?.IsAuthenticated == true &&
        !context.User.IsInRole("Manager")));
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

    // Tự động tạo/cập nhật database theo migration cho môi trường máy mới.
    await context.Database.MigrateAsync();

    int managerRoleId;
    try
    {
        managerRoleId = await context.VaiTros
            .Where(v => v.TenVaiTro == "Manager")
            .Select(v => v.MaVaiTro)
            .FirstOrDefaultAsync();
    }
    catch (SqlException) when (app.Environment.IsDevelopment())
    {
        // Trường hợp DB dev bị lệch schema (ví dụ có DB nhưng thiếu bảng), tái tạo lại theo migration.
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();

        managerRoleId = await context.VaiTros
            .Where(v => v.TenVaiTro == "Manager")
            .Select(v => v.MaVaiTro)
            .FirstOrDefaultAsync();
    }

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

static async Task<string> ResolveConnectionStringAsync(string configuredConnection, string localDbConnection)
{
    if (await CanOpenConnectionAsync(configuredConnection))
        return configuredConnection;

    var usesLocalhost = configuredConnection.Contains("Server=localhost", StringComparison.OrdinalIgnoreCase)
                        || configuredConnection.Contains("Data Source=localhost", StringComparison.OrdinalIgnoreCase)
                        || configuredConnection.Contains("Server=.\\", StringComparison.OrdinalIgnoreCase);

    if (usesLocalhost && await CanOpenConnectionAsync(localDbConnection))
        return localDbConnection;

    return configuredConnection;
}

static async Task<bool> CanOpenConnectionAsync(string connectionString)
{
    try
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            ConnectTimeout = 3
        };

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        return true;
    }
    catch
    {
        return false;
    }
}
