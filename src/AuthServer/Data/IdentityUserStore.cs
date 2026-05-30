using Microsoft.AspNetCore.Identity;
using Vefa.CustomAuth.Core.Stores;

namespace AuthServer.Data;

/// <summary>
/// Bridges ASP.NET Core Identity to Vefa.CustomAuth. Credential validation and
/// profile lookups go through <see cref="UserManager{TUser}"/>, so Identity owns
/// password hashing, lockout, and user persistence.
/// </summary>
public sealed class IdentityUserStore : ICustomAuthUserStore
{
    private readonly UserManager<AppUser> _userManager;

    public IdentityUserStore(UserManager<AppUser> userManager) => _userManager = userManager;

    public async Task<CustomAuthUserInfo?> ValidateCredentialsAsync(
        string userName, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user is null || !await _userManager.CheckPasswordAsync(user, password))
        {
            return null;
        }

        return ToUserInfo(user);
    }

    public async Task<CustomAuthUserInfo?> FindByIdAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
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
