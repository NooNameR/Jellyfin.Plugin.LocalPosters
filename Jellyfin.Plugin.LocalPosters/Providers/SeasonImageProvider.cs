using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Jellyfin.Plugin.LocalPosters.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
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

        var seasonRegex = new Regex($@"^{sanitizedSeriesName} \({season.ProductionYear}\) - Season {season.IndexNumber}(\.[a-z]+)?$");
        var seriesRegex = new Regex($@"^{sanitizedSeriesName} \({season.Series.ProductionYear}\) - Season {season.IndexNumber}(\.[a-z]+)?$");

        var result = new List<LocalImageInfo>();
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

                result.Add(new LocalImageInfo { FileInfo = file, Type = ImageType.Primary });
                return result;
            }
        }

        return result;
    }

    /// <inheritdoc />
    public int Order => 1;
}
