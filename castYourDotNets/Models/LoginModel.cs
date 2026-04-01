using System.ComponentModel.DataAnnotations;

public class LoginModel
{
    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = "";

    // Backend errors (like invalid credentials)
    public string? ServerError { get; set; }
}