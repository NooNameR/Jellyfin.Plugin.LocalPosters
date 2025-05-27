using Google.Apis.Util.Store;
using Jellyfin.Plugin.LocalPosters.Configuration;
using Jellyfin.Plugin.LocalPosters.Entities;
using Jellyfin.Plugin.LocalPosters.GDrive;
using Jellyfin.Plugin.LocalPosters.Matchers;
using Jellyfin.Plugin.LocalPosters.Providers;
using Jellyfin.Plugin.LocalPosters.ScheduledTasks;
using Jellyfin.Plugin.LocalPosters.Utils;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.LocalPosters;

/// <summary>
///
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddDbContext<Context>((p, builder) =>
        {
            var plugin = p.GetRequiredService<LocalPostersPlugin>();
            builder.UseSqlite($"Data Source={plugin.DbPath}");
        });
        serviceCollection.AddSingleton<IMatcherFactory, MatcherFactory>();
        serviceCollection.AddSingleton<LocalPostersPlugin>(_ =>
        {
            ArgumentNullException.ThrowIfNull(LocalPostersPlugin.Instance);
            return LocalPostersPlugin.Instance;
        });
        serviceCollection.AddScoped(GetDbSet<PosterRecord>);
        serviceCollection.AddScoped(GetQueryable<PosterRecord>);
        serviceCollection.AddScoped<ImageSearcher>();
        serviceCollection.AddSingleton<ImageSizeProvider>();
        serviceCollection.AddScoped<IImageSearcher>(provider =>
            new CachedImageSearcher(provider.GetRequiredService<ImageSearcher>(), provider.GetRequiredService<IMemoryCache>(),
                provider.GetRequiredService<IFileSystem>(), provider.GetRequiredService<LocalPostersPlugin>()));
        serviceCollection.AddScoped<PluginConfiguration>(p => p.GetRequiredService<LocalPostersPlugin>().Configuration);
        serviceCollection.AddScoped(CreateBorderReplacer);
        serviceCollection.AddSingleton<IDataStore>(provider =>
            new FileDataStore(provider.GetRequiredService<LocalPostersPlugin>().GDriveTokenFolder, true));
        serviceCollection.AddScoped(CreateSyncClients);
        serviceCollection.AddScoped<GDriveServiceProvider>();
        serviceCollection.AddKeyedScoped(GDriveSyncClient.DownloadLimiterKey, GetGDriveDownloadLimiter);
        serviceCollection.AddKeyedSingleton(Constants.ScheduledTaskLockKey, new SemaphoreSlim(1));
        serviceCollection.AddSingleton<LocalImageProvider>();
    }

    static SemaphoreSlim GetGDriveDownloadLimiter(IServiceProvider serviceProvider, object _)
    {
        var configuration = serviceProvider.GetRequiredService<PluginConfiguration>();
        return new SemaphoreSlim(configuration.ConcurrentDownloadLimit);
    }

    static IEnumerable<ISyncClient> CreateSyncClients(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<PluginConfiguration>();

        if (!configuration.Folders.Any(x => x.IsRemote))
            yield break;

        var driveServiceProvider = serviceProvider.GetRequiredService<GDriveServiceProvider>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        var downloadLimiter = serviceProvider.GetRequiredKeyedService<SemaphoreSlim>(GDriveSyncClient.DownloadLimiterKey);

        foreach (var folder in configuration.Folders)
        {
            if (!folder.IsRemote)
                continue;

            if (string.IsNullOrEmpty(folder.LocalPath))
                continue;

            yield return new GDriveSyncClient(loggerFactory.CreateLogger($"{nameof(GDriveSyncClient)}[{folder.RemoteId}]"),
                driveServiceProvider, folder.RemoteId,
                folder.LocalPath,
                fileSystem, downloadLimiter);
        }
    }

    static DbSet<T> GetDbSet<T>(IServiceProvider provider) where T : class
    {
        return provider.GetRequiredService<Context>().Set<T>();
    }

    static IQueryable<T> GetQueryable<T>(IServiceProvider provider) where T : class
    {
        return provider.GetRequiredService<Context>().Set<T>().AsNoTracking();
    }

    static IImageProcessor CreateBorderReplacer(IServiceProvider provider)
    {
        var pluginConfiguration = provider.GetRequiredService<PluginConfiguration>();
        var resizer = pluginConfiguration.ResizeImage
            ? new SkiaImageResizer(provider.GetRequiredService<ImageSizeProvider>())
            : NoopImageResizer.Instance.Value;

        if (!pluginConfiguration.EnableBorderReplacer)
            return resizer;

        if (pluginConfiguration.RemoveBorder || !pluginConfiguration.SkColor.HasValue)
            return new SkiaSharpBorderRemover(resizer);

        if (pluginConfiguration.SkColor.HasValue)
            return new SkiaSharpImageProcessor(pluginConfiguration.SkColor.Value, resizer);

        return resizer;
    }
}
