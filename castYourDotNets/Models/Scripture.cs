using System.ComponentModel.DataAnnotations;

namespace castYourDotNets.Models;

public class Scripture
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(150)]
    public string Reference { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Text { get; set; } = string.Empty;

    [StringLength(120)]
    public string Topic { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
