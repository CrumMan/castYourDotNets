using System.Text.RegularExpressions;
using castYourDotNets.Contracts;
using castYourDotNets.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace castYourDotNets.Services;

public sealed class AccountRegistrationService
{
    // Enforces a consistent username format across clients and backend.
    private static readonly Regex UsernamePattern = new("^[a-zA-Z0-9._-]{3,32}$", RegexOptions.Compiled);

    private readonly IAccountRepository accountRepository;
    private readonly IPasswordHasher<UserAccount> passwordHasher;

    public AccountRegistrationService(
        IAccountRepository accountRepository,
        IPasswordHasher<UserAccount> passwordHasher)
    {
        this.accountRepository = accountRepository;
        this.passwordHasher = passwordHasher;
    }

    public async Task<AccountRegistrationResult> RegisterAsync(
        RegisterAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate early to avoid unnecessary persistence work.
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return AccountRegistrationResult.Failure(errors);
        }

        var username = request.Username.Trim();
        var normalizedUsername = username.ToUpperInvariant();

        if (await accountRepository.UsernameExistsAsync(normalizedUsername, cancellationToken))
        {
            return AccountRegistrationResult.Failure(new Dictionary<string, string[]>
            {
                [nameof(request.Username)] = ["That username is already taken."]
            });
        }

        var account = new UserAccount
        {
            Username = username,
            NormalizedUsername = normalizedUsername
        };

        account.PasswordHash = passwordHasher.HashPassword(account, request.Password);

        try
        {
            // Persist only password hashes; never persist plaintext passwords.
            var savedAccount = await accountRepository.AddAsync(account, cancellationToken);

            return AccountRegistrationResult.Success(new AccountResponse
            {
                Id = savedAccount.Id,
                Username = savedAccount.Username,
                CreatedAtUtc = savedAccount.CreatedAtUtc
            });
        }
        catch (DbUpdateException)
        {
            // Handles concurrent duplicate registrations; DB unique index is the final guardrail.
            return AccountRegistrationResult.Failure(new Dictionary<string, string[]>
            {
                [nameof(request.Username)] = ["That username is already taken."]
            });
        }
    }

    private static Dictionary<string, string[]> Validate(RegisterAccountRequest request)
    {
        // Return field-level errors in ValidationProblem format.
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            errors[nameof(request.Username)] = ["Username is required."];
        }
        else if (!UsernamePattern.IsMatch(request.Username.Trim()))
        {
            errors[nameof(request.Username)] =
                ["Username must be 3-32 characters and use only letters, numbers, dots, underscores, or hyphens."];
        }

        var passwordErrors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            passwordErrors.Add("Password is required.");
        }
        else
        {
            if (request.Password.Length < 8)
            {
                passwordErrors.Add("Password must be at least 8 characters long.");
            }

            if (!request.Password.Any(char.IsLetter))
            {
                passwordErrors.Add("Password must contain at least one letter.");
            }

            if (!request.Password.Any(char.IsDigit))
            {
                passwordErrors.Add("Password must contain at least one number.");
            }
        }

        if (passwordErrors.Count > 0)
        {
            errors[nameof(request.Password)] = passwordErrors.ToArray();
        }

        return errors;
    }
}