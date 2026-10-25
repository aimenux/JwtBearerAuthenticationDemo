using Example03.Domain;
using Example03.Infrastructure.Repositories;
using AwesomeAssertions;

namespace Example03.Tests.UnitTests;

public class MovieRepositoryTests
{
    [Fact]
    public async Task Should_Not_Allow_Returned_Movies_To_Be_Cast_To_Mutable_List()
    {
        // Arrange
        var repository = new MovieRepository();

        // Act
        var movies = await repository.GetMoviesAsync(CancellationToken.None);

        // Assert
        var act = () => (List<Movie>)movies;
        act.Should().Throw<InvalidCastException>();
    }

    [Fact]
    public async Task Should_Not_Allow_Mutating_Returned_Movies()
    {
        // Arrange
        var repository = new MovieRepository();
        var movies = await repository.GetMoviesAsync(CancellationToken.None);
        var mutableView = (IList<Movie>)movies;

        // Act
        var act = () => mutableView.Add(new Movie { Id = 3, Title = "Injected" });

        // Assert
        act.Should().Throw<NotSupportedException>();
        movies.Should().HaveCount(2);
    }
}
