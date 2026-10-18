using Microsoft.Extensions.Options;
using SportEcommerce.Application.Abstractions;
using SportEcommerce.Application.Config;

namespace SportEcommerce.Infrastructure.Security;

internal sealed class FirebaseWebConfigProvider(IOptions<FirebaseOptions> options) : IFirebaseWebConfigProvider
{
    public FirebaseWebConfig? Get()
    {
        var o = options.Value;
        return string.IsNullOrWhiteSpace(o.ProjectId) || string.IsNullOrWhiteSpace(o.ApiKey)
            ? null
            : new FirebaseWebConfig(o.ApiKey, o.AuthDomain, o.ProjectId, o.AppId);
    }
}
