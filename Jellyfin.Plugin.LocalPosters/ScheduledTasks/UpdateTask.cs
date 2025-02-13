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
public class UpdateTask : IScheduledTask
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<UpdateTask> _logger;
    private readonly IProviderManager _providerManager;
    private readonly IDirectoryService _directoryService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly SemaphoreSlim _executionLock;

    /// <summary>
    ///
    /// </summary>
    /// <param name="libraryManager"></param>
    /// <param name="logger"></param>
    /// <param name="providerManager"></param>
    /// <param name="directoryService"></param>
    /// <param name="serviceScopeFactory"></param>
    /// <param name="executionLock"></param>
    public UpdateTask(ILibraryManager libraryManager, ILogger<UpdateTask> logger, IProviderManager providerManager,
        IDirectoryService directoryService, IServiceScopeFactory serviceScopeFactory,
        [FromKeyedServices(Constants.ScheduledTaskLockKey)]
        SemaphoreSlim executionLock)
    {
        _libraryManager = libraryManager;
        _logger = logger;
        _providerManager = providerManager;
        _directoryService = directoryService;
        _serviceScopeFactory = serviceScopeFactory;
        _executionLock = executionLock;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        progress.Report(0);

        await _executionLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
#pragma warning disable CA2007
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
#pragma warning restore CA2007
            var queryable = scope.ServiceProvider.GetRequiredService<IQueryable<PosterRecord>>();

            var imageRefreshOptions = new ImageRefreshOptions(_directoryService)
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
            progress.Report(15);

            var searcher = scope.ServiceProvider.GetRequiredService<IImageSearcher>();

            var metadataRefreshOptions =
                new MetadataRefreshOptions(_directoryService)
                {
                    IsAutomated = false,
                    ImageRefreshMode = imageRefreshOptions.ImageRefreshMode,
                    ReplaceImages = imageRefreshOptions.ReplaceImages
                };

            var currentProgress = 15d;
            var increaseInProgress = (100 - currentProgress) / dict.Count;

            _logger.LogInformation("Found {Items} items to refresh", dict.Count);

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

                foreach (var item in _libraryManager.GetItemList(new InternalItemsQuery { IncludeItemTypes = [kind] }))
                {
                    var allImageProviders = _providerManager.GetImageProviders(item, imageRefreshOptions);
                    if (allImageProviders.All(x => x.Name != LocalPostersPlugin.ProviderName) || ids.Contains(item.Id))
                        continue;

                    dict.Add(item.Id, item);
                }
            }
        }
        finally
        {
            _executionLock.Release();
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
