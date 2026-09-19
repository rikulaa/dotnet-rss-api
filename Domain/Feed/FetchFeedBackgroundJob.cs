using System.ServiceModel.Syndication;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using System.Xml;

namespace Api.Domain.Feed;

public class FetchFeedBackgroundJob(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<FetchFeedBackgroundJob> logger,
        Channel<int> feedQueue
        ) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {

        logger.LogInformation("Started feed queue watcher");
        while (!cancellationToken.IsCancellationRequested)
        {
            await foreach (var feedId in feedQueue.Reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    // do the job
                    logger.LogInformation($"Process feed {feedId}");

                    await using AsyncServiceScope scope = serviceScopeFactory.CreateAsyncScope();

                    AppContext appContext =
                        scope.ServiceProvider.GetRequiredService<AppContext>();

                    var feed = await appContext.Feeds.FindAsync(feedId);
                    if (feed is null)
                    {
                        logger.LogError($"Feed {feedId} could not be found!");
                        continue;
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
                                Title = item.Title.Text,
                                Description = item.Summary.Text,
                                Author = string.Join(",", item.Authors.Select(author => author.Name)),
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

                } catch (Exception error)
                {
                    logger.LogError(error, "Unable to fetch feed content");
                }
            }

        }
    }

    protected string? GetContent(SyndicationContent syndicationContent)
    {
        if (syndicationContent is TextSyndicationContent textContent) {
            return textContent.Text;
        }

        if (syndicationContent is XmlSyndicationContent xml) {
            return xml.GetReaderAtContent().ReadInnerXml();
        }

        logger.LogDebug($"Unsupported content '{syndicationContent?.GetType()}' for item content!");
        return null;
    }

}