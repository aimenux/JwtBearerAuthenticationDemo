using Example11.Domain;
using Example11.Infrastructure.Repositories;
using Example11.Presentation.Controllers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Example11.Tests.UnitTests;

public class MoviesControllerTests
{
    [Fact]
    public async Task Should_Get_Movies_Returns_Success()
    {
        // Arrange
        var repository = Substitute.For<IMovieRepository>();
        repository
            .GetMoviesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Movie>
            {
                new Movie
                {
                    Id = 1,
                    Title = "Matrix"
                }
            });
        var controller = new MoviesController(repository);

        // Act
        var result = await controller.GetMoviesAsync(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        result
            .As<OkObjectResult>().Value
            .As<IReadOnlyCollection<Movie>>()
            .Should().NotBeNullOrEmpty();
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Should_Get_Movie_By_Id_Returns_Success(int movieId)
    {
        var repository = Substitute.For<IMovieRepository>();
        repository
            .GetMovieByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new Movie
            {
                Id = 1,
                Title = "Matrix"
            });
        var controller = new MoviesController(repository);

        var result = await controller.GetMovieByIdAsync(movieId, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        result
            .As<OkObjectResult>().Value
            .As<Movie>()
            .Should().NotBeNull();
    }    
}