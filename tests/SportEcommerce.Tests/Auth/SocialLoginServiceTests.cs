using SportEcommerce.Application.Auth;
using SportEcommerce.Application.Common;
using SportEcommerce.Tests.Support;

namespace SportEcommerce.Tests.Auth;

public sealed class SocialLoginServiceTests : IAsyncDisposable
{
    private readonly TestDatabase _database = new();

    [Fact]
    public async Task FirebaseLogin_CreatesUser_ThenReusesIt()
    {
        await using var db = _database.CreateContext();
        var social = new ServiceFactory(db, new FakeFirebaseVerifier(new("uid-1", "Sam@Example.com", true, "Sam"))).Social;

        var first = await social.FirebaseLoginAsync(new FirebaseLoginRequest("token"), CancellationToken.None);
        var second = await social.FirebaseLoginAsync(new FirebaseLoginRequest("token"), CancellationToken.None);

        Assert.Equal("sam@example.com", first.User.Email);
        Assert.Equal(first.User.Id, second.User.Id);
    }

    [Fact]
    public async Task FirebaseLogin_LinksExistingPasswordAccount_ByVerifiedEmail()
    {
        await using var db = _database.CreateContext();
        var password = new ServiceFactory(db).Auth;
        var registered = await password.RegisterAsync(new RegisterRequest("Jane", "jane@example.com", "Passw0rd!"), CancellationToken.None);

        var social = new ServiceFactory(db, new FakeFirebaseVerifier(new("uid-2", "jane@example.com", true, "Jane"))).Social;
        var login = await social.FirebaseLoginAsync(new FirebaseLoginRequest("token"), CancellationToken.None);

        Assert.Equal(registered.User.Id, login.User.Id);
    }

    [Fact]
    public async Task FirebaseLogin_RejectsUnverifiedEmail()
    {
        await using var db = _database.CreateContext();
        var social = new ServiceFactory(db, new FakeFirebaseVerifier(new("uid-3", "x@example.com", false, null))).Social;

        await Assert.ThrowsAsync<BusinessRuleException>(() => social.FirebaseLoginAsync(new FirebaseLoginRequest("token"), CancellationToken.None));
    }

    [Fact]
    public async Task PasswordLogin_RejectsSocialOnlyAccount()
    {
        await using var db = _database.CreateContext();
        var services = new ServiceFactory(db, new FakeFirebaseVerifier(new("uid-4", "s@example.com", true, "S")));
        await services.Social.FirebaseLoginAsync(new FirebaseLoginRequest("token"), CancellationToken.None);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => services.Auth.LoginAsync(new LoginRequest("s@example.com", "anything"), CancellationToken.None));
    }

    public ValueTask DisposeAsync() => _database.DisposeAsync();
}
