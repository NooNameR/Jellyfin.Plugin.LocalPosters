using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jellyfin.Plugin.LocalPosters;

/// <summary>
///
/// </summary>
public class ContextMigrationHostedService : IHostedService
{
    private readonly IServiceScopeFactory _factory;

    /// <summary>
    ///
    /// </summary>
    /// <param name="factory"></param>
    public ContextMigrationHostedService(IServiceScopeFactory factory)
    {
        _factory = factory;
    }

    /// <inheritdoc />
    [SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var serviceScope = _factory.CreateAsyncScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<Context>();

        await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
