using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace SportEcommerce.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(value, out var id)
            ? id
            : throw new InvalidOperationException("Authenticated principal has no valid user id claim.");
    }
}
