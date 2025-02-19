using Jellyfin.Plugin.LocalPosters.Entities;
using Jellyfin.Plugin.LocalPosters.Matchers;
using Jellyfin.Plugin.LocalPosters.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.LocalPosters.ScheduledTasks;

/// <summary>
///
/// </summary>
public class UpdateTask(
    ILibraryManager libraryManager,
    ILogger<UpdateTask> logger,
    IProviderManager providerManager,
    IDirectoryService directoryService,
    IServiceScopeFactory serviceScopeFactory,
    IMatcherFactory matcherFactory,
    [FromKeyedServices(Constants.ScheduledTaskLockKey)]
    SemaphoreSlim executionLock) : IScheduledTask
{
    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        progress.Report(0);

        await executionLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var queryable = scope.ServiceProvider.GetRequiredService<IQueryable<PosterRecord>>();

            var imageRefreshOptions = new ImageRefreshOptions(directoryService)
            {
                ImageRefreshMode = MetadataRefreshMode.FullRefresh, ReplaceImages = [ImageType.Primary]
            };

            var dict = new Dictionary<Guid, BaseItem>();
            var ids = new HashSet<Guid>(await queryable.Select(x => x.Id).ToListAsync(cancellationToken).ConfigureAwait(false));

            var currentProgress = 0d;
            foreach (var item in libraryManager.GetItemList(new InternalItemsQuery
                     {
                         IncludeItemTypes = [..matcherFactory.SupportedItemKinds], ExcludeItemIds = [..ids]
                     }))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (providerManager.HasImageProviderEnabled(item, imageRefreshOptions))
                    dict.Add(item.Id, item);
            }

            currentProgress += 10;
            progress.Report(currentProgress);

            var searcher = scope.ServiceProvider.GetRequiredService<IImageSearcher>();

            var metadataRefreshOptions =
                new MetadataRefreshOptions(directoryService)
                {
                    IsAutomated = false,
                    ImageRefreshMode = imageRefreshOptions.ImageRefreshMode,
                    ReplaceImages = imageRefreshOptions.ReplaceImages
                };

            var increaseInProgress = (100 - currentProgress) / dict.Count;

            logger.LogInformation("Found {Items} items to refresh", dict.Count);

            foreach (var (_, item) in dict)
            {
                var result = searcher.Search(item, cancellationToken);
                if (result.Exists)
                    await item.RefreshMetadata(metadataRefreshOptions, cancellationToken).ConfigureAwait(false);

                currentProgress += increaseInProgress;
                progress.Report(currentProgress);
            }

            progress.Report(100d);
        }
        finally
        {
            executionLock.Release();
        }
    }

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return [new TaskTriggerInfo { Type = TaskTriggerInfo.TriggerDaily, TimeOfDayTicks = TimeSpan.FromHours(0).Ticks }];
    }

    /// <inheritdoc />
    public string Name => "Match and Update local posters";

    /// <inheritdoc />
    public string Key => "MatchAndUpdateLocalPosters";

    /// <inheritdoc />
    public string Description => "Update posters using local library";

    /// <inheritdoc />
    public string Category => "Local Posters";
}
