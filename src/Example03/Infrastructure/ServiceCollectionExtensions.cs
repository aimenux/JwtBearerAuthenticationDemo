using Example03.Infrastructure.Repositories;

namespace Example03.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        services.AddScoped<IMovieRepository, MovieRepository>();
        return builder;
    }
}