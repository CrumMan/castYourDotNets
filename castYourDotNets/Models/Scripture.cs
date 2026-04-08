namespace castYourDotNets.Models;

public sealed class Scripture
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Reference { get; set; } = string.Empty;
    public string ScriptureSource { get; set; } = string.Empty;
    public string Book { get; set; } = string.Empty;
    public int Chapter { get; set; }
    public int VerseNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsMemorized { get; set; }
    public int PracticeCount { get; set; }
    public int CurrentStreakDays { get; set; }
    public DateTime? LastPracticedAtUtc { get; set; }
    public DateTime? MemorizedAtUtc { get; set; }
}
