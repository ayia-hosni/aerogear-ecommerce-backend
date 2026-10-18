using SportEcommerce.Application.Config;

namespace SportEcommerce.Application.Abstractions;

public interface IFirebaseWebConfigProvider
{
    /// <returns>The web config, or <c>null</c> when Firebase login is not configured.</returns>
    FirebaseWebConfig? Get();
}
