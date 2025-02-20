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

            var count = (double)await context.Set<PosterRecord>().AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false);
            const int BatchSize = 1000;

            var dbSet = context.Set<PosterRecord>();
            do
            {
                var items = await dbSet.OrderBy(x => x.Id)
                    .Take(BatchSize)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                dbSet.RemoveRange(items);
                count -= await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                progress.Report(100 - (count / BatchSize));
            } while (count > 0);

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
