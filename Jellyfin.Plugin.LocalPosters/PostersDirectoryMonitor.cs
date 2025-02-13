using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Jellyfin.Plugin.LocalPosters.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Hosting;

namespace Jellyfin.Plugin.LocalPosters;

/// <summary>
///
/// </summary>
public class PostersDirectoryMonitor : IHostedService
{
    private readonly IDirectoryService _directoryService;
    private readonly ILibraryManager _libraryManager;
    private readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers = new();

    /// <summary>
    ///
    /// </summary>
    /// <param name="directoryService"></param>
    /// <param name="libraryManager"></param>
    public PostersDirectoryMonitor(IDirectoryService directoryService, ILibraryManager libraryManager)
    {
        _directoryService = directoryService;
        _libraryManager = libraryManager;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            var plugin = LocalPostersPlugin.Instance;
            while (plugin is null)
            {
                await Task.Delay(200, cancellationToken).ConfigureAwait(false);
                plugin = LocalPostersPlugin.Instance;
            }

            plugin.ConfigurationChanged += OnConfigurationChanged;

            for (var i = 0; i < plugin!.Configuration.Folders.Length; i++)
            {
                var folder = _directoryService.GetDirectory(plugin.Configuration.Folders[i]);
                if (folder is not { Exists: true })
                    continue;

                StartWatcher(folder);
            }
        }, cancellationToken);
    }

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP017:Prefer using")]
    private void StartWatcher(FileSystemMetadata folder)
    {
        var watcher = new FileSystemWatcher(folder.FullName, "*")
        {
            IncludeSubdirectories = true,
            InternalBufferSize = 65536,
            NotifyFilter =
                NotifyFilters.CreationTime |
                NotifyFilters.FileName |
                NotifyFilters.LastWrite,
        };
        watcher.Created += OnCreated;
        watcher.Changed += OnChanged;

        if (_watchers.TryAdd(folder.FullName, watcher))
        {
            watcher.EnableRaisingEvents = true;
            return;
        }

        watcher.Dispose();
    }

    private void OnConfigurationChanged(object? sender, BasePluginConfiguration e)
    {
        if (e is not PluginConfiguration configuration)
            return;

        var folders = configuration.Folders.Select(f => _directoryService.GetDirectory(f))
            .OfType<FileSystemMetadata>().ToDictionary(x => x.FullName);
        var toAdd = folders.Keys.Where(x => !_watchers.ContainsKey(x));
        var toRemove = _watchers.Keys.Where(x => !folders.ContainsKey(x));

        foreach (var key in toRemove)
        {
            if (_watchers.TryRemove(key, out var watcher))
                watcher.Dispose();
        }

        foreach (var key in toAdd) StartWatcher(folders[key]);
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var key in _watchers.Keys)
        {
            if (_watchers.TryRemove(key, out var watcher))
                watcher.Dispose();
        }

        return Task.CompletedTask;
    }
}
