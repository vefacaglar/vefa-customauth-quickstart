using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebClient.Pages;

public class IndexModel : PageModel
{
    public bool IsSignedIn => User.Identity?.IsAuthenticated ?? false;
    public string? DisplayName => User.Identity?.Name;

    public void OnGet()
    {
    }
}
