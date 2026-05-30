using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthServer.Pages;

public class IndexModel : PageModel
{
    private readonly IConfiguration _configuration;

    public IndexModel(IConfiguration configuration) => _configuration = configuration;

    public string Issuer => (_configuration["CustomAuth:Issuer"] ?? "https://localhost:5001").TrimEnd('/');

    public void OnGet()
    {
    }
}
