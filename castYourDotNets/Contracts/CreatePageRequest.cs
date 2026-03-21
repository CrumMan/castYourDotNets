namespace castYourDotNets.Contracts;

public sealed class CreatePageRequest
{
    // Source text collection, e.g., Book of Mormon or King James Bible.
    public string Source { get; init; } = string.Empty;
    public string Book { get; init; } = string.Empty;
    public int Chapter { get; init; }
    public int VerseStart { get; init; }
    public int? VerseEnd { get; init; }
    public string Text { get; init; } = string.Empty;

    // Optional personal annotation from the user.
    public string? Notes { get; init; }
}