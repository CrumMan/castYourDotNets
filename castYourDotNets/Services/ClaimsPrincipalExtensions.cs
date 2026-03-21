using System.Security.Claims;

namespace castYourDotNets.Services;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetRequiredUserId(this ClaimsPrincipal principal)
    {
        // Centralize claim parsing so endpoint handlers do not repeat this logic.
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new InvalidOperationException("Authenticated user identifier is missing or invalid.");
    }
}