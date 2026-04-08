using System.Net.Http.Json;
using castYourDotNets.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace castYourDotNets.Services;

public sealed class AccountApiClient
{
    private readonly HttpClient httpClient;

    public AccountApiClient(IHttpClientFactory httpClientFactory, NavigationManager navigationManager)
    {
        httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(navigationManager.BaseUri);
    }

    public async Task<(AuthenticationResponse? Authentication, Dictionary<string, string[]> Errors)> SignInAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/accounts/login", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var authentication = await response.Content.ReadFromJsonAsync<AuthenticationResponse>(cancellationToken);
            return authentication is null
                ? (null, new Dictionary<string, string[]> { ["general"] = ["The server returned an empty response."] })
                : (authentication, new Dictionary<string, string[]>());
        }

        return (null, await ReadErrorsAsync(response, cancellationToken));
    }

    public async Task<(AccountResponse? Account, Dictionary<string, string[]> Errors)> SignUpAsync(
        RegisterAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/accounts/register", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var account = await response.Content.ReadFromJsonAsync<AccountResponse>(cancellationToken);
            return account is null
                ? (null, new Dictionary<string, string[]> { ["general"] = ["The server returned an empty response."] })
                : (account, new Dictionary<string, string[]>());
        }

        return (null, await ReadErrorsAsync(response, cancellationToken));
    }

    private static async Task<Dictionary<string, string[]>> ReadErrorsAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(cancellationToken);
            if (problem?.Errors is { Count: > 0 })
            {
                return new Dictionary<string, string[]>(problem.Errors, StringComparer.OrdinalIgnoreCase);
            }
        }
        catch
        {
            // Fall back to a generic error message if the response is not a validation payload.
        }

        var message = string.IsNullOrWhiteSpace(response.ReasonPhrase)
            ? "Something went wrong. Please try again."
            : response.ReasonPhrase;

        return new Dictionary<string, string[]>
        {
            ["general"] = [message]
        };
    }
}
