namespace castYourDotNets.Contracts;

public sealed class CreateMemorizationEntryRequest
{
    public string GameText { get; init; } = string.Empty;
    public bool IsMemorized { get; init; }
    public bool IsMemorizedThroughGame { get; init; }
}
