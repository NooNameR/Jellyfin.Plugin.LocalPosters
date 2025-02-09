using System.Collections.ObjectModel;
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
public class SeasonImageProvider : ILocalImageProvider, IHasOrder
{
    private readonly PluginConfiguration _configuration;
    private readonly ILogger<SeasonImageProvider> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly BorderReplacer _borderReplacer;

    /// <summary>
    ///
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="fileSystem"></param>
    public SeasonImageProvider(ILogger<SeasonImageProvider> logger,
        IFileSystem fileSystem)
    {
        _configuration = LocalPostersPlugin.Instance?.Configuration ?? new PluginConfiguration();
        _logger = logger;
        _fileSystem = fileSystem;
        _borderReplacer = new BorderReplacer();
    }

    /// <inheritdoc />
    public bool Supports(BaseItem item)
    {
        return item is Season;
    }

    /// <inheritdoc />
    public string Name => LocalPostersPlugin.ProviderName;

    /// <inheritdoc />
    public IEnumerable<LocalImageInfo> GetImages(BaseItem item, IDirectoryService directoryService)
    {
        if (item is not Season season) return [];

        _logger.LogDebug("Trying to match assets {SeriesName}, Season: {Season}", season.SeriesName, season.IndexNumber);
        var sanitizedSeriesName = season.SeriesName.Replace(":", "", StringComparison.OrdinalIgnoreCase);
        var fullName = $"{sanitizedSeriesName} ({season.ProductionYear}) - Season {season.IndexNumber}.jpg";
        var destinationFile = _fileSystem.GetFileInfo(Path.Combine(_configuration.AssetsPath(_fileSystem).FullName, fullName));
        var seasonRegex = new Regex($@"^{sanitizedSeriesName} \({season.ProductionYear}\) - Season {season.IndexNumber}(\.[a-z]+)?$");
        var seriesRegex = new Regex($@"^{sanitizedSeriesName} \({season.Series.ProductionYear}\) - Season {season.IndexNumber}(\.[a-z]+)?$");

        for (var i = _configuration.Folders.Count - 1; i >= 0; i--)
        {
            foreach (var file in _fileSystem.GetFiles(_configuration.Folders[i]))
            {
                var seriesMatch = seriesRegex.Match(file.Name);
                var seasonMatch = seasonRegex.Match(file.Name);

                _logger.LogDebug("Series match: {FileName}, success: {Success}", file.Name, seriesMatch.Success);
                _logger.LogDebug("Season match: {FileName}, success: {Success}", file.Name, seasonMatch.Success);

                if (!seriesMatch.Success && !seasonMatch.Success)
                    continue;

                _logger.LogDebug("Matched file: {FullName}", file.FullName);

                _borderReplacer.RemoveBorder(file.FullName, destinationFile.FullName);
                return [new LocalImageInfo { FileInfo = destinationFile, Type = ImageType.Primary }];
            }
        }

        return [];
    }

    /// <inheritdoc />
    public int Order => 1;
}
