using castYourDotNets.Models;
using Microsoft.EntityFrameworkCore;

namespace castYourDotNets.Data;

public sealed class VerseVaultDbContext : DbContext
{
    public VerseVaultDbContext(DbContextOptions<VerseVaultDbContext> options)
        : base(options)
    {
    }

    // User identity records (username, password hash, creation metadata).
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    // Scripture memorization entries owned by a single user.
    public DbSet<PageClass> PageClasses => Set<PageClass>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User account constraints:
        // - username and normalized username are required
        // - normalized username is unique so duplicates are blocked at DB level
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(account => account.Id);
            entity.Property(account => account.Username).IsRequired().HasMaxLength(32);
            entity.Property(account => account.NormalizedUsername).IsRequired().HasMaxLength(32);
            entity.Property(account => account.PasswordHash).IsRequired();
            entity.HasIndex(account => account.NormalizedUsername).IsUnique();
        });

        // Scripture entry constraints + ownership relationship:
        // - required scripture metadata
        // - one-to-many relationship from user -> scripture entries
        // - cascade delete so deleting an account removes its personal scripture records
        modelBuilder.Entity<PageClass>(entity =>
        {
            entity.HasKey(page => page.Id);
            entity.Property(page => page.Source).IsRequired().HasMaxLength(64);
            entity.Property(page => page.Book).IsRequired().HasMaxLength(128);
            entity.Property(page => page.Text).IsRequired();
            entity.Property(page => page.Notes).HasMaxLength(2000);

            entity.HasOne(page => page.User)
                .WithMany(user => user.PageClasses)
                .HasForeignKey(page => page.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}