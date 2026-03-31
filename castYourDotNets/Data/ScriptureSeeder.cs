using castYourDotNets.Models;

namespace castYourDotNets.Data;

public static class ScriptureSeeder
{
    public static async Task SeedAsync(VerseVaultDbContext dbContext)
    {
        if (dbContext.Scriptures.Any())
            return;

        var scriptures = new List<Scripture>
        {
            new Scripture
            {
                Work = "Book of Mormon",
                Book = "1 Nephi",
                Chapter = 3,
                Verse = 7,
                Text = "And it came to pass that I, Nephi, said unto my father: I will go and do the things which the Lord hath commanded, for I know that the Lord giveth no commandments unto the children of men, save he shall prepare a way for them that they may accomplish the thing which he commandeth them.",
                Topic = "Faith"
            },
            new Scripture
            {
                Work = "Book of Mormon",
                Book = "1 Nephi",
                Chapter = 2,
                Verse = 15,
                Text = "And my father dwelt in a tent.",
                Topic = "Sacrifice"
            },
            new Scripture
            {
                Work = "New Testament",
                Book = "John",
                Chapter = 3,
                Verse = 16,
                Text = "For God so loved the world, that he gave his only begotten Son, that whosoever believeth in him should not perish, but have everlasting life.",
                Topic = "Love"
            },
            new Scripture
            {
                Work = "Old Testament",
                Book = "Psalm",
                Chapter = 23,
                Verse = 1,
                Text = "The Lord is my shepherd; I shall not want.",
                Topic = "Trust"
            }
        };

        dbContext.Scriptures.AddRange(scriptures);
        await dbContext.SaveChangesAsync();
    }
}
