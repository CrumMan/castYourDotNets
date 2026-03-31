using System.ComponentModel.DataAnnotations;

namespace castYourDotNets.Models;

public class Scripture
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(150)]
    public string Reference { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string ScriptureSource { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Book { get; set; } = string.Empty;

    public int Chapter { get; set; }

    public int VerseNumber { get; set; }

    [Required]
    [StringLength(2000)]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsMemorized { get; set; }

    public int PracticeCount { get; set; }

    public int CurrentStreakDays { get; set; }

    public DateTime? LastPracticedAtUtc { get; set; }

    public DateTime? MemorizedAtUtc { get; set; }
}
