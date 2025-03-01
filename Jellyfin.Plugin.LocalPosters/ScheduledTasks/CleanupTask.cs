using Jellyfin.Plugin.LocalPosters.Entities;
using MediaBrowser.Model.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.LocalPosters.ScheduledTasks;

/// <summary>
///
/// </summary>
public class CleanupTask(
    IServiceScopeFactory serviceScopeFactory,
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
            var context = scope.ServiceProvider.GetRequiredService<Context>();
            await context.Set<PosterRecord>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

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
        return [];
    }

    /// <inheritdoc />
    public string Name => "Cleanup local posters db";

    /// <inheritdoc />
    public string Key => "CleanupLocalPostersDB";

    /// <inheritdoc />
    public string Description => "Cleanup local posters database";

    /// <inheritdoc />
    public string Category => "Local Posters";
}
