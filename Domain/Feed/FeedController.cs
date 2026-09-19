using System.ComponentModel.DataAnnotations;
using System.Threading.Channels;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Domain.Feed;

public static class Controller
{
    public static WebApplication MapFeedEndpoints(this WebApplication app)
    {
        var feedsApp = app.MapGroup("/feeds");

        // Get feeds
        feedsApp.MapGet("/", (AppContext db) =>
                {
                    var feeds = db.Feeds.Select(
                            feed => new FeedDto(
                                feed.Id,
                                feed.Title,
                                feed.Url
                                )
                            ).ToList();
                    return feeds;
                });

        // Create a feed
        feedsApp.MapPost("/", async (CreateFeedRequest input, AppContext db, Channel<int> queue) =>
        {
            var feed = new Feed { Title = input.Title, Url = input.Url };
            db.Feeds.Add(feed);
            await db.SaveChangesAsync();

            await queue.Writer.WriteAsync(feed.Id);

            return TypedResults.Ok(
                    new FeedDto(
                        feed.Id,
                        feed.Title,
                        feed.Url
                        ));
        });

        // Get single feed
        feedsApp.MapGet("/{id}", async Task<Results<Ok<FeedDto>, NotFound>> ([Required(ErrorMessage = "Invalid id")] int id, AppContext db) =>
                {
                    var feed = await db.Feeds.FindAsync(id);

                    if (feed is null) return TypedResults.NotFound();

                    return TypedResults.Ok(
                            new FeedDto(
                                feed.Id,
                                feed.Title,
                                feed.Url
                                ));
                });

        // Update single feed
        feedsApp.MapPut("/{id}", async Task<Results<Ok<FeedDto>, NotFound>> ([Required(ErrorMessage = "Invalid id")] int id, UpdateFeedRequest input, AppContext db) =>
                {
                    var feed = await db.Feeds.FindAsync(id);

                    if (feed is null) return TypedResults.NotFound();

                    if (input.Url is not null) feed.Url = input.Url;
                    if (input.Title is not null) feed.Title = input.Title;

                    await db.SaveChangesAsync();

                    return TypedResults.Ok(
                            new FeedDto(
                                feed.Id,
                                feed.Title,
                                feed.Url
                                ));
                });

        // Refresh feed
        feedsApp.MapPut("/{id}/refresh", async Task<Results<NoContent, NotFound>> ([Required(ErrorMessage = "Invalid id")] int id, AppContext db, Channel<int> queue) =>
                {
                    var feed = await db.Feeds.FindAsync(id);

                    if (feed is null) return TypedResults.NotFound();

                    // Will refetch feed items
                    await queue.Writer.WriteAsync(feed.Id);

                    return TypedResults.NoContent();
                });


        return app;
    }
}