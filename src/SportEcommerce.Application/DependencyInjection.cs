using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SportEcommerce.Application.Auth;
using SportEcommerce.Application.Categories;
using SportEcommerce.Application.Orders;
using SportEcommerce.Application.Products;
using SportEcommerce.Domain.Entities;

namespace SportEcommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISocialLoginService, SocialLoginService>();
        services.AddScoped<IOrderService, OrderService>();
        return services;
    }
}
