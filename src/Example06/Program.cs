using Example06;
using Example06.Infrastructure;
using Example06.Presentation;
using Example06.Presentation.Authentication;
using Example06.Presentation.Endpoints;

var app = WebApplication.CreateBuilder(args, enforceHttps: true)
    .AddPresentation()
    .AddInfrastructure()
    .Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHsts();

app.UseHttpsRedirection();

var appGroup = app
    .MapGroup("/api/movies")
    .AddEndpointFilter<JwtSecurityFilter>()
    .WithGroupName("Movies");

appGroup
    .MapGet("list", (IMoviesEndpoints endpoints, CancellationToken cancellationToken) => endpoints.GetMoviesAsync(cancellationToken))
    .WithName("GetMovies");

appGroup
    .MapGet("{movieId:int}", (IMoviesEndpoints endpoints, int movieId, CancellationToken cancellationToken) => endpoints.GetMovieByIdAsync(movieId, cancellationToken))
    .WithName("GetMovieById");

app.Run();