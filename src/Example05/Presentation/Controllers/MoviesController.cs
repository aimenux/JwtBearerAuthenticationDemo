using Example05.Infrastructure.Repositories;
using Example05.Presentation.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Example05.Presentation.Controllers;

[ApiController]
[Route("api/movies")]
public class MoviesController : ControllerBase
{
    private readonly IMovieRepository _repository;

    public MoviesController(IMovieRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    [ServiceFilter(typeof(JwtSecurityFilter))]
    [HttpGet("list")]
    public async Task<IActionResult> GetMoviesAsync(CancellationToken cancellationToken)
    {
        var movies = await _repository.GetMoviesAsync(cancellationToken);
        return Ok(movies);
    }
    
    [JwtSecurityServiceFilter]
    [HttpGet("{movieId:int}")]
    public async Task<IActionResult> GetMovieByIdAsync([FromRoute] int movieId, CancellationToken cancellationToken)
    {
        var movie = await _repository.GetMovieByIdAsync(movieId, cancellationToken);
        return movie is null ? NotFound() : Ok(movie);
    }
}