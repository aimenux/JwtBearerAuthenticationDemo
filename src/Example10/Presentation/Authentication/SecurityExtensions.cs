using Microsoft.AspNetCore.Authentication;

namespace Example10.Presentation.Authentication;

public static class SecurityExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = JwtConstants.BearerScheme;
                options.DefaultChallengeScheme = JwtConstants.BearerScheme;
                options.DefaultAuthenticateScheme = JwtConstants.BearerScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, JwtAuthenticationHandler>(JwtConstants.BearerScheme, _ => { });

        return services;
    }
}
