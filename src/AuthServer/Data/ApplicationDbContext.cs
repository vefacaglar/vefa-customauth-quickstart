using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace VefaCustomAuth.Quickstart.AuthServer.Data;

/// <summary>
/// The main application database. Derives from <see cref="IdentityDbContext{TUser}"/>
/// so it owns the ASP.NET Core Identity user/role tables, and is the place to add
/// future application entities (just add <c>DbSet</c>s here). Kept separate from the
/// Vefa.CustomAuth protocol context and the Data Protection key-ring context so each
/// can evolve or move to a different store independently.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Drop the "AspNet" prefix from the Identity tables.
        modelBuilder.Entity<AppUser>().ToTable("Users");
        modelBuilder.Entity<IdentityRole>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
    }
}
