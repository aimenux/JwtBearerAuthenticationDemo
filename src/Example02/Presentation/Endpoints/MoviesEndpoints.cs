using Example02.Infrastructure.Repositories;

namespace Example02.Presentation.Endpoints;

public interface IMoviesEndpoints
{
    Task<IResult> GetMoviesAsync(CancellationToken cancellationToken);
    Task<IResult> GetMovieByIdAsync(int movieId, CancellationToken cancellationToken);
}

public class MoviesEndpoints : IMoviesEndpoints
{
    private readonly IMovieRepository _repository;
    private readonly ILogger<MoviesEndpoints> _logger;

    public MoviesEndpoints(IMovieRepository repository, ILogger<MoviesEndpoints> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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