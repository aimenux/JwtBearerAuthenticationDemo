using Example12;
using Example12.Infrastructure;
using Example12.Presentation;
using Example12.Presentation.Endpoints;

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

app.UseAuthentication();
app.UseAuthorization();

var appGroup = app
    .MapGroup("/api/movies")
    .RequireAuthorization()
    .WithGroupName("Movies");

appGroup
    .MapGet("list", (IMoviesEndpoints endpoints, CancellationToken cancellationToken) => endpoints.GetMoviesAsync(cancellationToken))
    .WithName("GetMovies");

appGroup
    .MapGet("{movieId:int}", (IMoviesEndpoints endpoints, int movieId, CancellationToken cancellationToken) => endpoints.GetMovieByIdAsync(movieId, cancellationToken))
    .WithName("GetMovieById");

app.Run();