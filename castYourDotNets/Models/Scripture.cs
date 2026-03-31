using System.ComponentModel.DataAnnotations;

namespace castYourDotNets.Models;

public class Scripture
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(50)]
    // e.g. "Book of Mormon", "Bible"
    public string Work { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    // e.g. "1 Nephi", "John"
    public string Book { get; set; } = string.Empty;

    public int Chapter { get; set; }

    public int Verse { get; set; }


    [Required]
    [StringLength(2000)]
    public string Text { get; set; } = string.Empty;

    [StringLength(120)]
    public string Topic { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsMemorized { get; set; }

    public int PracticeCount { get; set; }

    public int CurrentStreakDays { get; set; }

    public DateTime? LastPracticedAtUtc { get; set; }

    public DateTime? MemorizedAtUtc { get; set; }

    public string Reference => $"{Book} {Chapter}:{Verse}";

}
