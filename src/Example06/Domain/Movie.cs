namespace Example06.Domain;

public sealed record Movie
{
    public int Id { get; init; }
    public string Title { get; init; }
}