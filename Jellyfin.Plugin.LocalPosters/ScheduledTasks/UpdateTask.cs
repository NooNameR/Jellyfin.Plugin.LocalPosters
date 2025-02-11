using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Tasks;
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

    /// <summary>
    ///
    /// </summary>
    /// <param name="libraryManager"></param>
    /// <param name="logger"></param>
    /// <param name="providerManager"></param>
    public UpdateTask(ILibraryManager libraryManager, ILogger<UpdateTask> logger, IProviderManager providerManager)
    {
        _libraryManager = libraryManager;
        _logger = logger;
        _providerManager = providerManager;
    }

    /// <inheritdoc />
    public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return [];
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
