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

    public DbSet<MemorizationEntry> MemorizationEntries => Set<MemorizationEntry>();
    public DbSet<Verse_Vault> VerseVaults => Set<Verse_Vault>();

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

        modelBuilder.Entity<Verse_Vault>(entity =>
        {
            entity.HasKey(verse_vault => verse_vault.id);
            entity.Property(verse_vault => verse_vault.Verse_Refrence).IsRequired().HasMaxLength(150);
            entity.Property(verse_vault => verse_vault.book).IsRequired().HasMaxLength(20);
            entity.Property(verse_vault => verse_vault.VerseText).IsRequired().HasMaxLength(2000);
            entity.Property(verse_vault => verse_vault.Chapter).IsRequired();
            entity.Property(verse_vault => verse_vault.VerseInt).IsRequired();
        });

        modelBuilder.Entity<Scripture>(entity =>
        {
            entity.HasKey(scripture => scripture.Id);
            entity.Property(scripture => scripture.Reference).IsRequired().HasMaxLength(150);
            entity.Property(scripture => scripture.ScriptureSource).IsRequired().HasMaxLength(50);
            entity.Property(scripture => scripture.Book).IsRequired().HasMaxLength(100);
            entity.Property(scripture => scripture.Text).IsRequired().HasMaxLength(2000);
        });

        modelBuilder.Entity<MemorizationEntry>(entity =>
        {
            entity.HasKey(entry => entry.Id);
            entity.Property(entry => entry.GameText).IsRequired().HasMaxLength(2000);

            entity.HasOne(entry => entry.User)
                .WithMany()
                .HasForeignKey(entry => entry.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}