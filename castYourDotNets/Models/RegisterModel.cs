using System.ComponentModel.DataAnnotations;

namespace castYourDotNets.Models;

public class RegisterModel
{
    [Required]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")]
    [RegularExpression(@"^[A-Za-z0-9._-]{3,32}$", ErrorMessage = "Username can only contain letters, numbers, dots, underscores, and hyphens.")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).*$", ErrorMessage = "Password must contain at least one letter and one number.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}