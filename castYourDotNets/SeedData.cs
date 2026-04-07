using castYourDotNets.Data;
using castYourDotNets.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace castYourDotNets;

public static class SeedData
{
    public static void Initialize(
    VerseVaultDbContext verseContext,
    AppDbContext appContext,
    IPasswordHasher<User> hasher,
    string contentRootPath)
    {
        Verse_Vault? vault = null;

        // =========================
        // 🔐 SEED USER ACCOUNT
        // =========================
        if (!appContext.Users.Any())
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                PasswordHash = HashPassword("Password123"),
                CreatedAtUtc = DateTimeOffset.UtcNow
            };

            appContext.Users.Add(user);
            Console.WriteLine("Seeded default user: test@test.com");
        }

        // =========================
        // 📖 EXISTING SCRIPTURE SEED
        // =========================
        var seedFiles = new[]
        {
            new
            {
                RelativePath = "Data/book-of-mormon.json",
                Scripture = Verse_Vault.Scripture.BookOfMormon
            },
            new
            {
                RelativePath = "Data/old-testament.json",
                Scripture = Verse_Vault.Scripture.OldTestament
            },
            new
            {
                RelativePath = "Data/new-testament.json",
                Scripture = Verse_Vault.Scripture.NewTestament
            },
            new
            {
                RelativePath = "Data/doctrine-and-covenants.json",
                Scripture = Verse_Vault.Scripture.DoctrineAndCovenants
            }
        };

        foreach (var seedFile in seedFiles)
        {
            var dataPath = Path.Combine(contentRootPath, seedFile.RelativePath);
            if (!File.Exists(dataPath))
            {
                Console.WriteLine($"Unable to find seed file at {dataPath}");
                return;
            }

            var json = File.ReadAllText(dataPath);
            var data = JsonSerializer.Deserialize<ScriptureRoot>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (data == null || (data.sections == null && data.books == null))
            {
                Console.WriteLine($"Seed data for {dataPath} is empty or invalid.");
                return;
            }

            if (seedFile.Scripture == Verse_Vault.Scripture.DoctrineAndCovenants)
            {
                foreach (var section in data.sections ?? [])
                {
                    foreach (var verse in section.verses)
                    {
                        vault = new Verse_Vault
                        {
                            scripture = seedFile.Scripture,
                            book = "Doctrine and Covenants",
                            Chapter = section.section,
                            VerseInt = verse.verse,
                            VerseText = verse.text,
                            Verse_Refrence = verse.Verse_Refrence ?? $"D&C {section.section}:{verse.verse}"
                        };
                        verseContext.VerseVaults.Add(vault);
                    }
                }
            }
            else
            {
                foreach (var book in data.books ?? [])
                {
                    foreach (var chapter in book.chapters)
                    {
                        foreach (var verse in chapter.verses)
                        {
                            vault = new Verse_Vault
                            {
                                scripture = seedFile.Scripture,
                                book = book.book,
                                Chapter = chapter.chapter,
                                VerseInt = verse.verse,
                                VerseText = verse.text,
                                Verse_Refrence = verse.Verse_Refrence ?? $"{book.book} {chapter.chapter}:{verse.verse}"
                            };
                            verseContext.VerseVaults.Add(vault);
                        }
                    }
                }
            }
        }

        verseContext.SaveChanges();

        if (vault != null)
        {
            Console.WriteLine($"Initialized, {vault.Verse_Refrence} was the last input reference.");
        }
    }

    // =========================
    // 🔑 PASSWORD HASHING (MATCHES AuthService)
    // =========================
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}