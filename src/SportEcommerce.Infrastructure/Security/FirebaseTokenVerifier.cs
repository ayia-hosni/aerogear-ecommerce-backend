using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SportEcommerce.Application.Abstractions;
using SportEcommerce.Application.Common;

namespace SportEcommerce.Infrastructure.Security;

internal sealed class FirebaseTokenVerifier : IFirebaseTokenVerifier
{
    private readonly JsonWebTokenHandler _handler = new();
    private readonly Lazy<Settings> _settings;

    // Resolved on first use so the API still boots (password login keeps working) before Firebase is configured.
    public FirebaseTokenVerifier(IOptions<FirebaseOptions> options) =>
        _settings = new Lazy<Settings>(() =>
        {
            var projectId = options.Value.ProjectId;
            if (string.IsNullOrWhiteSpace(projectId))
            {
                throw new InvalidOperationException("Firebase:ProjectId is not configured.");
            }

            var issuer = $"https://securetoken.google.com/{projectId}";
            return new Settings(projectId, issuer, new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{issuer}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever()));
        });

    public async Task<FirebaseIdentity> VerifyAsync(string idToken, CancellationToken cancellationToken)
    {
        var (projectId, issuer, manager) = _settings.Value;
        var configuration = await manager.GetConfigurationAsync(cancellationToken);

        var result = await _handler.ValidateTokenAsync(idToken, new TokenValidationParameters
        {
            ValidIssuer = issuer,
            ValidAudience = projectId,
            IssuerSigningKeys = configuration.SigningKeys,
            ClockSkew = TimeSpan.FromSeconds(30)
        });

        if (!result.IsValid || result.SecurityToken is not JsonWebToken token)
        {
            throw new InvalidCredentialsException();
        }

        // Firebase puts the user id in "sub"; "user_id" is a duplicate kept for legacy clients.
        if (string.IsNullOrEmpty(token.Subject))
        {
            throw new InvalidCredentialsException();
        }

        token.TryGetPayloadValue<string>("email", out var email);
        token.TryGetPayloadValue<bool>("email_verified", out var emailVerified);
        token.TryGetPayloadValue<string>("name", out var name);

        return new FirebaseIdentity(token.Subject, email, emailVerified, name);
    }

    private sealed record Settings(string ProjectId, string Issuer, ConfigurationManager<OpenIdConnectConfiguration> Manager);
}
