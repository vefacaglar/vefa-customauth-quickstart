namespace AuthServer.Data;

/// <summary>
/// Host-owned user entity. Vefa.CustomAuth does not own user persistence;
/// the host application stores users and exposes them through ICustomAuthUserStore.
/// </summary>
public class AppUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UserName { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string? Email { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
}
