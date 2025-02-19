using Jellyfin.Plugin.LocalPosters.Configuration;
using Jellyfin.Plugin.LocalPosters.GDrive;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.LocalPosters.ScheduledTasks;

/// <summary>
///
/// </summary>
public class SyncGDriveTask(
    ILogger<UpdateTask> logger,
    IServiceScopeFactory serviceScopeFactory,
    ITaskManager manager,
    [FromKeyedServices(Constants.ScheduledTaskLockKey)]
    SemaphoreSlim executionLock) : IScheduledTask
{
    private readonly object _sync = new();

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        progress.Report(0);

        await executionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var syncClients = scope.ServiceProvider.GetRequiredService<IEnumerable<ISyncClient>>().ToArray();
            var configuration = scope.ServiceProvider.GetRequiredService<PluginConfiguration>();
            var totalClients = syncClients.Length;

            logger.LogInformation("Syncing GDrive {FoldersCount} folders using {NumThreads} threads", totalClients,
                configuration.ConcurrentDownloadLimit);

            var tasks = new List<Task>(totalClients);
            var trackers = new double[totalClients];
            var totalSum = 0.0;
            long totalItems = 0;
            for (var index = 0; index < totalClients; index += 1)
            {
                var i = index;
                var itemProgress = new Progress<double>(d =>
                {
                    lock (_sync)
                    {
                        trackers[i] = Math.Max(d, trackers[i]);
                        totalSum = Math.Max(totalSum, trackers.Sum());
                    }

                    progress.Report((totalSum / 100) * (90d / totalClients));
                });
                var syncClient = syncClients[index];

                tasks.Add(Task.Run(async () =>
                {
                    var itemsCount = await syncClient.SyncAsync(itemProgress, cancellationToken).ConfigureAwait(false);
                    Interlocked.Add(ref totalItems, itemsCount);
                }, cancellationToken));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            progress.Report(99);

            if (totalItems > 0)
            {
                logger.LogInformation("{Items} new items were downloaded, scheduling UpdateTask", totalItems);
                manager.QueueScheduledTask<UpdateTask>();
            }

            progress.Report(100);
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
    public string Name => "Sync Posters with GDrive";

    /// <inheritdoc />
    public string Key => "SyncLocalPostersFromGDrive";

    /// <inheritdoc />
    public string Description => "Sync Local Posters with GDrive";

    /// <inheritdoc />
    public string Category => "Local Posters";
}
