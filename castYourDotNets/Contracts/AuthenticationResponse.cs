namespace castYourDotNets.Contracts;

public sealed class AuthenticationResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public DateTimeOffset ExpiresAtUtc { get; init; }

    public AccountResponse Account { get; init; } = new();
}