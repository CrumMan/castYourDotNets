namespace castYourDotNets.Models;

public class ScriptureRoot
{
    public List<BookJson>? books { get; set; }
    public List<SectionJson>? sections { get; set; }
}
public class SectionJson
{
    public int section { get; set; }
    public List<VerseJson> verses { get; set; } = new();
}
public class ScriptureData
{
    public List<BookJson> books { get; set; } = new();
}

public class BookJson
{
    public string book { get; set; } = "";
    public List<ChapterJson> chapters { get; set; } = new();
}

public class ChapterJson
{
    public int chapter { get; set; }
    public List<VerseJson> verses { get; set; } = new();
}

public class VerseJson
{
    public int verse { get; set; }
    public string Verse_Refrence { get; set; } = null!;
    public string text { get; set; } = null!;
}
