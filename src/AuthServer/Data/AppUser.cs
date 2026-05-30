using Microsoft.AspNetCore.Identity;

namespace VefaCustomAuth.Quickstart.AuthServer.Data;

/// <summary>
/// Application user, managed by ASP.NET Core Identity. The extra profile fields
/// are surfaced as OIDC claims (given_name / family_name) by the user store.
/// </summary>
public class AppUser : IdentityUser
{
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
}
