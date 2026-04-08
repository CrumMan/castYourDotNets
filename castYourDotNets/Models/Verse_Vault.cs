namespace castYourDotNets.Models;

public sealed class Verse_Vault
{
    public enum Scripture
    {
        BookOfMormon,
        OldTestament,
        NewTestament,
        DoctrineAndCovenants
    }

    public Guid id { get; set; } = Guid.NewGuid();
    public Scripture scripture { get; set; }
    public string book { get; set; } = string.Empty;
    public int Chapter { get; set; }
    public int VerseInt { get; set; }
    public string Verse_Refrence { get; set; } = string.Empty;
    public string VerseText { get; set; } = string.Empty;
}
