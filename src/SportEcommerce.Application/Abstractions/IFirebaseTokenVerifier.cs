using SportEcommerce.Application.Common;

namespace SportEcommerce.Application.Abstractions;

public interface IFirebaseTokenVerifier
{
    /// <exception cref="InvalidCredentialsException">The token is malformed, expired or not issued for this project.</exception>
    Task<FirebaseIdentity> VerifyAsync(string idToken, CancellationToken cancellationToken);
}

public sealed record FirebaseIdentity(string Uid, string? Email, bool EmailVerified, string? Name);
