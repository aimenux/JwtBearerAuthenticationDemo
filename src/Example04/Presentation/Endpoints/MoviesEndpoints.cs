using Example04.Infrastructure.Repositories;

namespace Example04.Presentation.Endpoints;

public interface IMoviesEndpoints
{
    Task<IResult> GetMoviesAsync(CancellationToken cancellationToken);
    Task<IResult> GetMovieByIdAsync(int movieId, CancellationToken cancellationToken);
}

public class MoviesEndpoints : IMoviesEndpoints
{
    private readonly IMovieRepository _repository;

    public MoviesEndpoints(IMovieRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IResult> GetMoviesAsync(CancellationToken cancellationToken)
    {
        var movies = await _repository.GetMoviesAsync(cancellationToken);
        return Results.Ok(movies);
    }

    public async Task<IResult> GetMovieByIdAsync(int movieId, CancellationToken cancellationToken)
    {
        var movie = await _repository.GetMovieByIdAsync(movieId, cancellationToken);
        return movie is null ? Results.NotFound() : Results.Ok(movie);
    }
}