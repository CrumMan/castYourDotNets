using System.ComponentModel.DataAnnotations;

namespace castYourDotNets.Models;

public class MemorizationEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public UserAccount? User { get; set; }

    [Required]
    [StringLength(2000)]
    public string GameText { get; set; } = string.Empty;

    public bool IsMemorized { get; set; }

    public bool IsMemorizedThroughGame { get; set; }
}
