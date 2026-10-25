using Example09.Domain;

namespace Example09.Infrastructure.Repositories;

public interface IMovieRepository
{
    Task<IReadOnlyCollection<Movie>> GetMoviesAsync(CancellationToken cancellationToken);
    Task<Movie> GetMovieByIdAsync(int movieId, CancellationToken cancellationToken);
}