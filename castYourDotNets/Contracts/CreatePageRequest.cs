namespace castYourDotNets.Contracts;

public sealed class CreatePageRequest
{
    public string Source { get; init; } = string.Empty;
    public string Book { get; init; } = string.Empty;
    public int Chapter { get; init; }
    public int VerseStart { get; init; }
    public int? VerseEnd { get; init; }
    public string Text { get; init; } = string.Empty;

    public string? Notes { get; init; }
}