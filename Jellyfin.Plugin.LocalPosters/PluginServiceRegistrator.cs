using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Jellyfin.Plugin.LocalPosters.Configuration;
using Jellyfin.Plugin.LocalPosters.Entities;
using Jellyfin.Plugin.LocalPosters.GDrive;
using Jellyfin.Plugin.LocalPosters.Matchers;
using Jellyfin.Plugin.LocalPosters.ScheduledTasks;
using Jellyfin.Plugin.LocalPosters.Utils;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.IO;
using Microsoft.EntityFrameworkCore;
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
        serviceCollection.AddScoped<LocalPostersPlugin>(_ =>
        {
            ArgumentNullException.ThrowIfNull(LocalPostersPlugin.Instance);
            return LocalPostersPlugin.Instance;
        });
        serviceCollection.AddScoped(GetDbSet<PosterRecord>);
        serviceCollection.AddScoped(GetQueryable<PosterRecord>);
        serviceCollection.AddScoped<IImageSearcher, ImageSearcher>();
        serviceCollection.AddScoped<PluginConfiguration>(p => p.GetRequiredService<LocalPostersPlugin>().Configuration);
        serviceCollection.AddScoped(CreateBorderReplacer);
        serviceCollection.AddScoped(CreateSyncClients);
        serviceCollection.AddScoped(CreateDriveService);
        serviceCollection.AddKeyedScoped(GDriveSyncClient.DownloadLimiterKey, GetGDriveDownloadLimiter);
        serviceCollection.AddKeyedSingleton(Constants.ScheduledTaskLockKey, new SemaphoreSlim(1));
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

        var driveService = serviceProvider.GetRequiredService<DriveService>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        var downloadLimiter = serviceProvider.GetRequiredKeyedService<SemaphoreSlim>(GDriveSyncClient.DownloadLimiterKey);

        foreach (var folder in configuration.Folders)
        {
            if (!folder.IsRemote)
                continue;

            yield return new GDriveSyncClient(loggerFactory.CreateLogger($"{nameof(GDriveSyncClient)}[{folder.RemoteId}]"),
                driveService, folder.RemoteId,
                folder.LocalPath,
                fileSystem, downloadLimiter);
        }
    }


    static DriveService CreateDriveService(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<PluginConfiguration>();
        var fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

        GoogleCredential? credential = null;
        if (!string.IsNullOrEmpty(configuration.GoogleSaCredentialFile))
        {
            var saCredentialFile = fileSystem.GetFileInfo(configuration.GoogleSaCredentialFile);
            if (saCredentialFile.Exists)
                credential = GoogleCredential.FromFile(saCredentialFile.FullName)
                    .CreateScoped(DriveService.Scope.Drive);
        }

        ArgumentNullException.ThrowIfNull(credential, nameof(credential));

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = GDriveSyncClient.ApplicationName
        });
    }

    static DbSet<T> GetDbSet<T>(IServiceProvider provider) where T : class
    {
        return provider.GetRequiredService<Context>().Set<T>();
    }

    static IQueryable<T> GetQueryable<T>(IServiceProvider provider) where T : class
    {
        return provider.GetRequiredService<Context>().Set<T>().AsNoTracking();
    }

    static IBorderReplacer CreateBorderReplacer(IServiceProvider provider)
    {
        var pluginConfiguration = provider.GetRequiredService<PluginConfiguration>();
        if (!pluginConfiguration.EnableBorderReplacer)
            return new SkiaDefaultBorderReplacer();

        if (pluginConfiguration.RemoveBorder || !pluginConfiguration.SkColor.HasValue)
            return new SkiaSharpBorderRemover();

        if (pluginConfiguration.SkColor.HasValue)
            return new SkiaSharpBorderReplacer(pluginConfiguration.SkColor.Value);

        return new SkiaDefaultBorderReplacer();
    }
}
