using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace VefaCustomAuth.Quickstart.AuthServer.Data;

/// <summary>
/// Holds only the ASP.NET Core Data Protection key ring, in its own database.
/// Isolating it makes it easy to move the keys to another backing store later
/// (e.g. Redis) by swapping the registration in <c>Program.cs</c>.
/// </summary>
public class DataProtectionKeysDbContext : DbContext, IDataProtectionKeyContext
{
    public DataProtectionKeysDbContext(DbContextOptions<DataProtectionKeysDbContext> options)
        : base(options)
    {
    }

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<DataProtectionKey>().ToTable("Keys");
    }
}
