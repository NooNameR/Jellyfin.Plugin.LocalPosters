using Jellyfin.Plugin.LocalPosters.Configuration;
using Jellyfin.Plugin.LocalPosters.Logging;
using Jellyfin.Plugin.LocalPosters.Matchers;
using Jellyfin.Plugin.LocalPosters.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.LocalPosters.Providers;

/// <summary>
///
/// </summary>
public class LocalImageProvider : IDynamicImageProvider, IHasOrder
{
    private readonly ILogger<LocalImageProvider> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IMatcherFactory _matcherFactory;
    private readonly Func<PluginConfiguration, IBorderReplacer> _borderReplacerFactory;

    /// <summary>
    ///
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="fileSystem"></param>
    /// <param name="matcherFactory"></param>
    /// <param name="borderReplacerFactory"></param>
    public LocalImageProvider(ILogger<LocalImageProvider> logger,
        IFileSystem fileSystem, IMatcherFactory matcherFactory,
        Func<PluginConfiguration, IBorderReplacer> borderReplacerFactory)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _matcherFactory = matcherFactory;
        _borderReplacerFactory = borderReplacerFactory;
    }

    /// <inheritdoc />
    public bool Supports(BaseItem item)
    {
        return item is Movie or Series or Season;
    }

    /// <inheritdoc />
    public string Name => LocalPostersPlugin.ProviderName;

    /// <inheritdoc />
    public int Order => 1;

    /// <inheritdoc />
    public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
    {
        return [ImageType.Primary];
    }

    /// <inheritdoc />
    public Task<DynamicImageResponse> GetImage(BaseItem item, ImageType type, CancellationToken cancellationToken)
    {
        var configuration = LocalPostersPlugin.Instance?.Configuration ?? new PluginConfiguration();
        var matcher = _matcherFactory.Create(item);

        for (var i = configuration.Folders.Length - 1; i >= 0; i--)
        {
            foreach (var file in _fileSystem.GetFiles(configuration.Folders[i]))
            {
                var match = matcher.IsMatch(file.Name);

                _logger.LogMatching(file, item);

                if (!match)
                    continue;

                _logger.LogMatched(item, file);

                var borderReplacer = _borderReplacerFactory(configuration);
                return Task.FromResult(new DynamicImageResponse
                {
                    Stream = borderReplacer.Replace(file.FullName),
                    HasImage = true,
                    Format = ImageFormat.Jpg,
                    Protocol = MediaProtocol.File
                });
            }
        }

        _logger.LogMissing(item);
        return ValueCache.Empty.Value;
    }
}
