namespace castYourDotNets.Contracts;

public sealed class AccountResponse
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; init; }
}