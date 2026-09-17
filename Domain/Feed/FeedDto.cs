using System.ComponentModel.DataAnnotations;

namespace Api.Domain.Feed;

public record FeedDto(int Id, string Title, string Url);

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