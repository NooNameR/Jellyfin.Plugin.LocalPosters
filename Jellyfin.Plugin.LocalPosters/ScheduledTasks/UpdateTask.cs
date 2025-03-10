using Jellyfin.Plugin.LocalPosters.Entities;
using Jellyfin.Plugin.LocalPosters.Matchers;
using Jellyfin.Plugin.LocalPosters.Providers;
using Jellyfin.Plugin.LocalPosters.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Drawing;
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
    LocalImageProvider localImageProvider,
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
            var imageTypes = new[] { ImageType.Primary };

            var records = await dbSet.AsNoTracking().Select(x => new { x.ItemId, x.ImageType }).GroupBy(x => x.ItemId, x => x.ImageType)
                .ToDictionaryAsync(x => x.Key, x => x.ToHashSet(), cancellationToken)
                .ConfigureAwait(false);

            var count = libraryManager.GetCount(new InternalItemsQuery
            {
                IncludeItemTypes = [..matcherFactory.SupportedItemKinds], ImageTypes = imageTypes
            });

            var currentProgress = 0d;
            var increaseInProgress = (95 - currentProgress) / count;

            for (var startIndex = 0; startIndex < count; startIndex += BatchSize)
            {
                foreach (var item in libraryManager.GetItemList(new InternalItemsQuery
                         {
                             IncludeItemTypes = [..matcherFactory.SupportedItemKinds],
                             ImageTypes = imageTypes,
                             StartIndex = startIndex,
                             Limit = BatchSize,
                             SkipDeserialization = true
                         }))
                {
                    foreach (var imageType in imageTypes)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (records.TryGetValue(item.Id, out var types) && types.Contains(imageType))
                            continue;

                        var imageRefreshOptions = new ImageRefreshOptions(directoryService)
                        {
                            ImageRefreshMode = MetadataRefreshMode.FullRefresh, ReplaceImages = imageTypes
                        };

                        if (!providerManager.HasImageProviderEnabled(item, imageRefreshOptions))
                            continue;

                        var image = await localImageProvider.GetImage(item, imageType, cancellationToken).ConfigureAwait(false);
                        if (!image.HasImage)
                            continue;

                        await providerManager.SaveImage(item, image.Stream, image.Format.GetMimeType(), imageType, null,
                            cancellationToken).ConfigureAwait(false);
                    }

                    currentProgress += increaseInProgress;
                    progress.Report(currentProgress);

                    records.Remove(item.Id);
                }
            }

            var removed = 0;
            while (records.Count > 0)
            {
                var slice = new HashSet<Guid>(records.Keys.Take(BatchSize));
                var itemsToRemove = await dbSet.Where(x => slice.Contains(x.ItemId))
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);
                dbSet.RemoveRange(itemsToRemove);
                removed += await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                foreach (var itemToRemove in slice)
                    records.Remove(itemToRemove);
            }

            if (removed > 0)
                logger.LogInformation("{ItemsCount} items were removed from db, as nonexistent inside the library.", removed);

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
