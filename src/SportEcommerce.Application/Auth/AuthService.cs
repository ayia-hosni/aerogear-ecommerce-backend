using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportEcommerce.Application.Abstractions;
using SportEcommerce.Application.Common;
using SportEcommerce.Domain.Entities;

namespace SportEcommerce.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<UserDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken);
}

internal sealed class AuthService(
    IAppDbContext db,
    IPasswordHasher<User> passwordHasher,
    IJwtTokenGenerator tokenGenerator) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = AuthSupport.NormalizeEmail(request.Email);

        if (await db.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = null
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return tokenGenerator.IssueFor(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = AuthSupport.NormalizeEmail(request.Email);

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
                   ?? throw new InvalidCredentialsException();

        if (user.PasswordHash is null)
        {
            throw new InvalidCredentialsException();
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new InvalidCredentialsException();
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            await db.SaveChangesAsync(cancellationToken);
        }

        return tokenGenerator.IssueFor(user);
    }

    public async Task<UserDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken) =>
        await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserDto(u.Id, u.FullName, u.Email))
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException(nameof(User), userId);
}
