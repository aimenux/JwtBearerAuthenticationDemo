using Example04.Domain;
using Example04.Infrastructure.Repositories;
using Example04.Presentation.Endpoints;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;

namespace Example04.Tests.UnitTests;

public class MoviesEndpointsTests
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
        var endpoints = new MoviesEndpoints(repository);

        // Act
        var result = await endpoints.GetMoviesAsync(CancellationToken.None);

        // Assert
        result.Should().BeOfType<Ok<IReadOnlyCollection<Movie>>>();
        result
            .As<Ok<IReadOnlyCollection<Movie>>>().Value
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
        var endpoints = new MoviesEndpoints(repository);

        var result = await endpoints.GetMovieByIdAsync(movieId, CancellationToken.None);

        result.Should().BeOfType<Ok<Movie>>();
        result
            .As<Ok<Movie>>().Value
            .As<Movie>()
            .Should().NotBeNull();
    }    
}