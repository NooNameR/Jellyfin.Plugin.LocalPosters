using System.Threading.Channels;
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
    private readonly object _lock = new();

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

            var count = libraryManager.GetCount(new InternalItemsQuery { IncludeItemTypes = [..matcherFactory.SupportedItemKinds] });

            var ids = new HashSet<Guid>(await dbSet.AsTracking().Select(x => x.ItemId).ToListAsync(cancellationToken)
                .ConfigureAwait(false));

            var currentProgress = 0d;
            const int BatchSize = 5000;

            var channel = Channel.CreateBounded<KeyValuePair<Guid, HashSet<ImageType>>>(new BoundedChannelOptions(BatchSize)
            {
                FullMode = BoundedChannelFullMode.Wait, SingleReader = false, SingleWriter = true,
            });

            var increaseInProgress = (20 - currentProgress) / (Math.Max(1, count / (double)BatchSize));
            var items = new Dictionary<Guid, HashSet<ImageType>>();
            for (var startIndex = 0; startIndex < count; startIndex += BatchSize)
            {
                var library = libraryManager.GetItemList(new InternalItemsQuery
                {
                    IncludeItemTypes = [..matcherFactory.SupportedItemKinds],
                    StartIndex = startIndex,
                    Limit = BatchSize,
                    SkipDeserialization = true
                });

                var libraryIds = new HashSet<Guid>(library.Select(x => x.Id));

                var records = await dbSet.AsNoTracking().Where(x => libraryIds.Contains(x.ItemId))
                    .Select(x => new { x.ItemId, x.ImageType })
                    .GroupBy(x => x.ItemId, x => x.ImageType)
                    .ToDictionaryAsync(x => x.Key, x => x.ToHashSet(), cancellationToken)
                    .ConfigureAwait(false);

                foreach (var item in library)
                {
                    foreach (var imageType in matcherFactory.SupportedImageTypes(item))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (records.TryGetValue(item.Id, out var types) && types.Contains(imageType))
                            continue;

                        var imageRefreshOptions = new ImageRefreshOptions(directoryService)
                        {
                            ImageRefreshMode = MetadataRefreshMode.FullRefresh, ReplaceImages = [imageType]
                        };

                        if (!providerManager.HasImageProviderEnabled(item, imageRefreshOptions))
                            continue;

                        if (items.TryGetValue(item.Id, out var images))
                            images.Add(imageType);
                        else
                            items[item.Id] = [imageType];
                    }

                    ids.Remove(item.Id);
                }

                progress.Report(currentProgress += increaseInProgress);
            }

            var totalImages = items.Values.Sum(x => x.Count);
            increaseInProgress = (95 - currentProgress) / totalImages;

            var concurrencyLimit = Environment.ProcessorCount;
            var readerTask = StartReaders(channel.Reader);

            logger.LogInformation("Starting matching for {ItemsCount} unique items, and total: {TotalCount} using {NumThreads} threads",
                items.Count, totalImages, concurrencyLimit);

            foreach (var tuple in items)
                await channel.Writer.WriteAsync(tuple, cancellationToken).ConfigureAwait(false);

            channel.Writer.Complete();

            var removed = 0;
            while (ids.Count > 0)
            {
                var slice = new HashSet<Guid>(ids.Take(BatchSize));
                removed += await RemoveItems(slice).ConfigureAwait(false);

                foreach (var itemToRemove in slice)
                    ids.Remove(itemToRemove);
            }

            await readerTask.ConfigureAwait(false);

            if (removed > 0)
                logger.LogInformation("{ItemsCount} items were removed from db, as nonexistent inside the library.", removed);

            progress.Report(100d);
            return;

            async Task<int> RemoveItems(HashSet<Guid> slice)
            {
                var itemsToRemove = await dbSet.Where(x => slice.Contains(x.ItemId))
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);
                dbSet.RemoveRange(itemsToRemove);
                return await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            Task StartReaders(ChannelReader<KeyValuePair<Guid, HashSet<ImageType>>> reader)
            {
#pragma warning disable IDISP013
                return Task.WhenAll(Enumerable.Range(0, concurrencyLimit).Select(_ => Task.Run(ReaderTask, cancellationToken)));
#pragma warning restore IDISP013
                async Task ReaderTask()
                {
                    while (await reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        while (reader.TryRead(out var i))
                        {
                            await ProcessItem(i.Key, i.Value).ConfigureAwait(false);

                            if (!await reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
                                break;

                            continue;

                            async Task ProcessItem(Guid id, HashSet<ImageType> types)
                            {
                                cancellationToken.ThrowIfCancellationRequested();

                                var item = libraryManager.GetItemById(id);
                                if (item == null)
                                    return;

                                foreach (var imageType in types)
                                {
                                    var image = await localImageProvider.GetImage(item, imageType, cancellationToken).ConfigureAwait(false);
                                    if (image.HasImage)
                                        await providerManager.SaveImage(item, image.Stream, image.Format.GetMimeType(), imageType, null,
                                            cancellationToken).ConfigureAwait(false);

                                    lock (_lock)
                                        progress.Report(currentProgress += increaseInProgress);
                                }
                            }
                        }
                    }
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
