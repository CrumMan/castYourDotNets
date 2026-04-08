using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using castYourDotNets.Contracts;
using Microsoft.AspNetCore.Components;

namespace castYourDotNets.Services;

// Keeps the signed-in UI state in one place for the interactive Blazor pages.
public sealed class AuthUiService
{
    private readonly HttpClient httpClient;

    public AuthUiService(IHttpClientFactory httpClientFactory, NavigationManager navigationManager)
    {
        httpClient = httpClientFactory.CreateClient(nameof(AuthUiService));
        httpClient.BaseAddress ??= new Uri(navigationManager.BaseUri);
    }

    // The current session is kept in memory so the nav and protected pages can react immediately after sign-in.
    public AuthenticationResponse? CurrentSession { get; private set; }

    public bool IsAuthenticated =>
        CurrentSession is not null && !string.IsNullOrWhiteSpace(CurrentSession.AccessToken);

    public string? CurrentUsername => CurrentSession?.Account.Username;

    public event Action? AuthenticationStateChanged;

    // After registration succeeds, the user is immediately signed in and sent into the private app area.
    public async Task<AuthActionResult> RegisterAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(
            "api/accounts/register",
            new RegisterAccountRequest
            {
                Username = username,
                Password = password
            },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return AuthActionResult.Failure(await ReadErrorsAsync(response, cancellationToken));
        }

        return await LoginAsync(username, password, cancellationToken);
    }

    public async Task<AuthActionResult> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(
            "api/accounts/login",
            new LoginRequest
            {
                Username = username,
                Password = password
            },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return AuthActionResult.Failure(await ReadErrorsAsync(response, cancellationToken));
        }

        var authentication = await response.Content.ReadFromJsonAsync<AuthenticationResponse>(cancellationToken: cancellationToken);
        if (authentication is null || string.IsNullOrWhiteSpace(authentication.AccessToken))
        {
            return AuthActionResult.Failure(new Dictionary<string, string[]>
            {
                ["auth"] = ["Sign-in succeeded, but the session could not be started. Please try again."]
            });
        }

        CurrentSession = authentication;
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(authentication.TokenType, authentication.AccessToken);

        AuthenticationStateChanged?.Invoke();
        return AuthActionResult.Success();
    }

    public void SignOut()
    {
        CurrentSession = null;
        httpClient.DefaultRequestHeaders.Authorization = null;
        AuthenticationStateChanged?.Invoke();
    }

    // Converts API validation responses into a simple dictionary that the auth forms can display inline.
    private static async Task<Dictionary<string, string[]>> ReadErrorsAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (document.RootElement.TryGetProperty("errors", out var errorsElement) &&
                errorsElement.ValueKind == JsonValueKind.Object)
            {
                var errors = new Dictionary<string, string[]>();

                foreach (var property in errorsElement.EnumerateObject())
                {
                    errors[property.Name] = property.Value.ValueKind == JsonValueKind.Array
                        ? property.Value.EnumerateArray().Select(item => item.GetString() ?? "Invalid value.").ToArray()
                        : ["Invalid value."];
                }

                if (errors.Count > 0)
                {
                    return errors;
                }
            }
        }
        catch (JsonException)
        {
        }

        return new Dictionary<string, string[]>
        {
            ["auth"] = [$"Request failed with status {(int)response.StatusCode}. Please try again."]
        };
    }

    public sealed record AuthActionResult(bool Succeeded, Dictionary<string, string[]> Errors)
    {
        public static AuthActionResult Success() => new(true, new Dictionary<string, string[]>());

        public static AuthActionResult Failure(Dictionary<string, string[]> errors) => new(false, errors);
    }
}
