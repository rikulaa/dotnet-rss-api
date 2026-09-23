namespace Api.Domain.Feed;

using System.Xml;
using System.ServiceModel.Syndication;
using Microsoft.EntityFrameworkCore;


public class FetchFeedJob(
        AppContext appContext,
        ILogger<FetchFeedJob> logger
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
            var reader = XmlReader.Create(feed.Url);
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

            var existingGuids = await appContext.Items
                .Where(item => guids.Contains(item.Guid))
                .Select(item => item.Guid)
                .ToHashSetAsync();

            var newItems = items.Where(item => !existingGuids.Contains(item.Guid));

            appContext.AddRange(newItems);
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