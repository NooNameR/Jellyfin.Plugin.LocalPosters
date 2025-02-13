using Jellyfin.Plugin.LocalPosters.Entities;
using MediaBrowser.Model.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.LocalPosters.ScheduledTasks;

/// <summary>
///
/// </summary>
public class CleanupTask : IScheduledTask
{
    private readonly ILogger<UpdateTask> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly SemaphoreSlim _executionLock;

    /// <summary>
    ///
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="serviceScopeFactory"></param>
    /// <param name="executionLock"></param>
    public CleanupTask(ILogger<UpdateTask> logger, IServiceScopeFactory serviceScopeFactory,
        [FromKeyedServices(Constants.ScheduledTaskLockKey)] SemaphoreSlim executionLock)
    {
        _logger = logger;
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
            var context = scope.ServiceProvider.GetRequiredService<Context>();

            var count = (double)await context.Set<PosterRecord>().AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false);
            const double BatchSize = 1000;

            var dbSet = context.Set<PosterRecord>();
            do
            {
                var items = await dbSet.OrderBy(x => x.Id).ToListAsync(cancellationToken).ConfigureAwait(false);

                dbSet.RemoveRange(items);
                count -= await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                progress.Report(100 - (count / BatchSize));
            } while (count > 0);

            progress.Report(100);
        }
        finally
        {
            _executionLock.Release();
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
