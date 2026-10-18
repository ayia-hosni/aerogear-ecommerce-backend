using Microsoft.EntityFrameworkCore;
using SportEcommerce.Application.Abstractions;
using SportEcommerce.Application.Common;
using SportEcommerce.Domain.Entities;

namespace SportEcommerce.Application.Auth;

public interface ISocialLoginService
{
    Task<AuthResponse> FirebaseLoginAsync(FirebaseLoginRequest request, CancellationToken cancellationToken);
}

internal sealed class SocialLoginService(
    IAppDbContext db,
    IJwtTokenGenerator tokenGenerator,
    IFirebaseTokenVerifier firebaseVerifier) : ISocialLoginService
{
    public async Task<AuthResponse> FirebaseLoginAsync(FirebaseLoginRequest request, CancellationToken cancellationToken)
    {
        var identity = await firebaseVerifier.VerifyAsync(request.IdToken, cancellationToken);

        var user = await db.Users.FirstOrDefaultAsync(u => u.FirebaseUid == identity.Uid, cancellationToken);
        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(identity.Email) || !identity.EmailVerified)
            {
                throw new BusinessRuleException("Your social account must share a verified email address.");
            }

            var email = AuthSupport.NormalizeEmail(identity.Email);

            // Link to an existing account only when the provider vouches for the email, otherwise anyone could claim it.
            user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            if (user is null)
            {
                user = new User
                {
                    FullName = string.IsNullOrWhiteSpace(identity.Name) ? email.Split('@')[0] : identity.Name.Trim(),
                    Email = email
                };
                db.Users.Add(user);
            }

            user.FirebaseUid = identity.Uid;
            await db.SaveChangesAsync(cancellationToken);
        }

        return tokenGenerator.IssueFor(user);
    }
}
