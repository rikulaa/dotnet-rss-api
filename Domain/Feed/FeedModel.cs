namespace Api.Domain.Feed;

public class Feed
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Url { get; set; }

    public ICollection<Item> Items { get; set; } = [];
}

public class Item
{

    public int Id { get; set; }

    public int FeedId { get; set; }
    public Feed Feed { get; set; } = null!;

    public string? Guid { get; set; }

    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Author { get; set; }
    public string? Description { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }
}