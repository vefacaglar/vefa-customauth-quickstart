using Microsoft.EntityFrameworkCore;
using Vefa.CustomAuth.EntityFrameworkCore;

namespace AuthServer.Data;

/// <summary>
/// Derives from CustomAuthDbContext so that the OAuth2/OIDC protocol tables
/// (clients, scopes, codes, refresh tokens, sessions, signing keys, audit logs)
/// and our host-owned <see cref="AppUser"/> table live in a single SQLite database.
/// </summary>
public class AppDbContext : CustomAuthDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the CustomAuth model first.
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(b =>
        {
            b.ToTable("AppUsers");
            b.HasKey(u => u.Id);
            b.HasIndex(u => u.UserName).IsUnique();
            b.Property(u => u.UserName).IsRequired().HasMaxLength(256);
            b.Property(u => u.PasswordHash).IsRequired();
            b.Property(u => u.Email).HasMaxLength(256);
        });
    }
}
