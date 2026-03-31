using castYourDotNets.Data;
using castYourDotNets.Models;
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
                    System.Console.WriteLine($"Skipping {datapath} (file not found)");
                    continue;
                }
                string json = File.ReadAllText(datapath);

                var data = JsonSerializer.Deserialize<ScriptureRoot>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (data == null || (data.sections == null && data.books == null))
                {
                    System.Console.WriteLine($"Skipping {datapath} (no data)");
                    continue;
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
    }
}