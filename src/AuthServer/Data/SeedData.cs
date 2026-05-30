using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vefa.CustomAuth.Core.Managers;
using Vefa.CustomAuth.Core.Models;
using Vefa.CustomAuth.Tokens.Signing;

namespace AuthServer.Data;

/// <summary>
/// Creates the SQLite database and seeds the demo configuration on startup:
/// an active RSA signing key, scopes, the web client, and two test users.
/// This mirrors the "Config + EnsureSeedData" step of the Duende quickstarts.
/// </summary>
public static class SeedData
{
    public const string TestPassword = "Pass123$";

    public static async Task SeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        await SeedSigningKeyAsync(sp);
        await SeedScopesAsync(sp);
        await SeedClientsAsync(sp, app.Configuration);
        await SeedUsersAsync(db, sp.GetRequiredService<IPasswordHasher<AppUser>>());
    }

    private static async Task SeedSigningKeyAsync(IServiceProvider sp)
    {
        var keyManager = sp.GetRequiredService<ICustomAuthSigningKeyManager>();
        var active = await keyManager.GetActiveAsync();
        if (active is not null)
        {
            return;
        }

        var key = RsaKeyGenerator.Generate(DateTimeOffset.UtcNow);
        key.IsActive = true;
        await keyManager.StoreAsync(key);
    }

    private static async Task SeedScopesAsync(IServiceProvider sp)
    {
        var scopeManager = sp.GetRequiredService<ICustomAuthScopeManager>();

        var scopes = new[]
        {
            new CustomAuthScope { Name = "openid", DisplayName = "OpenID", Description = "Your user identifier.", Required = true },
            new CustomAuthScope { Name = "profile", DisplayName = "Profile", Description = "Your profile information." },
            new CustomAuthScope { Name = "email", DisplayName = "Email", Description = "Your email address." },
            new CustomAuthScope { Name = "offline_access", DisplayName = "Offline access", Description = "Keep you signed in (refresh tokens)." },
            new CustomAuthScope { Name = "api1", DisplayName = "Demo API", Description = "Access to the protected demo API." },
        };

        foreach (var s in scopes)
        {
            if (await scopeManager.FindByNameAsync(s.Name) is null)
            {
                await scopeManager.CreateAsync(s);
            }
        }
    }

    private static async Task SeedClientsAsync(IServiceProvider sp, IConfiguration config)
    {
        var clientManager = sp.GetRequiredService<ICustomAuthClientManager>();

        var webClientBaseUrl = config["Clients:WebClient:BaseUrl"] ?? "https://localhost:5002";

        if (await clientManager.FindByClientIdAsync("web-client") is null)
        {
            await clientManager.CreateAsync(new CustomAuthClient
            {
                ClientId = "web-client",
                DisplayName = "Vefa CustomAuth Web Client",
                // Public client: authorization code + PKCE, no client secret.
                TokenEndpointAuthMethod = CustomAuthClientAuthenticationMethod.None,
                RequirePkce = true,
                AllowRefreshTokens = true,
                RedirectUris = { $"{webClientBaseUrl}/signin-oidc" },
                PostLogoutRedirectUris = { $"{webClientBaseUrl}/signout-callback-oidc" },
                AllowedScopes = { "openid", "profile", "email", "offline_access", "api1" },
            });
        }
    }

    private static async Task SeedUsersAsync(AppDbContext db, IPasswordHasher<AppUser> hasher)
    {
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var alice = new AppUser
        {
            UserName = "alice",
            Email = "alice@example.com",
            GivenName = "Alice",
            FamilyName = "Smith",
        };
        alice.PasswordHash = hasher.HashPassword(alice, TestPassword);

        var bob = new AppUser
        {
            UserName = "bob",
            Email = "bob@example.com",
            GivenName = "Bob",
            FamilyName = "Jones",
        };
        bob.PasswordHash = hasher.HashPassword(bob, TestPassword);

        db.Users.AddRange(alice, bob);
        await db.SaveChangesAsync();
    }
}
