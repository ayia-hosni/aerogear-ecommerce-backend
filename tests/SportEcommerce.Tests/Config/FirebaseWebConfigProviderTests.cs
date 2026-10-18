using Microsoft.Extensions.Options;
using SportEcommerce.Infrastructure.Security;

namespace SportEcommerce.Tests.Config;

public sealed class FirebaseWebConfigProviderTests
{
    private static FirebaseWebConfigProvider Create(FirebaseOptions options) => new(Options.Create(options));

    [Fact]
    public void Get_ReturnsNull_WhenNotConfigured() =>
        Assert.Null(Create(new FirebaseOptions()).Get());

    [Fact]
    public void Get_ReturnsPublicSettings_WhenConfigured()
    {
        var config = Create(new FirebaseOptions { ProjectId = "demo", ApiKey = "public-key", AuthDomain = "demo.example.test", AppId = "app" }).Get();

        Assert.NotNull(config);
        Assert.Equal("demo", config.ProjectId);
        Assert.Equal("demo.example.test", config.AuthDomain);
    }
}
