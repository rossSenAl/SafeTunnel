using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SafeTunnel.Data;
using SafeTunnel.Services;

var builder = WebApplication.CreateBuilder(args);

// Servicios MVC
builder.Services.AddControllersWithViews();

// Conexión a SQL Server con Entity Framework Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Servicio de seguridad / cifrado
builder.Services.AddScoped<SafeTunnel.Services.SeguridadVpnService>();

// Autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
        options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
    });

// Autorización
builder.Services.AddAuthorization();
builder.Services.AddScoped<CifradoRsaService>();

var app = builder.Build();

// Manejo de errores
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// ESTA LÍNEA ES LA QUE TE FALTABA
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();