namespace castYourDotNets.Contracts;

public sealed class RegisterAccountRequest
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}