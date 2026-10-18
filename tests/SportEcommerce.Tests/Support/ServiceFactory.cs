using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SportEcommerce.Application;
using SportEcommerce.Application.Abstractions;
using SportEcommerce.Application.Auth;
using SportEcommerce.Application.Common;
using SportEcommerce.Application.Orders;
using SportEcommerce.Infrastructure.Persistence;
using SportEcommerce.Infrastructure.Security;

namespace SportEcommerce.Tests.Support;

internal sealed class ServiceFactory
{
    private readonly ServiceProvider _provider;

    public ServiceFactory(AppDbContext db, IFirebaseTokenVerifier? firebase = null)
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddSingleton<IAppDbContext>(db);
        services.AddSingleton(Options.Create(new JwtOptions
        {
            Issuer = "tests",
            Audience = "tests",
            SigningKey = new string('k', 48),
            ExpiryMinutes = 5
        }));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton(firebase ?? new FakeFirebaseVerifier(null));
        _provider = services.BuildServiceProvider();
    }

    public IOrderService Orders => _provider.GetRequiredService<IOrderService>();
    public ISocialLoginService Social => _provider.GetRequiredService<ISocialLoginService>();
    public IAuthService Auth => _provider.GetRequiredService<IAuthService>();
}

internal sealed class FakeFirebaseVerifier(FirebaseIdentity? identity) : IFirebaseTokenVerifier
{
    public Task<FirebaseIdentity> VerifyAsync(string idToken, CancellationToken cancellationToken) =>
        Task.FromResult(identity ?? throw new InvalidCredentialsException());
}
