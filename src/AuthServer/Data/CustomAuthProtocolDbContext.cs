using Microsoft.EntityFrameworkCore;
using Vefa.CustomAuth.Core.Models;
using Vefa.CustomAuth.EntityFrameworkCore;

namespace AuthServer.Data;

/// <summary>
/// Vefa.CustomAuth protocol persistence, in its own database. Derives from
/// CustomAuthDbContext so we can rename the tables (dropping the "CustomAuth"
/// prefix) after the base model is applied — as documented by the package.
/// </summary>
public class CustomAuthProtocolDbContext : CustomAuthDbContext
{
    public CustomAuthProtocolDbContext(DbContextOptions<CustomAuthProtocolDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CustomAuthClient>().ToTable("Clients");
        modelBuilder.Entity<CustomAuthClientRedirectUri>().ToTable("ClientRedirectUris");
        modelBuilder.Entity<CustomAuthClientPostLogoutRedirectUri>().ToTable("ClientPostLogoutRedirectUris");
        modelBuilder.Entity<CustomAuthClientAllowedScope>().ToTable("ClientAllowedScopes");
        modelBuilder.Entity<CustomAuthScope>().ToTable("Scopes");
        modelBuilder.Entity<CustomAuthAuthorizationCode>().ToTable("AuthorizationCodes");
        modelBuilder.Entity<CustomAuthRefreshToken>().ToTable("RefreshTokens");
        modelBuilder.Entity<CustomAuthSession>().ToTable("Sessions");
        modelBuilder.Entity<CustomAuthSigningKey>().ToTable("SigningKeys");
        modelBuilder.Entity<CustomAuthAuditLog>().ToTable("AuditLogs");
    }
}
