using System.ComponentModel.DataAnnotations;

public class RegisterModel
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public required string Password { get; set; }

        [Required]
        public required string ConfirmPassword { get; set; }

        public string? ServerError { get; set; }
    }