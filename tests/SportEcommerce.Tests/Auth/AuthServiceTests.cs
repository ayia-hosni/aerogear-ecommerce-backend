using SportEcommerce.Application.Auth;
using SportEcommerce.Application.Common;
using SportEcommerce.Tests.Support;

namespace SportEcommerce.Tests.Auth;

public sealed class AuthServiceTests : IAsyncDisposable
{
    private readonly TestDatabase _database = new();

    [Fact]
    public async Task RegisterThenLogin_ReturnsToken_AndNormalizesEmail()
    {
        await using var db = _database.CreateContext();
        var auth = new ServiceFactory(db).Auth;

        await auth.RegisterAsync(new RegisterRequest("Jane Doe", " Jane@Example.com ", "Passw0rd!"), CancellationToken.None);
        var login = await auth.LoginAsync(new LoginRequest("jane@example.com", "Passw0rd!"), CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
        Assert.Equal("jane@example.com", login.User.Email);
    }

    [Fact]
    public async Task Register_RejectsDuplicateEmail()
    {
        await using var db = _database.CreateContext();
        var auth = new ServiceFactory(db).Auth;
        var request = new RegisterRequest("Jane Doe", "jane@example.com", "Passw0rd!");

        await auth.RegisterAsync(request, CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() => auth.RegisterAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task Login_RejectsWrongPassword()
    {
        await using var db = _database.CreateContext();
        var auth = new ServiceFactory(db).Auth;
        await auth.RegisterAsync(new RegisterRequest("Jane Doe", "jane@example.com", "Passw0rd!"), CancellationToken.None);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            auth.LoginAsync(new LoginRequest("jane@example.com", "wrong-password"), CancellationToken.None));
    }

    public ValueTask DisposeAsync() => _database.DisposeAsync();
}
