using AuthServer.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vefa.CustomAuth.AspNetCore.Extensions;
using Vefa.CustomAuth.Core.Stores;
using Vefa.CustomAuth.EntityFrameworkCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var protocolConnectionString = builder.Configuration.GetConnectionString("CustomAuth")
    ?? "Data Source=customauth.db";
var applicationConnectionString = builder.Configuration.GetConnectionString("Application")
    ?? "Data Source=application.db";
var dataProtectionConnectionString = builder.Configuration.GetConnectionString("DataProtection")
    ?? "Data Source=dataprotection.db";

// Each concern lives in its own DbContext / database, so any one of them can be
// swapped independently later (e.g. protocol stores -> MongoDB via
// Vefa.CustomAuth.MongoDB, Data Protection keys -> Redis) without touching the
// others.

// --- 1) CustomAuth protocol persistence (EF Core + SQLite) -----------------
// Uses a derived context (CustomAuthProtocolDbContext) so the protocol tables
// can be renamed. Registered BEFORE AddCustomAuth so the EF stores win the
// TryAdd registrations (otherwise AddCustomAuth registers in-memory stores).
builder.Services.AddDbContext<CustomAuthProtocolDbContext>(o => o.UseSqlite(protocolConnectionString));
builder.Services.AddCustomAuthStores<CustomAuthProtocolDbContext>();

// --- 2) Main application database + ASP.NET Core Identity -------------------
// ApplicationDbContext is the primary app database (Identity tables today, more
// application entities later).
builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(applicationConnectionString));
builder.Services
    .AddIdentityCore<AppUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
// Expose Identity users to Vefa.CustomAuth.
builder.Services.AddScoped<ICustomAuthUserStore, IdentityUserStore>();

// --- 3) Data Protection key ring (its own DbContext + database) -------------
// Persisted + fixed application name => every instance behind a load balancer
// shares the keys (web-farm). To move to Redis later, swap PersistKeysToDbContext
// for PersistKeysToStackExchangeRedis and drop DataProtectionKeysDbContext.
builder.Services.AddDbContext<DataProtectionKeysDbContext>(o => o.UseSqlite(dataProtectionConnectionString));
builder.Services.AddDataProtection()
    .SetApplicationName("vefa-customauth-authserver")
    .PersistKeysToDbContext<DataProtectionKeysDbContext>();

// --- Vefa.CustomAuth authorization server ----------------------------------
builder.Services
    .AddCustomAuth(options =>
    {
        options.Issuer = builder.Configuration["CustomAuth:Issuer"] ?? "https://localhost:5001";
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.PostLogoutRedirectUri = "/";
        options.RequirePkce = true;
        options.RequireHttps = builder.Configuration.GetValue("CustomAuth:RequireHttps", true);
        // We own the login page (GET + POST) via a Razor Page, so the package
        // must not also map its built-in POST /Account/Login endpoint.
        options.MapDefaultLoginEndpoint = false;
    })
    .AddJwtTokenSigning()
    .AddSigningCertificate(
        builder.Configuration["SigningCertificate:Path"]!,
        builder.Configuration["SigningCertificate:Password"]);

var app = builder.Build();

await app.SeedAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();
// OAuth2 / OIDC protocol endpoints (authorize, token, jwks, userinfo, logout, ...).
app.MapCustomAuthEndpoints();

app.Run();
