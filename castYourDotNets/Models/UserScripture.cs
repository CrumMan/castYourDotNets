using System.ComponentModel.DataAnnotations;

namespace castYourDotNets.Models;

/// <summary>
/// Tracks each user's progress with individual scriptures.
/// Central point for personalizing scripture experience, tracking progress, and supporting game functionality.
/// </summary>
public class UserScripture
{
    /// <summary>
    /// Unique identifier for this user-scripture association
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the user who is memorizing this scripture
    /// </summary>
    public Guid UserId { get; set; }
    public UserAccount? User { get; set; }

    /// <summary>
    /// Reference to the scripture being memorized
    /// </summary>
    public Guid ScriptureId { get; set; }
    public Scripture? Scripture { get; set; }

    /// <summary>
    /// Number of times the user has practiced this scripture
    /// </summary>
    public int PracticeCount { get; set; } = 0;

    /// <summary>
    /// Number of consecutive days the user has practiced this scripture
    /// </summary>
    public int CurrentStreakDays { get; set; } = 0;

    /// <summary>
    /// Maximum consecutive days streak achieved
    /// </summary>
    public int LongestStreakDays { get; set; } = 0;

    /// <summary>
    /// Date and time when this scripture was last practiced
    /// </summary>
    public DateTime? LastPracticedAtUtc { get; set; }

    /// <summary>
    /// Date and time when this scripture was marked as memorized
    /// </summary>
    public DateTime? MemorizedAtUtc { get; set; }

    /// <summary>
    /// Whether the user has fully memorized this scripture
    /// </summary>
    public bool IsMemorized { get; set; } = false;

    /// <summary>
    /// Difficulty rating given by the user (1-5 scale: 1=easy, 5=very hard)
    /// </summary>
    public int? DifficultyRating { get; set; }

    /// <summary>
    /// Personal notes or reflections about this scripture
    /// </summary>
    [StringLength(1000)]
    public string PersonalNotes { get; set; } = string.Empty;

    /// <summary>
    /// When this user-scripture relationship was first created
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this record was last updated
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User's success rate as a percentage (0-100)
    /// </summary>
    public int SuccessRate { get; set; } = 0;

    /// <summary>
    /// Number of successful attempts
    /// </summary>
    public int SuccessfulAttempts { get; set; } = 0;

    /// <summary>
    /// Number of failed attempts
    /// </summary>
    public int FailedAttempts { get; set; } = 0;

    /// <summary>
    /// Priority level for the user (1=high, 2=medium, 3=low)
    /// </summary>
    public int Priority { get; set; } = 2; // Default to medium priority

    /// <summary>
    /// Status of memorization progress
    /// Options: "NotStarted", "InProgress", "Completed", "Mastered"
    /// </summary>
    [StringLength(20)]
    public string Status { get; set; } = "NotStarted";
}
