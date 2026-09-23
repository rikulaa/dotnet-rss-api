namespace Api.Domain.Feed;

using System.Xml;
using System.ServiceModel.Syndication;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Net;

public class FetchFeedJob(
        AppContext appContext,
        ILogger<FetchFeedJob> logger,
        HttpClient httpClient
)

{
    public async Task<bool> Execute(int feedId, CancellationToken cancellationToken)
    {
        try
        {
            // do the job
            logger.LogInformation($"Process feed {feedId}");

            var feed = await appContext.Feeds.FindAsync(feedId);
            if (feed is null)
            {
                logger.LogError($"Feed {feedId} could not be found!");
                return false;
            }

            logger.LogInformation($"Fetch from url: {feed.Url}");

            var request = new HttpRequestMessage(HttpMethod.Get, feed.Url);
            // Add a correct user agent
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("RssReader", "0.1"));

            // Add caching headers
            if (feed.ETag is not null)
            {
                // etag -> if none match
                request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(feed.ETag));
            }

            var lastModified = feed.LastModified ?? feed.LastFetchedAt;
            if (lastModified is not null)
            {
                request.Headers.IfModifiedSince = lastModified;
            }

            logger.LogInformation($"Request headers: {request.Headers}");

            // last-modified -> if modififed since
            var response = await httpClient.SendAsync(request, cancellationToken);
            logger.LogInformation($"Response status: {response.StatusCode} {feed.Url}");

            // No new content, skip updates
            if (response.StatusCode == HttpStatusCode.NotFound) {
                logger.LogInformation($"No need content from: {feed.Url}");
                feed.LastFetchedAt = DateTimeOffset.UtcNow;
                await appContext.SaveChangesAsync();
                return true;
            }

            feed.ETag = response.Headers.ETag?.ToString();
            feed.LastModified = response.Content.Headers.LastModified;
            feed.LastFetchedAt = DateTimeOffset.UtcNow;

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStreamAsync();

                var reader = XmlReader.Create(content);
                var feedContent = SyndicationFeed.Load(reader);


                var items = feedContent.Items
                    .Select(item =>
                            new Item
                            {
                                Guid = item.Id,
                                Feed = feed,
                                Title = item.Title?.Text,
                                Description = item.Summary?.Text,
                                Author = string.Join(",", values: item.Authors?.Select(author => author.Name) ?? []),
                                Content = GetContent(item.Content),
                                PublishedAt = item.PublishDate,
                            }
                           );

                var guids = items.Select(item => item.Guid);

                // Existing items inside the current feed
                var existingGuids = await appContext.Entry(feed)
                    .Collection(feed => feed.Items)
                    .Query()
                    .Where(item => guids.Contains(item.Guid))
                    .Select(item => item.Guid)
                    .ToHashSetAsync();

                var newItems = items.Where(item => !existingGuids.Contains(item.Guid));

                appContext.AddRange(newItems);
            }

            await appContext.SaveChangesAsync();
            return true;

        }
        catch (Exception error)
        {
            logger.LogError(error, "Unable to fetch feed content");
            return false;
        }
    }


    protected string? GetContent(SyndicationContent syndicationContent)
    {
        if (syndicationContent is TextSyndicationContent textContent)
        {
            return textContent.Text;
        }

        if (syndicationContent is XmlSyndicationContent xml)
        {
            return xml.GetReaderAtContent().ReadInnerXml();
        }

        logger.LogDebug($"Unsupported content '{syndicationContent?.GetType()}' for item content!");
        return null;
    }
}