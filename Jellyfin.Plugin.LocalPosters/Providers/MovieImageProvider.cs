using System.Text.RegularExpressions;
using Jellyfin.Plugin.LocalPosters.Configuration;
using Jellyfin.Plugin.LocalPosters.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
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
public class MovieImageProvider : IDynamicImageProvider, IHasOrder
{
    private readonly PluginConfiguration _configuration;
    private readonly ILogger<MovieImageProvider> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IBorderReplacer _borderReplacer;

    /// <summary>
    ///
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="fileSystem"></param>
    /// <param name="borderReplacer"></param>
    public MovieImageProvider(ILogger<MovieImageProvider> logger,
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
        return item is Movie;
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
        if (item is not Movie movie) return ValueCache.Empty.Value;

        _logger.LogDebug("Trying to match assets {Movie}", movie.Name);
        var movieName = movie.Name.Replace(":", "", StringComparison.OrdinalIgnoreCase);

        var regex = new Regex($@"^{movieName} \({movie.ProductionYear}\)(\.[a-z]+)?$");

        for (var i = _configuration.Folders.Count - 1; i >= 0; i--)
        {
            foreach (var file in _fileSystem.GetFiles(_configuration.Folders[i]))
            {
                var match = regex.Match(file.Name);

                _logger.LogDebug("File match: {FileName}, success: {Success}", file.Name, match.Success);

                if (!match.Success)
                    continue;

                _logger.LogDebug("Matched file: {FullName}", file.FullName);
                var destinationFile = _fileSystem.GetFileInfo(Path.Combine(movie.ContainingFolderPath, "poster.jpg"));

                _borderReplacer.RemoveBorder(file.FullName, destinationFile.FullName);
                return Task.FromResult(new DynamicImageResponse { HasImage = true, Path = destinationFile.FullName, Format = ImageFormat.Jpg, Protocol = MediaProtocol.File });
            }
        }

        return ValueCache.Empty.Value;
    }
}
