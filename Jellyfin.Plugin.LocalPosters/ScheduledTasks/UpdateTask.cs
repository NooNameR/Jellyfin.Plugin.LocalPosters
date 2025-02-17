using Jellyfin.Data.Enums;
using Jellyfin.Plugin.LocalPosters.Entities;
using Jellyfin.Plugin.LocalPosters.Matchers;
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
public class UpdateTask(ILibraryManager libraryManager, ILogger<UpdateTask> logger, IProviderManager providerManager,
    IDirectoryService directoryService, IServiceScopeFactory serviceScopeFactory,
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

            TraverseItems(BaseItemKind.Series);
            progress.Report(5);
            TraverseItems(BaseItemKind.Season);
            progress.Report(10);
            TraverseItems(BaseItemKind.Movie);
            TraverseItems(BaseItemKind.BoxSet);
            progress.Report(15);

            var searcher = scope.ServiceProvider.GetRequiredService<IImageSearcher>();

            var metadataRefreshOptions =
                new MetadataRefreshOptions(directoryService)
                {
                    IsAutomated = false,
                    ImageRefreshMode = imageRefreshOptions.ImageRefreshMode,
                    ReplaceImages = imageRefreshOptions.ReplaceImages
                };

            var currentProgress = 15d;
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

            void TraverseItems(BaseItemKind kind)
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var item in libraryManager.GetItemList(new InternalItemsQuery { IncludeItemTypes = [kind] }))
                {
                    var allImageProviders = providerManager.GetImageProviders(item, imageRefreshOptions);
                    if (allImageProviders.All(x => x.Name != LocalPostersPlugin.ProviderName) || ids.Contains(item.Id))
                        continue;

                    dict.Add(item.Id, item);
                }
            }
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
