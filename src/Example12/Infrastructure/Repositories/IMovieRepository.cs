using Example12.Domain;

namespace Example12.Infrastructure.Repositories;

public interface IMovieRepository
{
    Task<IReadOnlyCollection<Movie>> GetMoviesAsync(CancellationToken cancellationToken);
    Task<Movie> GetMovieByIdAsync(int movieId, CancellationToken cancellationToken);
}