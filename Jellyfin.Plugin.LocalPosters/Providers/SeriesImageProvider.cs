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
public class SeriesImageProvider : IDynamicImageProvider, IHasOrder
{
    private readonly PluginConfiguration _configuration;
    private readonly ILogger<SeriesImageProvider> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly SkiaSharpBorderReplacer _skiaSharpBorderReplacer;

    /// <summary>
    ///
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="fileSystem"></param>
    public SeriesImageProvider(ILogger<SeriesImageProvider> logger,
        IFileSystem fileSystem)
    {
        _configuration = LocalPostersPlugin.Instance?.Configuration ?? new PluginConfiguration();
        _logger = logger;
        _fileSystem = fileSystem;
        _skiaSharpBorderReplacer = new SkiaSharpBorderReplacer();
    }

    /// <inheritdoc />
    public bool Supports(BaseItem item)
    {
        return item is Series;
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
        if (item is not Series series) return ValueCache.Empty.Value;

        _logger.LogDebug("Trying to match assets {SeriesName}", series.Name);

        foreach (var regex in Regexes())
        {
            for (var i = _configuration.Folders.Count - 1; i >= 0; i--)
            {
                foreach (var file in _fileSystem.GetFiles(_configuration.Folders[i]))
                {
                    var match = regex.Match(file.Name);

                    _logger.LogDebug("File match: {FileName}, success: {Success}", file.Name, match.Success);

                    if (!match.Success)
                        continue;

                    _logger.LogDebug("Matched file: {FullName}", file.FullName);
                    var destinationFile = _fileSystem.GetFileInfo(Path.Combine(series.ContainingFolderPath, "poster.jpg"));

                    _skiaSharpBorderReplacer.RemoveBorder(file.FullName, destinationFile.FullName);
                    return Task.FromResult(new DynamicImageResponse
                    {
                        HasImage = true, Path = destinationFile.FullName, Format = ImageFormat.Jpg, Protocol = MediaProtocol.File
                    });
                }
            }
        }

        _logger.LogInformation("Was not able to match: {Series} ({Year})", series.Name, series.ProductionYear);

        return ValueCache.Empty.Value;

        IEnumerable<Regex> Regexes()
        {
            var sanitizedName = series.Name.Replace($" ({series.ProductionYear})", "", StringComparison.OrdinalIgnoreCase);

            yield return
                new Regex($@"^{sanitizedName} \({series.ProductionYear}\)(\.[a-z]+)?$", RegexOptions.IgnoreCase);
            foreach (var replacement in new[] { "", "-" })
                yield return new Regex(
                    $@"^{sanitizedName.Replace(":", replacement, StringComparison.OrdinalIgnoreCase)} \({series.ProductionYear}\)(\.[a-z]+)?$",
                    RegexOptions.IgnoreCase);
        }
    }
}
