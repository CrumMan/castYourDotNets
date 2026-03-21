namespace castYourDotNets.Contracts;

public sealed class PageResponse
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }
    public string Source { get; init; } = string.Empty;
    public string Book { get; init; } = string.Empty;
    public int Chapter { get; init; }
    public int VerseStart { get; init; }
    public int? VerseEnd { get; init; }

    public string Reference { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public bool IsMemorized { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? MemorizedAtUtc { get; init; }
    public int ReviewStreakDays { get; init; }
    public DateTimeOffset? LastReviewedAtUtc { get; init; }
}