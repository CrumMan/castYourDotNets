using castYourDotNets.Models;

namespace castYourDotNets.Services;

public interface IAccountRepository
{
    Task<bool> UsernameExistsAsync(string normalizedUsername, CancellationToken cancellationToken = default);
    Task<UserAccount> AddAsync(UserAccount account, CancellationToken cancellationToken = default);
    Task<UserAccount?> GetByNormalizedUsernameAsync(string normalizedUsername, CancellationToken cancellationToken = default);
    Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}