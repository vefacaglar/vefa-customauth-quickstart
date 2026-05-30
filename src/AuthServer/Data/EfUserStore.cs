using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vefa.CustomAuth.Core.Stores;

namespace AuthServer.Data;

/// <summary>
/// Bridges the host user database to Vefa.CustomAuth. The authorization server
/// calls <see cref="ValidateCredentialsAsync"/> during login and
/// <see cref="FindByIdAsync"/> when building ID tokens / userinfo responses.
/// </summary>
public sealed class EfUserStore : ICustomAuthUserStore
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<AppUser> _passwordHasher;

    public EfUserStore(AppDbContext db, IPasswordHasher<AppUser> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<CustomAuthUserInfo?> ValidateCredentialsAsync(
        string userName, string password, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Failed ? null : ToUserInfo(user);
    }

    public async Task<CustomAuthUserInfo?> FindByIdAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user is null ? null : ToUserInfo(user);
    }

    private static CustomAuthUserInfo ToUserInfo(AppUser user)
    {
        var claims = new Dictionary<string, object>();
        if (!string.IsNullOrWhiteSpace(user.GivenName))
        {
            claims["given_name"] = user.GivenName;
        }
        if (!string.IsNullOrWhiteSpace(user.FamilyName))
        {
            claims["family_name"] = user.FamilyName;
        }

        return new CustomAuthUserInfo
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            AdditionalClaims = claims.Count > 0 ? claims : null,
        };
    }
}
