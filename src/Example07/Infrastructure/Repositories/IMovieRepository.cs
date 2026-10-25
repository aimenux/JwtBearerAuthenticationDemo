using Example07.Domain;

namespace Example07.Infrastructure.Repositories;

public interface IMovieRepository
{
    Task<IReadOnlyCollection<Movie>> GetMoviesAsync(CancellationToken cancellationToken);
    Task<Movie> GetMovieByIdAsync(int movieId, CancellationToken cancellationToken);
}