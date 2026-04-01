namespace castYourDotNets.Contracts;

public sealed class MemorizationEntryResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid? ScriptureId { get; init; }
    public string GameText { get; init; } = string.Empty;
    public bool IsMemorized { get; init; }
    public bool IsMemorizedThroughGame { get; init; }

    // Memorization Table Fields
    public string TableVerses { get; init; } = string.Empty;
    public string TableThemes { get; init; } = string.Empty;
    public string TableKeywords { get; init; } = string.Empty;
    public string TableInsights { get; init; } = string.Empty;
    public string TablePersonalApplication { get; init; } = string.Empty;
    public bool HasMemorizationTable { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
