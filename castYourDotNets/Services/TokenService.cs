using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using castYourDotNets.Contracts;
using castYourDotNets.Models;
using castYourDotNets.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace castYourDotNets.Services;

public sealed class TokenService
{
    private readonly JwtOptions jwtOptions;

    public TokenService(IOptions<JwtOptions> jwtOptions)
    {
        this.jwtOptions = jwtOptions.Value;
    }

    public AuthenticationResponse CreateAuthenticationResponse(UserAccount account)
    {
        var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(jwtOptions.ExpirationMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new Claim(ClaimTypes.Name, account.Username),
            new Claim(JwtRegisteredClaimNames.UniqueName, account.Username)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        return new AuthenticationResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAtUtc,
            Account = new AccountResponse
            {
                Id = account.Id,
                Username = account.Username,
                CreatedAtUtc = account.CreatedAtUtc
            }
        };
    }
}