using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportEcommerce.Application.Abstractions;
using SportEcommerce.Infrastructure.Persistence;
using SportEcommerce.Infrastructure.Security;

namespace SportEcommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")
                              ?? throw new InvalidOperationException("Connection string 'Default' is not configured.")));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddOptions<FirebaseOptions>()
            .Bind(configuration.GetSection(FirebaseOptions.SectionName));

        services.AddSingleton<IFirebaseTokenVerifier, FirebaseTokenVerifier>();
        services.AddSingleton<IFirebaseWebConfigProvider, FirebaseWebConfigProvider>();

        return services;
    }
}
