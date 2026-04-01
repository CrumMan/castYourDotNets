using castYourDotNets.Data;
using castYourDotNets.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System.Text.Json;


namespace castYourDotNets;

public static class SeedData
{
    public static void Initialize(VerseVaultDbContext context)
    {
        {
            Verse_Vault vault = null;
            string[] paths = ["Data/book-of-mormon.json", "Data/old-testament.json", "Data/new-testament.json", "Data/doctrine-and-covenants.json"];
            foreach (string datapath in paths)
            {
                Verse_Vault.Scripture scripture_book;
                switch (datapath)
                {
                    case "Data/book-of-mormon.json":
                        scripture_book = Verse_Vault.Scripture.BookOfMormon;
                        break;
                    case "Data/old-testament.json":
                        scripture_book = Verse_Vault.Scripture.OldTestament;
                        break;
                    case "Data/new-testament.json":
                        scripture_book = Verse_Vault.Scripture.NewTestament;
                        break;
                    case "Data/doctrine-and-covenants.json":
                        scripture_book = Verse_Vault.Scripture.DoctrineAndCovenants;
                        break;
                    default:
                        System.Console.WriteLine($"Unknown datapath: {datapath}");
                        return;
                }
                if (!File.Exists(datapath))
                {
                    System.Console.WriteLine($"error finding {datapath}");
                    return;
                }
                string json = File.ReadAllText(datapath);

                var data = JsonSerializer.Deserialize<ScriptureRoot>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (data == null || (data.sections == null && data.books == null))
                {
                    System.Console.WriteLine($"data for {datapath} is null");
                    return;
                }
                if (scripture_book == Verse_Vault.Scripture.DoctrineAndCovenants)
                {
                    foreach (var section in data.sections)
                    {
                        foreach (var verse in section.verses)
                        {
                            vault = new Verse_Vault
                            {
                                scripture = scripture_book,
                                book = "Doctrine and Covenants",
                                Chapter = section.section,
                                VerseInt = verse.verse,
                                VerseText = verse.text,
                                Verse_Refrence = verse.Verse_Refrence ?? $"D&C {section.section}:{verse.verse}"
                            };
                            context.VerseVaults.Add(vault);
                        }
                    }

                }
                else
                {
                    foreach (var book in data.books)
                    {
                        foreach (var chapter in book.chapters)
                        {
                            foreach (var verse in chapter.verses)
                            {
                                vault = new Verse_Vault
                                {
                                    scripture = scripture_book,
                                    book = book.book,
                                    Chapter = chapter.chapter,
                                    VerseInt = verse.verse,
                                    VerseText = verse.text,
                                    Verse_Refrence = verse.Verse_Refrence ?? $"{book.book} {chapter.chapter}:{verse.verse}"
                                };
                                context.VerseVaults.Add(vault);
                            }
                        }
                    }

                }
            }
            context.SaveChanges();
            if (vault != null) System.Console.WriteLine($"Initialized, {vault.Verse_Refrence} was the last input refrence");
        }

        // Seed UserScripture data for testing and onboarding
        SeedUserScriptureData(context);
    }

    /// <summary>
    /// Seeds initial UserScripture data with test users and their scripture memorization progress.
    /// This provides immediate, usable data for testing and user onboarding.
    /// </summary>
    private static void SeedUserScriptureData(VerseVaultDbContext context)
    {
        // Skip if UserScriptures already exist
        if (context.UserScriptures.Any())
        {
            System.Console.WriteLine("UserScripture data already seeded.");
            return;
        }

        // Create test users if they don't exist
        var testUser = context.UserAccounts.FirstOrDefault(u => u.NormalizedUsername == "DEMOUSER");

        if (testUser == null)
        {
            var passwordHasher = new PasswordHasher<UserAccount>();
            testUser = new UserAccount
            {
                Id = Guid.NewGuid(),
                Username = "DemoUser",
                NormalizedUsername = "DEMOUSER",
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
            testUser.PasswordHash = passwordHasher.HashPassword(testUser, "Demo@123");
            context.UserAccounts.Add(testUser);
            context.SaveChanges();
            System.Console.WriteLine($"Created test user: {testUser.Username}");
        }

        // Get some scriptures to associate with the user
        var scriptures = context.Scriptures.Take(5).ToList();

        // If no scriptures exist, create some sample scriptures for testing
        if (scriptures.Count == 0)
        {
            scriptures = new List<Scripture>
            {
                new Scripture
                {
                    Reference = "John 3:16",
                    Text = "For God so loved the world that he gave his one and only Son, that whoever believes in him shall not perish but have eternal life.",
                    Topic = "Love",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Scripture
                {
                    Reference = "Proverbs 3:5-6",
                    Text = "Trust in the Lord with all your heart and lean not on your own understanding; in all your ways submit to him, and he will make your paths straight.",
                    Topic = "Trust",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Scripture
                {
                    Reference = "Philippians 4:6-7",
                    Text = "Do not be anxious about anything, but in every situation, by prayer and petition, with thanksgiving, present your requests to God. And the peace of God, which transcends all understanding, will guard your hearts and your minds in Christ Jesus.",
                    Topic = "Peace",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Scripture
                {
                    Reference = "Matthew 6:33",
                    Text = "But seek first his kingdom and his righteousness, and all these things will be given to you as well.",
                    Topic = "Righteousness",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Scripture
                {
                    Reference = "Psalm 23:1",
                    Text = "The Lord is my shepherd, I lack nothing.",
                    Topic = "Comfort",
                    CreatedAtUtc = DateTime.UtcNow
                }
            };

            context.Scriptures.AddRange(scriptures);
            context.SaveChanges();
            System.Console.WriteLine($"Created {scriptures.Count} sample scriptures for seeding.");
        }

        // Create UserScripture entries with varying progress levels
        var userScriptures = new List<UserScripture>();
        var random = new Random();
        var now = DateTime.UtcNow;

        for (int i = 0; i < scriptures.Count; i++)
        {
            var scripture = scriptures[i];
            var progress = i; // Simulate different progress levels (0-4)

            var userScripture = new UserScripture
            {
                Id = Guid.NewGuid(),
                UserId = testUser.Id,
                ScriptureId = scripture.Id,
                PracticeCount = progress * 3,
                CurrentStreakDays = progress * 2,
                LongestStreakDays = progress * 3,
                IsMemorized = progress >= 3, // Last 2 are marked as memorized
                Status = progress switch
                {
                    0 => "NotStarted",
                    1 => "InProgress",
                    2 => "InProgress",
                    3 => "Completed",
                    _ => "Mastered"
                },
                DifficultyRating = (i % 5) + 1, // Rating 1-5
                Priority = (i % 3) + 1,
                SuccessfulAttempts = progress * 2,
                FailedAttempts = progress,
                SuccessRate = 0, // Will be calculated below
                LastPracticedAtUtc = progress > 0 ? now.AddDays(-progress) : null,
                MemorizedAtUtc = progress >= 3 ? now.AddDays(-progress * 5) : null,
                PersonalNotes = progress > 0 ? $"Working on memorizing {scripture.Reference}. This scripture inspires me to deepen my faith." : string.Empty,
                CreatedAtUtc = now.AddDays(-progress * 7),
                UpdatedAtUtc = now.AddDays(-progress)
            };

            // Fix SuccessRate calculation
            if (userScripture.SuccessfulAttempts + userScripture.FailedAttempts > 0)
            {
                userScripture.SuccessRate = (userScripture.SuccessfulAttempts * 100) /
                    (userScripture.SuccessfulAttempts + userScripture.FailedAttempts);
            }

            userScriptures.Add(userScripture);
        }

        context.UserScriptures.AddRange(userScriptures);
        context.SaveChanges();

        System.Console.WriteLine($"Seeded {userScriptures.Count} UserScripture entries for scripture memorization tracking.");
    }
}