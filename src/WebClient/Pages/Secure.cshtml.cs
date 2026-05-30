using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebClient.Pages;

[Authorize]
public class SecureModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SecureModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public List<KeyValuePair<string, string>> Claims { get; } = new();
    public string? AccessToken { get; private set; }
    public string? IdToken { get; private set; }
    public string? RefreshToken { get; private set; }

    public string? ApiResponse { get; private set; }
    public string? ApiError { get; private set; }

    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostCallApiAsync()
    {
        await LoadAsync();

        var client = _httpClientFactory.CreateClient("api");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);

        try
        {
            var response = await client.GetAsync("/identity");
            var body = await response.Content.ReadAsStringAsync();
            ApiResponse = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}\n{body}";
        }
        catch (Exception ex)
        {
            ApiError = ex.Message;
        }

        return Page();
    }

    private async Task LoadAsync()
    {
        Claims.Clear();
        foreach (var claim in User.Claims)
        {
            Claims.Add(new KeyValuePair<string, string>(claim.Type, claim.Value));
        }

        AccessToken = await HttpContext.GetTokenAsync("access_token");
        IdToken = await HttpContext.GetTokenAsync("id_token");
        RefreshToken = await HttpContext.GetTokenAsync("refresh_token");
    }
}
