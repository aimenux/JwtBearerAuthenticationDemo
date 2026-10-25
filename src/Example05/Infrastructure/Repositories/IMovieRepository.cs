using Example05.Domain;

namespace Example05.Infrastructure.Repositories;

public interface IMovieRepository
{
    Task<IReadOnlyCollection<Movie>> GetMoviesAsync(CancellationToken cancellationToken);
    Task<Movie> GetMovieByIdAsync(int movieId, CancellationToken cancellationToken);
}