using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vefa.CustomAuth.AspNetCore.Extensions;

namespace AuthServer.Pages.Account;

public class LogoutModel : PageModel
{
    public bool SignedOut { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        // Revoke the SSO session and clear the session cookie.
        await HttpContext.SignOutCustomAuthAsync(cancellationToken);
        SignedOut = true;
        return Page();
    }
}
