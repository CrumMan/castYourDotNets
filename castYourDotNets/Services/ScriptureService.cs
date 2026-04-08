using System.Net.Http.Json;
using castYourDotNets.Contracts;
using castYourDotNets.Models;
using Microsoft.AspNetCore.Components;

namespace castYourDotNets.Services;

// Central API wrapper for the signed-in scripture, practice, memorize, and dashboard pages.
public sealed class ScriptureService
{
    private readonly HttpClient httpClient;

    public ScriptureService(IHttpClientFactory httpClientFactory, NavigationManager navigationManager)
    {
        httpClient = httpClientFactory.CreateClient(nameof(ScriptureService));
        httpClient.BaseAddress ??= new Uri(navigationManager.BaseUri);
    }

    public async Task<IReadOnlyList<Scripture>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<Scripture>>("api/scriptures", cancellationToken) ?? [];
    }

    public async Task<Scripture?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/scriptures/{id}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<Scripture>(cancellationToken: cancellationToken);
    }

    // Picks the next verse to review by favoring items that are not memorized and have fewer recent practices.
    public async Task<Scripture?> GetNextPracticeTargetAsync(CancellationToken cancellationToken = default)
    {
        var scriptures = await GetAllAsync(cancellationToken);
        return scriptures
            .OrderBy(scripture => scripture.IsMemorized)
            .ThenBy(scripture => scripture.PracticeCount)
            .ThenBy(scripture => scripture.LastPracticedAtUtc ?? DateTime.MinValue)
            .FirstOrDefault()
            ?? scriptures.FirstOrDefault();
    }

    public async Task AddAsync(Scripture scripture, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/scriptures", scripture, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateAsync(Scripture updatedScripture, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync(
            $"api/scriptures/{updatedScripture.Id}",
            updatedScripture,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/scriptures/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RecordPracticeAsync(Guid id, bool succeeded, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(
            $"api/scriptures/{id}/practice",
            new ScripturePracticeRequest { Succeeded = succeeded },
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task SetMemorizedAsync(Guid id, bool isMemorized, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(
            $"api/scriptures/{id}/memorized",
            new ScriptureMemorizedRequest { IsMemorized = isMemorized },
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<Verse_Vault>> GetAllVersesAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<Verse_Vault>>("api/versevault", cancellationToken) ?? [];
    }
}
