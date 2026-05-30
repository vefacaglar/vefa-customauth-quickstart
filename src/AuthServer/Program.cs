using AuthServer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vefa.CustomAuth.AspNetCore.Extensions;
using Vefa.CustomAuth.Core.Stores;
using Vefa.CustomAuth.EntityFrameworkCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var connectionString = builder.Configuration.GetConnectionString("CustomAuth")
    ?? "Data Source=customauth.db";

// --- Persistence (EF Core + SQLite) ---------------------------------------
// AppDbContext derives from CustomAuthDbContext and adds the host-owned Users
// table. Registering the EF stores BEFORE AddCustomAuth ensures the EF-backed
// stores win the TryAdd registrations (AddCustomAuth would otherwise register
// in-memory scope/audit stores).
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(connectionString));
builder.Services.AddCustomAuthStores<AppDbContext>();

// The host owns user persistence.
builder.Services.AddSingleton<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<ICustomAuthUserStore, EfUserStore>();

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
    .AddJwtTokenSigning();

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
