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

    public DbSet<UserScripture> UserScriptures => Set<UserScripture>();

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
            entity.Property(scripture => scripture.Text).IsRequired().HasMaxLength(2000);
            entity.Property(scripture => scripture.Topic).HasMaxLength(120);
        });

        modelBuilder.Entity<MemorizationEntry>(entity =>
        {
            entity.HasKey(entry => entry.Id);
            entity.Property(entry => entry.GameText).IsRequired().HasMaxLength(2000);
            entity.Property(entry => entry.TableVerses).HasMaxLength(4000);
            entity.Property(entry => entry.TableThemes).HasMaxLength(2000);
            entity.Property(entry => entry.TableKeywords).HasMaxLength(2000);
            entity.Property(entry => entry.TableInsights).HasMaxLength(2000);
            entity.Property(entry => entry.TablePersonalApplication).HasMaxLength(2000);

            entity.HasOne(entry => entry.User)
                .WithMany()
                .HasForeignKey(entry => entry.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(entry => entry.Scripture)
                .WithMany()
                .HasForeignKey(entry => entry.ScriptureId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // UserScripture: Tracks user's progress with each scripture
        modelBuilder.Entity<UserScripture>(entity =>
        {
            entity.HasKey(us => us.Id);
            entity.Property(us => us.PersonalNotes).HasMaxLength(1000);
            entity.Property(us => us.Status).IsRequired().HasMaxLength(20);
            entity.Property(us => us.PracticeCount).HasDefaultValue(0);
            entity.Property(us => us.CurrentStreakDays).HasDefaultValue(0);
            entity.Property(us => us.LongestStreakDays).HasDefaultValue(0);
            entity.Property(us => us.IsMemorized).HasDefaultValue(false);
            entity.Property(us => us.SuccessRate).HasDefaultValue(0);
            entity.Property(us => us.SuccessfulAttempts).HasDefaultValue(0);
            entity.Property(us => us.FailedAttempts).HasDefaultValue(0);
            entity.Property(us => us.Priority).HasDefaultValue(2);
            entity.Property(us => us.DifficultyRating).HasColumnType("INTEGER");

            // Relationships
            entity.HasOne(us => us.User)
                .WithMany(u => u.UserScriptures)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(us => us.Scripture)
                .WithMany()
                .HasForeignKey(us => us.ScriptureId)
                .OnDelete(DeleteBehavior.Cascade);

            // Composite unique index to prevent duplicates
            entity.HasIndex(us => new { us.UserId, us.ScriptureId }).IsUnique();

            // Regular indexes for common queries
            entity.HasIndex(us => us.UserId);
            entity.HasIndex(us => us.ScriptureId);
            entity.HasIndex(us => us.Status);
            entity.HasIndex(us => us.IsMemorized);
            entity.HasIndex(us => us.LastPracticedAtUtc);
        });
    }
}