namespace Api.Domain.Feed;

record FeedDto(string Title, string Url);

public class Feed
{
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required string Url { get; set; }
}
