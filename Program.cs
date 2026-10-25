using AuthenticationModule.Data;
using AuthenticationModule.Decorators;
using AuthenticationModule.Filters;
using AuthenticationModule.Middleware;
using AuthenticationModule.Models;
using AuthenticationModule.Security;
using AuthenticationModule.Services;
using Dapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using AuthenticationModule.Services.Projects;
var builder = WebApplication.CreateBuilder(args);
DefaultTypeMap.MatchNamesWithUnderscores = true;

// 1. Servicios
builder.Services.AddDbContext<AuthDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILoggingService, LoggingDecorator>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IProyectoRepository, ProyectoRepository>();
builder.Services.AddScoped<IEstudianteRepository, EstudianteRepository>();
builder.Services.AddScoped<IDocenteRepository, DocenteRepository>();
builder.Services.AddScoped<IEvaluacionRepository, EvaluacionRepository>();

// 2. Autenticación por cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.LogoutPath = "/Account/Logout";
        o.AccessDeniedPath = "/Account/AccessDenied";
        o.ExpireTimeSpan = TimeSpan.FromDays(7);
        o.SlidingExpiration = true;
        o.Cookie.Name = "AuthCookie";
        o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        o.Cookie.SameSite = SameSiteMode.Strict;
    });

// 3. Autorización
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    o.AddPolicy("UserOnly", p => p.RequireRole("User", "Admin"));
});

// 4. CSRF
builder.Services.AddAntiforgery(o =>
{
    o.HeaderName = "X-CSRF-TOKEN";
    o.Cookie.Name = "AntiForgeryToken";
    o.Cookie.HttpOnly = true;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// 5. Razor Pages + convenciones + filtro anti-XSS global
builder.Services.AddRazorPages(o =>
{
    o.Conventions.AuthorizePage("/Admin/Users", "AdminOnly");
    o.Conventions.ConfigureFilter(new AntiXssFilter());
});

// 6. HSTS
builder.Services.Configure<HstsOptions>(o => { o.IncludeSubDomains = true; o.MaxAge = TimeSpan.FromDays(365); });

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseRouting();
app.UseAuthentication();   // ¿quién eres?
app.UseAuthorization();    // ¿qué puedes hacer?
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

// Migraciones + seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();

    void Seed(string username, string email, string pass, int roleId)
    {
        if (db.Users.Any(u => u.Username == username)) return;
        var (hash, salt) = PasswordHasher.HashPassword(pass);
        db.Users.Add(new User
        {
            Username = username,
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
    }
    Seed("admin", "admin@authmodule.local", "Admin123!", 1);
    Seed("user", "user@authmodule.local", "User123!", 2);
    db.SaveChanges();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await SeedDataProyectos.SeedProyectosAsync(db, logger);
}

app.Run();