using SportEcommerce.Domain.Entities;

namespace SportEcommerce.Application.Abstractions;

public interface IJwtTokenGenerator
{
    AccessToken Generate(User user);
}

public sealed record AccessToken(string Value, DateTime ExpiresAt);
