using Example12.Infrastructure.Repositories;

namespace Example12.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        services.AddScoped<IMovieRepository, MovieRepository>();
        return builder;
    }
}