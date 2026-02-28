using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using SecureOrders.Application.Auth;
using SecureOrders.Application.Common.Interfaces;
using SecureOrders.Infrastructure.Auth;
using SecureOrders.Infrastructure.Persistence;

namespace SecureOrders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("DefaultConnection");
        var redisConn = configuration["Redis:ConnectionString"];

        if (string.IsNullOrWhiteSpace(conn))
            throw new InvalidOperationException("DefaultConnection is missing.");
        if (string.IsNullOrWhiteSpace(redisConn))
            throw new InvalidOperationException("Redis:ConnectionString is missing.");

        services.AddDbContext<ApplicationDbContext>(opt =>
            opt.UseNpgsql(conn));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConn));

        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<AuthOptions>>().Value);
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenStore, RedisRefreshTokenStore>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
