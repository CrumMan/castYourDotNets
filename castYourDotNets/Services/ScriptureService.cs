using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using castYourDotNets.Contracts;
using castYourDotNets.Models;
using Microsoft.AspNetCore.Components;

namespace castYourDotNets.Services;

public sealed class ScriptureService
{
    private readonly HttpClient httpClient;
    private readonly AuthSessionState authSession;

    private static readonly IReadOnlyList<Verse_Vault> SampleVerses =
    [
        new()
        {
            scripture = Verse_Vault.Scripture.BookOfMormon,
            book = "1 Nephi",
            Chapter = 3,
            VerseInt = 7,
            Verse_Refrence = "1 Nephi 3:7",
            VerseText = "I will go and do the things which the Lord hath commanded."
        },
        new()
        {
            scripture = Verse_Vault.Scripture.BookOfMormon,
            book = "Mosiah",
            Chapter = 2,
            VerseInt = 17,
            Verse_Refrence = "Mosiah 2:17",
            VerseText = "When ye are in the service of your fellow beings ye are only in the service of your God."
        },
        new()
        {
            scripture = Verse_Vault.Scripture.NewTestament,
            book = "John",
            Chapter = 14,
            VerseInt = 15,
            Verse_Refrence = "John 14:15",
            VerseText = "If ye love me, keep my commandments."
        },
        new()
        {
            scripture = Verse_Vault.Scripture.OldTestament,
            book = "Proverbs",
            Chapter = 3,
            VerseInt = 5,
            Verse_Refrence = "Proverbs 3:5",
            VerseText = "Trust in the Lord with all thine heart; and lean not unto thine own understanding."
        },
        new()
        {
            scripture = Verse_Vault.Scripture.DoctrineAndCovenants,
            book = "Doctrine and Covenants",
            Chapter = 18,
            VerseInt = 10,
            Verse_Refrence = "Doctrine and Covenants 18:10",
            VerseText = "Remember the worth of souls is great in the sight of God."
        }
    ];

    public ScriptureService(
        IHttpClientFactory httpClientFactory,
        NavigationManager navigationManager,
        AuthSessionState authSession)
    {
        httpClient = httpClientFactory.CreateClient(nameof(ScriptureService));
        httpClient.BaseAddress = new Uri(navigationManager.BaseUri);
        this.authSession = authSession;
    }

    public Task<List<Verse_Vault>> GetAllVersesAsync()
    {
        return Task.FromResult(SampleVerses.ToList());
    }

    public async Task<IReadOnlyList<Scripture>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuthHeader();

        var response = await httpClient.GetAsync("api/scriptures", cancellationToken);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return [];
        }

        response.EnsureSuccessStatusCode();
        var pages = await response.Content.ReadFromJsonAsync<List<PageResponse>>(cancellationToken) ?? [];
        return pages.Select(ToScripture).ToList();
    }

    public async Task<Scripture?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ApplyAuthHeader();

        var response = await httpClient.GetAsync($"api/scriptures/{id}", cancellationToken);
        if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var page = await response.Content.ReadFromJsonAsync<PageResponse>(cancellationToken);
        return page is null ? null : ToScripture(page);
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
        ApplyAuthHeader();

        var request = new CreatePageRequest
        {
            Source = string.IsNullOrWhiteSpace(scripture.ScriptureSource) ? "Manual" : scripture.ScriptureSource,
            Book = scripture.Book,
            Chapter = scripture.Chapter,
            VerseStart = scripture.VerseNumber,
            VerseEnd = scripture.VerseNumber,
            Text = scripture.Text,
            Notes = null
        };

        var response = await httpClient.PostAsJsonAsync("api/scriptures", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> UpdateAsync(Scripture updatedScripture, CancellationToken cancellationToken = default)
    {
        ApplyAuthHeader();

        var response = await httpClient.PutAsJsonAsync($"api/scriptures/{updatedScripture.Id}", updatedScripture, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ApplyAuthHeader();

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
        ApplyAuthHeader();

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
        ApplyAuthHeader();

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

    private void ApplyAuthHeader()
    {
        var token = authSession.CurrentAuthentication?.AccessToken;
        httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    private static Scripture ToScripture(PageResponse page) =>
        new()
        {
            Id = page.Id,
            Reference = page.Reference,
            ScriptureSource = page.Source,
            Book = page.Book,
            Chapter = page.Chapter,
            VerseNumber = page.VerseStart,
            Text = page.Text,
            CreatedAtUtc = page.CreatedAtUtc.UtcDateTime,
            IsMemorized = page.IsMemorized,
            PracticeCount = page.ReviewStreakDays,
            CurrentStreakDays = page.ReviewStreakDays,
            LastPracticedAtUtc = page.LastReviewedAtUtc?.UtcDateTime,
            MemorizedAtUtc = page.MemorizedAtUtc?.UtcDateTime
        };
}
