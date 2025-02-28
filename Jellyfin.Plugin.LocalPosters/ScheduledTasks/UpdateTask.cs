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
    const int BatchSize = 5000;

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        progress.Report(0);

        await executionLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<Context>();
            var dbSet = context.Set<PosterRecord>();

            var imageRefreshOptions = new ImageRefreshOptions(directoryService)
            {
                ImageRefreshMode = MetadataRefreshMode.FullRefresh, ReplaceImages = [ImageType.Primary]
            };

            var ids = new HashSet<Guid>(await dbSet.AsNoTracking().Select(x => x.Id).ToListAsync(cancellationToken).ConfigureAwait(false));
            var records = libraryManager.GetCount(new InternalItemsQuery { IncludeItemTypes = [..matcherFactory.SupportedItemKinds] });
            var searcher = scope.ServiceProvider.GetRequiredService<IImageSearcher>();

            var metadataRefreshOptions =
                new MetadataRefreshOptions(directoryService)
                {
                    IsAutomated = false,
                    ImageRefreshMode = imageRefreshOptions.ImageRefreshMode,
                    ReplaceImages = imageRefreshOptions.ReplaceImages
                };

            var currentProgress = 0d;
            var increaseInProgress = (95 - currentProgress) / records;

            for (var startIndex = 0; startIndex < records; startIndex += BatchSize)
            {
                foreach (var item in libraryManager.GetItemList(new InternalItemsQuery
                         {
                             IncludeItemTypes = [..matcherFactory.SupportedItemKinds],
                             StartIndex = startIndex,
                             Limit = BatchSize,
                             SkipDeserialization = true
                         }))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!ids.Contains(item.Id) && providerManager.HasImageProviderEnabled(item, imageRefreshOptions))
                    {
                        var result = searcher.Search(item, cancellationToken);
                        if (result.Exists)
                            await item.RefreshMetadata(metadataRefreshOptions, cancellationToken).ConfigureAwait(false);
                    }

                    currentProgress += increaseInProgress;
                    progress.Report(currentProgress);

                    ids.Remove(item.Id);
                }
            }

            if (ids.Count > 0)
            {
                var itemsToRemove = await dbSet.Where(x => ids.Contains(x.Id))
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);
                dbSet.RemoveRange(itemsToRemove);
                var removed = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                logger.LogInformation("{ItemsCount} items were removed from db, as nonexistent in library.", removed);
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
