using Jellyfin.Plugin.LocalPosters.Configuration;
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
    private readonly PluginConfiguration _configuration;
    private readonly ILogger<LocalImageProvider> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IBorderReplacer _borderReplacer;
    private readonly IMatcherFactory _matcherFactory;

    /// <summary>
    ///
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="fileSystem"></param>
    /// <param name="borderReplacer"></param>
    /// <param name="matcherFactory"></param>
    public LocalImageProvider(ILogger<LocalImageProvider> logger,
        IFileSystem fileSystem, IBorderReplacer borderReplacer, IMatcherFactory matcherFactory)
    {
        _configuration = LocalPostersPlugin.Instance?.Configuration ?? new PluginConfiguration();
        _logger = logger;
        _fileSystem = fileSystem;
        _borderReplacer = borderReplacer;
        _matcherFactory = matcherFactory;
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
        _logger.LogDebug("Trying to match assets {Name}", item.Name);

        var matcher = _matcherFactory.Create(item);

        for (var i = _configuration.Folders.Length - 1; i >= 0; i--)
        {
            foreach (var file in _fileSystem.GetFiles(_configuration.Folders[i]))
            {
                var match = matcher.IsMatch(file.Name);

                _logger.LogDebug("File match: {FileName}, success: {Success}", file.Name, match);

                if (!match)
                    continue;

                _logger.LogDebug("Matched file: {FullName}", file.FullName);

                return Task.FromResult(new DynamicImageResponse
                {
                    Stream = _borderReplacer.RemoveBorder(file.FullName),
                    HasImage = true,
                    Format = ImageFormat.Jpg,
                    Protocol = MediaProtocol.File
                });
            }
        }

        _logger.LogInformation("Was not able to match: {Name} ({ProductionYear}), PremiereDate: {PremiereDate}", item.Name,
            item.ProductionYear, item.PremiereDate);

        return ValueCache.Empty.Value;
    }
}
