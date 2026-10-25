using Example08.Presentation.Authentication;
using Example08.Presentation.Endpoints;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Example08.Presentation;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddPresentation(this WebApplicationBuilder builder)
    {
        builder.Services.AddJwtOptions();

        builder.Services.AddEndpoints();
        builder.Services.AddSwagger();
        builder.Services.AddJwtAuthentication();
        builder.Services.AddAuthorization();
        return builder;
    }

    private static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        services.AddScoped<IMoviesEndpoints, MoviesEndpoints>();
        return services;
    }

    private static IServiceCollection AddJwtOptions(this IServiceCollection services)
    {
        services
            .AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();
        services.AddSingleton<JwtTokenValidator>();
        return services;
    }

    private static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }
}
