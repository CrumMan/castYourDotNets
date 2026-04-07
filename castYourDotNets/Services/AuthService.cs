using castYourDotNets.Services;
using castYourDotNets.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

public class AuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AccountRegistrationResult> Register(string email, string password)
    {
        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            return AccountRegistrationResult.Failure(new Dictionary<string, string[]>
            {
                { "Register", new[] { "User already exists" } }
            });
        }

        var user = new User
        {
            Id = Guid.NewGuid(), // make sure your User model supports this
            Email = email,
            PasswordHash = HashPassword(password),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var accountResponse = new AccountResponse
        {
            Id = user.Id,
            Username = user.Email, // since no username field yet
            CreatedAtUtc = user.CreatedAtUtc
        };

        return AccountRegistrationResult.Success(accountResponse);
    }

    public async Task<LoginResult> Login(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || user.PasswordHash != HashPassword(password))
        {
            return LoginResult.Failure(new Dictionary<string, string[]>
            {
                { "Login", new[] { "Invalid email or password" } }
            });
        }

        var accountResponse = new AccountResponse
        {
            Id = user.Id,
            Username = user.Email,
            CreatedAtUtc = user.CreatedAtUtc
        };

        var authResponse = new AuthenticationResponse
        {
            AccessToken = GenerateToken(),
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddHours(1),
            Account = accountResponse
        };

        return LoginResult.Success(authResponse);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private string GenerateToken()
    {
        // simple token for now (you can upgrade to JWT later)
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}