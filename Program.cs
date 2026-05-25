using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SafeTunnel.Data;
using SafeTunnel.Services;
using SafeTunnel.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Servicios MVC
builder.Services.AddControllersWithViews();

// ========== AGREGAR SESIONES (necesario para TempData) ==========
builder.Services.AddDistributedMemoryCache(); // Almacenamiento en memoria para sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// ================================================================

// Conexión a SQL Server con Entity Framework Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Servicio de seguridad / cifrado
builder.Services.AddScoped<SafeTunnel.Services.SeguridadVpnService>();
builder.Services.AddScoped<SafeTunnel.Services.CifradoRsaService>();
builder.Services.AddScoped<SafeTunnel.Services.RedSimuladaService>();
builder.Services.AddScoped<SafeTunnel.Services.AtaqueService>();
builder.Services.AddSignalR(options =>
{
    // Aumentar tamaño máximo de mensajes a 64 MB (para archivos grandes)
    options.MaximumReceiveMessageSize = 64 * 1024 * 1024; // 64 MB
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.EnableDetailedErrors = true; // Para ver errores detallados en el cliente
});
builder.Services.AddScoped<EmailService>();

// Autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
        options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
    });

// Autorización
builder.Services.AddAuthorization();

var app = builder.Build();

// Manejo de errores
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ========== AGREGAR SESIONES ANTES DE AUTENTICACIÓN ==========
app.UseSession();
// =============================================================

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<SimulacionHub>("/simulacionHub");

app.Run();