using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vefa.CustomAuth.AspNetCore.Extensions;
using Vefa.CustomAuth.Core.Stores;

namespace VefaCustomAuth.Quickstart.AuthServer.Pages.Account;

public class LoginModel : PageModel
{
    private readonly ICustomAuthUserStore _userStore;

    public LoginModel(ICustomAuthUserStore userStore) => _userStore = userStore;

    [BindProperty]
    public string UserName { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string? ReturnUrl { get; set; }

    public string? Error { get; set; }

    public void OnGet(string? returnUrl, string? error, string? userName)
    {
        ReturnUrl = returnUrl;
        UserName = userName ?? string.Empty;
        Error = MapError(error);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
        {
            Error = "Please enter both a username and a password.";
            return Page();
        }

        var user = await _userStore.ValidateCredentialsAsync(UserName, Password, cancellationToken);
        if (user is null)
        {
            Error = "Invalid username or password.";
            return Page();
        }

        // Open the single sign-on session (sets the CustomAuth session cookie).
        await HttpContext.SignInCustomAuthAsync(user.UserId, cancellationToken);

        // Return to the authorize request that sent us here, when it is safe.
        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return Redirect(ReturnUrl);
        }

        return RedirectToPage("/Index");
    }

    private static string? MapError(string? error) => error switch
    {
        null or "" => null,
        "invalid_credentials" => "Invalid username or password.",
        "missing_credentials" => "Please enter both a username and a password.",
        "account_locked" => "This account is temporarily locked due to too many failed attempts.",
        "antiforgery_failed" => "Your session expired. Please try again.",
        _ => "Login failed. Please try again.",
    };
}
