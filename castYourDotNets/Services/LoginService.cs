using castYourDotNets.Contracts;
using castYourDotNets.Models;
using Microsoft.AspNetCore.Identity;

namespace castYourDotNets.Services;

public sealed class LoginService
{
    private readonly IAccountRepository accountRepository;
    private readonly IPasswordHasher<UserAccount> passwordHasher;
    private readonly TokenService tokenService;

    public LoginService(
        IAccountRepository accountRepository,
        IPasswordHasher<UserAccount> passwordHasher,
        TokenService tokenService)
    {
        this.accountRepository = accountRepository;
        this.passwordHasher = passwordHasher;
        this.tokenService = tokenService;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return LoginResult.Failure(errors);
        }

        var normalizedUsername = request.Username.Trim().ToUpperInvariant();
        var account = await accountRepository.GetByNormalizedUsernameAsync(normalizedUsername, cancellationToken);

        if (account is null)
        {
            return InvalidCredentials();
        }

        var passwordResult = passwordHasher.VerifyHashedPassword(account, account.PasswordHash, request.Password);
        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return InvalidCredentials();
        }

        return LoginResult.Success(tokenService.CreateAuthenticationResponse(account));
    }

    private static Dictionary<string, string[]> Validate(LoginRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            errors[nameof(request.Username)] = ["Username is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors[nameof(request.Password)] = ["Password is required."];
        }

        return errors;
    }

    private static LoginResult InvalidCredentials() =>
        LoginResult.Failure(new Dictionary<string, string[]>
        {
            ["credentials"] = ["Invalid username or password."]
        });
}