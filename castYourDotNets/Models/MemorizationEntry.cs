using System.ComponentModel.DataAnnotations;

namespace castYourDotNets.Models;

public class MemorizationEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public UserAccount? User { get; set; }

    /// <summary>
    /// Reference to the Scripture being memorized
    /// </summary>
    public Guid? ScriptureId { get; set; }
    public Scripture? Scripture { get; set; }

    [Required]
    [StringLength(2000)]
    public string GameText { get; set; } = string.Empty;

    public bool IsMemorized { get; set; }

    public bool IsMemorizedThroughGame { get; set; }

    // Memorization Table Fields (Joshua 1:8 - Meditation Study Structure)
    /// <summary>
    /// Full verses or selected passages from the scripture
    /// </summary>
    [StringLength(4000)]
    public string TableVerses { get; set; } = string.Empty;

    /// <summary>
    /// Main themes and topics identified in the passage
    /// </summary>
    [StringLength(2000)]
    public string TableThemes { get; set; } = string.Empty;

    /// <summary>
    /// Key words and phrases that stand out
    /// </summary>
    [StringLength(2000)]
    public string TableKeywords { get; set; } = string.Empty;

    /// <summary>
    /// Personal insights and spiritual lessons learned
    /// </summary>
    [StringLength(2000)]
    public string TableInsights { get; set; } = string.Empty;

    /// <summary>
    /// Practical application in daily life and spiritual growth
    /// </summary>
    [StringLength(2000)]
    public string TablePersonalApplication { get; set; } = string.Empty;

    /// <summary>
    /// Date when the memorization table was created
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date of last update to the memorization table
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Indicates if this entry has a structured memorization table
    /// </summary>
    public bool HasMemorizationTable { get; set; }
}
