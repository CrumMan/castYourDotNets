using castYourDotNets.Contracts;

namespace castYourDotNets.Services;

public sealed class LoginResult
{
    public bool Succeeded { get; private init; }
    public AuthenticationResponse? Authentication { get; private init; }
    public IDictionary<string, string[]> Errors { get; private init; } = new Dictionary<string, string[]>();

    public static LoginResult Success(AuthenticationResponse authentication) =>
        new()
        {
            Succeeded = true,
            Authentication = authentication
        };

    public static LoginResult Failure(IDictionary<string, string[]> errors) =>
        new()
        {
            Errors = errors
        };
}