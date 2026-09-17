namespace Api.Domain.Feed;

public class Feed
{
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Url { get; set; }
}