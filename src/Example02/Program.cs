using Example02;
using Example02.Infrastructure;
using Example02.Presentation;
using Example02.Presentation.Authentication;
using Example02.Presentation.Endpoints;

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

app.UseJwtAuthentication();

app
    .MapGet("/api/movies/list", (IMoviesEndpoints endpoints, CancellationToken cancellationToken) => endpoints.GetMoviesAsync(cancellationToken))
    .WithName("GetMovies");

app
    .MapGet("/api/movies/{movieId:int}", (IMoviesEndpoints endpoints, int movieId, CancellationToken cancellationToken) => endpoints.GetMovieByIdAsync(movieId, cancellationToken))
    .WithName("GetMovieById");

app.Run();