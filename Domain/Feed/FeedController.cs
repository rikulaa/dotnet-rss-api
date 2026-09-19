using System.ComponentModel.DataAnnotations;
using System.Threading.Channels;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

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
                            feed => feed.ToDto()
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

            return TypedResults.Ok(feed.ToDto());
        });

        // Get single feed
        feedsApp.MapGet("/{id}", async Task<Results<Ok<FeedDto>, NotFound>> (
                    [Required(ErrorMessage = "Invalid id")] int id,
                    Include? include,
                    AppContext db
                ) =>
                {
                    Console.WriteLine("includes: " + include?.Values);

                    var q = db.Feeds.AsQueryable();
                    if (include is not null && include.Values.Contains("item"))
                    {
                        q = q.Include(f => f.Items);
                    }
                    var feed = await q.FirstOrDefaultAsync(f => f.Id == id);

                    if (feed is null) return TypedResults.NotFound();

                    return TypedResults.Ok(feed.ToDto(include?.Values));
                });

        // Update single feed
        feedsApp.MapPut("/{id}", async Task<Results<Ok<FeedDto>, NotFound>> ([Required(ErrorMessage = "Invalid id")] int id, UpdateFeedRequest input, AppContext db) =>
                {
                    var feed = await db.Feeds.FindAsync(id);

                    if (feed is null) return TypedResults.NotFound();

                    if (input.Url is not null) feed.Url = input.Url;
                    if (input.Title is not null) feed.Title = input.Title;

                    await db.SaveChangesAsync();

                    return TypedResults.Ok(feed.ToDto());
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

        // Delete feed
        feedsApp.MapDelete("/{id}", async Task<Results<NoContent, NotFound>> ([Required(ErrorMessage = "Invalid id")] int id, AppContext db) =>
                {
                    var feed = await db.Feeds.FindAsync(id);

                    if (feed is null) return TypedResults.NotFound();

                    db.Feeds.Remove(feed);
                    await db.SaveChangesAsync();

                    return TypedResults.NoContent();
                });


        return app;
    }
}

public class Include
{
    public required IEnumerable<string> Values;

    public static bool TryParse(string? value, out Include? result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = null;
            return true;
        }

        var values = value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        result =  new Include{ Values = values };
        return true;
    }
}