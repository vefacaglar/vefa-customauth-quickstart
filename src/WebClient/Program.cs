using Vefa.CustomAuth.AspNetCore.Client;

var builder = WebApplication.CreateBuilder(args);

var authority = builder.Configuration["CustomAuth:Authority"] ?? "https://localhost:5001";
var apiBaseUrl = builder.Configuration["CustomAuth:ApiBaseUrl"] ?? "https://localhost:5003";

builder.Services.AddRazorPages();

// Relying-party OpenID Connect integration (cookie + OIDC code flow with PKCE).
// Requests "openid profile email offline_access" by default; we add "api1".
builder.Services.AddCustomAuthClient(options =>
{
    options.Authority = authority;
    options.ClientId = "web-client";
    options.RequireHttpsMetadata = builder.Configuration.GetValue("CustomAuth:RequireHttpsMetadata", true);
    options.AdditionalScopes.Add("api1");
});

// Typed access to the protected API.
builder.Services.AddHttpClient("api", client => client.BaseAddress = new Uri(apiBaseUrl));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
// GET /logout -> cookie sign-out + upstream OIDC end-session.
app.MapCustomAuthSignOut("/logout");

app.Run();
