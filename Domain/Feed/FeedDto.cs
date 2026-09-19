using System.ComponentModel.DataAnnotations;

namespace Api.Domain.Feed;

public static class FeedExtensions
{
    public static FeedDto ToDto(this Feed feed)
    {
        return TransformFeed(feed, Array.Empty<string>());
    }
    public static FeedDto ToDto(this Feed feed, IEnumerable<string>? include)
    {
        return TransformFeed(feed, include);
    }
    private static FeedDto TransformFeed(Feed feed, IEnumerable<string>? include) {
        var includeItem = include?.Contains("item") == true;

        return new FeedDto(
                feed.Id,
                feed.Title,
                feed.Url,
                includeItem ? feed.Items.Select(i => i.toDto()) : null
                );

    }

    public static ItemDto toDto(this Item item)
    {
        return new ItemDto(
                item.Id,
                item.FeedId,
                item.Guid,
                item.Title,
                item.Content,
                item.Author,
                item.Description
                );
    }
}

public record ItemDto(
    int Id,
    int FeedId,
    string? Guid,
    string? Title,
    string? Content,
    string? Author,
    string? Description
);

public record FeedDto(int Id, string Title, string Url, IEnumerable<ItemDto>? items = null);

public record CreateFeedRequest(
        [Required]
        [MinLength(1)]
        [MaxLength(255)]
        string Title,

        [Required]
        [MinLength(1)]
        [MaxLength(255)]
        string Url
        );

public record UpdateFeedRequest(
        [MinLength(1)]
        [MaxLength(255)]
        string? Title,

        [MinLength(1)]
        [MaxLength(255)]
        string? Url
        );