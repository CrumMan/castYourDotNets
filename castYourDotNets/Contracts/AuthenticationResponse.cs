namespace castYourDotNets.Contracts;

public sealed class AuthenticationResponse
{
    // Signed JWT used in Authorization: Bearer <token> requests.
    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public DateTimeOffset ExpiresAtUtc { get; init; }

    // Basic account payload returned with token for client initialization.
    public AccountResponse Account { get; init; } = new();
}