namespace castYourDotNets.Contracts;

public sealed class MemorizationEntryResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string GameText { get; init; } = string.Empty;
    public bool IsMemorized { get; init; }
    public bool IsMemorizedThroughGame { get; init; }
}
