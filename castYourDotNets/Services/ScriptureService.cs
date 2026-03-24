using System.Net;
using System.Net.Http.Json;
using castYourDotNets.Contracts;
using castYourDotNets.Models;
using Microsoft.AspNetCore.Components;

namespace castYourDotNets.Services;

public class ScriptureService
{
    private readonly HttpClient httpClient;

    public ScriptureService(IHttpClientFactory httpClientFactory, NavigationManager navigationManager)
    {
        httpClient = httpClientFactory.CreateClient(nameof(ScriptureService));
        httpClient.BaseAddress = new Uri(navigationManager.BaseUri);
    }

    public async Task<IReadOnlyList<Scripture>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<Scripture>>("api/scriptures", cancellationToken) ?? [];
    }

    public async Task<Scripture?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/scriptures/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Scripture>(cancellationToken);
    }

    public async Task<Scripture?> GetNextPracticeTargetAsync(CancellationToken cancellationToken = default)
    {
        var scriptures = await GetAllAsync(cancellationToken);
        return scriptures
            .Where(scripture => !scripture.IsMemorized)
            .OrderBy(scripture => scripture.LastPracticedAtUtc ?? DateTime.MinValue)
            .ThenBy(scripture => scripture.Reference)
            .FirstOrDefault()
            ?? scriptures.FirstOrDefault();
    }

    public async Task AddAsync(Scripture scripture, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/scriptures", scripture, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> UpdateAsync(Scripture updatedScripture, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync(
            $"api/scriptures/{updatedScripture.Id}",
            updatedScripture,
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/scriptures/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<bool> RecordPracticeAsync(Guid id, bool succeeded, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(
            $"api/scriptures/{id}/practice",
            new ScripturePracticeRequest { Succeeded = succeeded },
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<bool> SetMemorizedAsync(Guid id, bool isMemorized, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(
            $"api/scriptures/{id}/memorized",
            new ScriptureMemorizedRequest { IsMemorized = isMemorized },
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }
}
