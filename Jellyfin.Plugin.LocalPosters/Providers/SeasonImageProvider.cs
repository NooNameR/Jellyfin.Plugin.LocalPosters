using System.Text.RegularExpressions;
using Jellyfin.Plugin.LocalPosters.Configuration;
using Jellyfin.Plugin.LocalPosters.Utils;
using MediaBrowser.Controller.Entities;
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
public class SeasonImageProvider : IDynamicImageProvider, IHasOrder
{
    private readonly PluginConfiguration _configuration;
    private readonly ILogger<SeasonImageProvider> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IBorderReplacer _borderReplacer;

    /// <summary>
    ///
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="fileSystem"></param>
    /// <param name="borderReplacer"></param>
    public SeasonImageProvider(ILogger<SeasonImageProvider> logger,
        IFileSystem fileSystem, IBorderReplacer borderReplacer)
    {
        _configuration = LocalPostersPlugin.Instance?.Configuration ?? new PluginConfiguration();
        _logger = logger;
        _fileSystem = fileSystem;
        _borderReplacer = borderReplacer;
    }

    /// <inheritdoc />
    public bool Supports(BaseItem item)
    {
        return item is Season;
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
        if (item is not Season season) return ValueCache.Empty.Value;

        _logger.LogDebug("Trying to match assets {SeriesName}, Season: {Season}", season.SeriesName, season.IndexNumber);
        var sanitizedSeriesName = season.SeriesName.Replace(":", "", StringComparison.OrdinalIgnoreCase);

        var seasonRegex = new Regex($@"^{sanitizedSeriesName} \({season.ProductionYear}\) - Season {season.IndexNumber}(\.[a-z]+)?$");
        var seriesRegex = new Regex($@"^{sanitizedSeriesName} \({season.Series.ProductionYear}\) - Season {season.IndexNumber}(\.[a-z]+)?$");

        foreach (var regex in new[] { seriesRegex, seasonRegex })
        {
            for (var i = _configuration.Folders.Count - 1; i >= 0; i--)
            {
                foreach (var file in _fileSystem.GetFiles(_configuration.Folders[i]))
                {
                    var match = regex.Match(file.Name);

                    _logger.LogDebug("Series match: {FileName}, success: {Success}", file.Name, match.Success);

                    if (!match.Success)
                        continue;

                    _logger.LogDebug("Matched file: {FullName}", file.FullName);

                    var destinationFile = _fileSystem.GetFileInfo(Path.Combine(season.ContainingFolderPath, "poster.jpg"));

                    _borderReplacer.RemoveBorder(file.FullName, destinationFile.FullName);
                    return Task.FromResult(new DynamicImageResponse { HasImage = true, Path = destinationFile.FullName, Format = ImageFormat.Jpg, Protocol = MediaProtocol.File });
                }
            }
        }

        return ValueCache.Empty.Value;
    }
}
