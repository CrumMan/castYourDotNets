namespace castYourDotNets.Models;

public class PageClass
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid UserId { get; private set; }
    public UserAccount? User { get; private set; }
    public string Source { get; private set; } = string.Empty;
    public string Book { get; private set; } = string.Empty;
    public int Chapter { get; private set; }
    public int VerseStart { get; private set; }
    public int? VerseEnd { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public bool IsMemorized { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? MemorizedAtUtc { get; private set; }
    public int ReviewStreakDays { get; private set; }
    public DateTimeOffset? LastReviewedAtUtc { get; private set; }

    private PageClass()
    {
    }

    public PageClass(
        Guid userId,
        string source,
        string book,
        int chapter,
        int verseStart,
        int? verseEnd,
        string text,
        string? notes = null)
    {
        UserId = Require(userId, nameof(userId));
        Source = Require(source, nameof(source));
        Book = Require(book, nameof(book));
        Chapter = Positive(chapter, nameof(chapter));
        VerseStart = Positive(verseStart, nameof(verseStart));
        VerseEnd = verseEnd is null ? null : Positive(verseEnd.Value, nameof(verseEnd));

        if (VerseEnd is not null && VerseEnd < VerseStart)
        {
            throw new ArgumentException("VerseEnd must be greater than or equal to VerseStart.", nameof(verseEnd));
        }

        Text = Require(text, nameof(text));
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public string Reference =>
        VerseEnd is null || VerseEnd == VerseStart
            ? $"{Book} {Chapter}:{VerseStart}"
            : $"{Book} {Chapter}:{VerseStart}-{VerseEnd}";

    public void UpdateNotes(string? notes)
    {
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void MarkMemorized(DateTimeOffset? atUtc = null)
    {
        IsMemorized = true;
        MemorizedAtUtc = atUtc ?? DateTimeOffset.UtcNow;
    }

    public void MarkNotMemorized()
    {
        IsMemorized = false;
        MemorizedAtUtc = null;
    }

    public void RecordReview(DateTimeOffset? reviewedAtUtc = null)
    {
        // Streak rules:
        // - same day review does not increment
        // - next-day review increments
        // - any larger gap resets streak to 1
        var now = reviewedAtUtc ?? DateTimeOffset.UtcNow;

        if (LastReviewedAtUtc is null)
        {
            ReviewStreakDays = 1;
        }
        else
        {
            var previousDate = LastReviewedAtUtc.Value.UtcDateTime.Date;
            var currentDate = now.UtcDateTime.Date;
            var gap = (currentDate - previousDate).Days;

            if (gap == 0)
            {
                return;
            }

            ReviewStreakDays = gap == 1 ? ReviewStreakDays + 1 : 1;
        }

        LastReviewedAtUtc = now;
    }

    private static string Require(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", paramName);
        }

        return value.Trim();
    }

    private static int Positive(int value, string paramName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Value must be greater than zero.");
        }

        return value;
    }

    private static Guid Require(Guid value, string paramName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Value is required.", paramName);
        }

        return value;
    }
}