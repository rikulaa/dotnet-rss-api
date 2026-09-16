using System.ComponentModel.DataAnnotations;

namespace Api.Domain.Feed;

public static class Controller
{
    public static WebApplication MapFeedEndpoints(this WebApplication app)
    {
        // Get feeds
        app.MapGet("/feed", () =>
        {
            return Enumerable.Range(1, 5).Select(index =>
                {
                    return new FeedDto(
                                    "Your RSS feed",
                                    "https://localhost"
                            );
                })
                .ToArray();
        });

        app.MapGet("/feed/{id}", (
            [Required(ErrorMessage = "Invalid id")] int id
            ) =>
        {
                    return new FeedDto(
                                    "Your RSS feed" + id,
                                    "https://localhost"
                            );
        });

        return app;
    }
}
