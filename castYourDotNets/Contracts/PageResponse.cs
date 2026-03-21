namespace castYourDotNets.Contracts;

public sealed class PageResponse
{
    public Guid Id { get; init; }

    // Owner account id for the scripture record.
    public Guid UserId { get; init; }
    public string Source { get; init; } = string.Empty;
    public string Book { get; init; } = string.Empty;
    public int Chapter { get; init; }
    public int VerseStart { get; init; }
    public int? VerseEnd { get; init; }

    // Human-friendly verse representation (single verse or range).
    public string Reference { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public bool IsMemorized { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? MemorizedAtUtc { get; init; }
    public int ReviewStreakDays { get; init; }
    public DateTimeOffset? LastReviewedAtUtc { get; init; }
}