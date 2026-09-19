using Scalar.AspNetCore;
using Api.Domain.Feed;
using Api.Extensions;
using System.Threading.Channels;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add validation to minimal api endpoints via service extension: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/validation?view=aspnetcore-10.0
// Needs to be done like if endpoint mappings are not in the same assembly where 'AddValidation' is called
builder.Services.AddApiValidation();

// Add http client to container
builder.Services.AddHttpClient();

// Configure exceptions
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<AppContext>();

builder.Services.AddHostedService<FetchFeedBackgroundJob>();

var feedQueue = Channel.CreateUnbounded<int>();
builder.Services.AddSingleton(feedQueue);

builder.Services.AddSingleton<FeedService>();

var app = builder.Build();

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    StatusCodeSelector = exception => exception switch
    {
        BadHttpRequestException => StatusCodes.Status400BadRequest,
        // Voit lisätä tähän muitakin:
        // KeyNotFoundException => StatusCodes.Status404NotFound,
        _ => StatusCodes.Status500InternalServerError
    }
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();


app.MapGet("/example-feed", () =>
{
    var xml = """
        <?xml version="1.0" encoding="utf-8"?>
        <rss xmlns:a10="http://www.w3.org/2005/Atom" version="2.0">
          <channel>
            <title>My Blog Feed</title>
            <link>http://someuri/</link>
            <description>Basic example to produce a RSS feed</description>
            <managingEditor>info@improveandrepeat.com</managingEditor>
            <lastBuildDate>Sat, 3 May 2025 16:01:46 +0100</lastBuildDate>
            <category>.Net</category>

            <item>
              <guid isPermaLink="false">ItemThreeID</guid>
              <link>http://localhost/Content/three</link>
              <author>C.C@....</author>
              <category>.Net</category>
              <category>Practice</category>
              <category>Work</category>
              <title>#3 - With a Category</title>
              <description>This is the content for item three</description>
              <pubDate>Sat, 3 May 2025 15:01:46 +0100</pubDate>
              <a10:updated>2025-05-03T16:01:46+01:00</a10:updated>
            </item>
          </channel>
        </rss>
        """;

    return Results.Text(xml, "application/rss+xml", Encoding.UTF8);
});

app.MapFeedEndpoints();

app.Run();