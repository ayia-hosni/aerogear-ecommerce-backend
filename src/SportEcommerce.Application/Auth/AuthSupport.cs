using SportEcommerce.Application.Abstractions;
using SportEcommerce.Domain.Entities;

namespace SportEcommerce.Application.Auth;

internal static class AuthSupport
{
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static AuthResponse IssueFor(this IJwtTokenGenerator tokens, User user)
    {
        var token = tokens.Generate(user);
        return new AuthResponse(token.Value, token.ExpiresAt, new UserDto(user.Id, user.FullName, user.Email));
    }
}
