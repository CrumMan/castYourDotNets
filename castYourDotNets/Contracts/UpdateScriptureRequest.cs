namespace castYourDotNets.Contracts;

public sealed class UpdateScriptureRequest
{
    public string Work { get; init; } = string.Empty;
    public string Book { get; init; } = string.Empty;
    public int Chapter { get; init; }
    public int Verse { get; init; }
    public string Text { get; init; } = string.Empty;
    public string Topic { get; init; } = string.Empty;
    public bool IsMemorized { get; init; }
}
