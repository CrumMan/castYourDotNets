using castYourDotNets.Contracts;

namespace castYourDotNets.Services;

public sealed class AuthSessionState
{
    public AuthenticationResponse? CurrentAuthentication { get; private set; }

    public bool IsAuthenticated =>
        CurrentAuthentication is not null && CurrentAuthentication.ExpiresAtUtc > DateTimeOffset.UtcNow;

    public string Username => CurrentAuthentication?.Account.Username ?? string.Empty;

    public event Action? Changed;

    public void SignIn(AuthenticationResponse authentication)
    {
        CurrentAuthentication = authentication;
        Changed?.Invoke();
    }

    public void SignOut()
    {
        CurrentAuthentication = null;
        Changed?.Invoke();
    }
}
