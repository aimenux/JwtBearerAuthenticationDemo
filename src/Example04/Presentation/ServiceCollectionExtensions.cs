using Example04.Presentation.Endpoints;
using Microsoft.Extensions.Options;
using Example04.Presentation.Authentication;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Example04.Presentation;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddPresentation(this WebApplicationBuilder builder)
    {
        builder.Services.AddJwtOptions();

        builder.Services.AddEndpoints();
        builder.Services.AddSwagger();
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
