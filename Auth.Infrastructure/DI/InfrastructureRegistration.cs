using Auth.Application.Interfaces;
using Auth.Application.Services;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.EF;
using Auth.Infrastructure.EF.Repositories;
using Auth.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure.DI;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddAuthApplicationCore(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenService>();
        return services;
    }

    public static IServiceCollection AddAuthData(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddDbContext<UsersDbContext>(o =>
            o.UseNpgsql(cfg.GetConnectionString("AuthDb")));
        services.AddScoped<IUserRepository, EfUserRepository>();
        return services;
    }
}
