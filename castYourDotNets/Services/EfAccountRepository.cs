using castYourDotNets.Data;
using castYourDotNets.Models;
using Microsoft.EntityFrameworkCore;

namespace castYourDotNets.Services;

public sealed class EfAccountRepository : IAccountRepository
{
    private readonly VerseVaultDbContext dbContext;

    public EfAccountRepository(VerseVaultDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<bool> UsernameExistsAsync(string normalizedUsername, CancellationToken cancellationToken = default)
    {
        // Existence check used by registration to provide friendly duplicate-username errors.
        return dbContext.UserAccounts.AnyAsync(
            account => account.NormalizedUsername == normalizedUsername,
            cancellationToken);
    }

    public async Task<UserAccount> AddAsync(UserAccount account, CancellationToken cancellationToken = default)
    {
        // Persist and return tracked entity with generated values.
        dbContext.UserAccounts.Add(account);
        await dbContext.SaveChangesAsync(cancellationToken);
        return account;
    }

    public Task<UserAccount?> GetByNormalizedUsernameAsync(string normalizedUsername, CancellationToken cancellationToken = default)
    {
        // Used by login flow.
        return dbContext.UserAccounts.SingleOrDefaultAsync(
            account => account.NormalizedUsername == normalizedUsername,
            cancellationToken);
    }

    public Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Used by /me endpoint.
        return dbContext.UserAccounts.SingleOrDefaultAsync(account => account.Id == id, cancellationToken);
    }
}