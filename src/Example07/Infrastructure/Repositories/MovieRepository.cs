using Example07.Domain;

namespace Example07.Infrastructure.Repositories;

public class MovieRepository : IMovieRepository
{
    private static readonly List<Movie> Movies =
    [
        new Movie
        {
            Id = 1,
            Title = "Payback"
        },
        new Movie
        {
            Id = 2,
            Title = "Fearless"
        }
    ];

    public Task<IReadOnlyCollection<Movie>> GetMoviesAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyCollection<Movie>>(Movies.AsReadOnly());
    }

    public Task<Movie> GetMovieByIdAsync(int movieId, CancellationToken cancellationToken)
    {
        var movie = Movies.SingleOrDefault(x => x.Id == movieId);
        return Task.FromResult(movie);
    }
}