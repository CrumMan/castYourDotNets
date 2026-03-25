using castYourDotNets.Models;
using Microsoft.EntityFrameworkCore;

namespace castYourDotNets.Data;

public sealed class VerseVaultDbContext : DbContext
{
    public VerseVaultDbContext(DbContextOptions<VerseVaultDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    public DbSet<PageClass> PageClasses => Set<PageClass>();

    public DbSet<Scripture> Scriptures => Set<Scripture>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(account => account.Id);
            entity.Property(account => account.Username).IsRequired().HasMaxLength(32);
            entity.Property(account => account.NormalizedUsername).IsRequired().HasMaxLength(32);
            entity.Property(account => account.PasswordHash).IsRequired();
            entity.HasIndex(account => account.NormalizedUsername).IsUnique();
        });

        // One-to-many ownership (UserAccount -> PageClass), cascade delete on account removal.
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

        modelBuilder.Entity<Scripture>(entity =>
        {
            entity.HasKey(scripture => scripture.Id);
            entity.Property(scripture => scripture.Reference).IsRequired().HasMaxLength(150);
            entity.Property(scripture => scripture.Text).IsRequired().HasMaxLength(2000);
            entity.Property(scripture => scripture.Topic).HasMaxLength(120);
        });
    }
}