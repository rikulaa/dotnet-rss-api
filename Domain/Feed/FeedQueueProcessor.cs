using System.Threading.Channels;

namespace Api.Domain.Feed;

public class FeedQueueProcessor(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<FeedQueueProcessor> logger,
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
                await using AsyncServiceScope scope = serviceScopeFactory.CreateAsyncScope();

                FetchFeedJob job =
                    scope.ServiceProvider.GetRequiredService<FetchFeedJob>();

                await job.Execute(feedId, cancellationToken);
            }

        }
    }

}