using castYourDotNets.Contracts;

namespace castYourDotNets.Services;

public sealed class AccountRegistrationResult
{
    public bool Succeeded { get; private init; }
    public AccountResponse? Account { get; private init; }
    public IDictionary<string, string[]> Errors { get; private init; } = new Dictionary<string, string[]>();

    public static AccountRegistrationResult Success(AccountResponse account) =>
        new()
        {
            Succeeded = true,
            Account = account
        };

    public static AccountRegistrationResult Failure(IDictionary<string, string[]> errors) =>
        new()
        {
            Errors = errors
        };
}