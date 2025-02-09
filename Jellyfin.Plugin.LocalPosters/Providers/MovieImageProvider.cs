using System.Text.RegularExpressions;
using Jellyfin.Plugin.LocalPosters.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.LocalPosters.Providers;

/// <summary>
///
/// </summary>
public class MovieImageProvider : ILocalImageProvider, IHasOrder
{
    private readonly PluginConfiguration _configuration;
    private readonly ILogger<MovieImageProvider> _logger;
    private readonly IFileSystem _fileSystem;

    /// <summary>
    ///
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="fileSystem"></param>
    public MovieImageProvider(ILogger<MovieImageProvider> logger,
        IFileSystem fileSystem)
    {
        _configuration = LocalPostersPlugin.Instance?.Configuration ?? new PluginConfiguration();
        _logger = logger;
        _fileSystem = fileSystem;
    }

    /// <inheritdoc />
    public bool Supports(BaseItem item)
    {
        return item is Movie;
    }

    /// <inheritdoc />
    public string Name => LocalPostersPlugin.ProviderName;

    /// <inheritdoc />
    public IEnumerable<LocalImageInfo> GetImages(BaseItem item, IDirectoryService directoryService)
    {
        if (item is not Movie movie) return [];

        _logger.LogDebug("Trying to match assets {Movie}", movie.Name);
        var movieName = movie.Name.Replace(":", "", StringComparison.OrdinalIgnoreCase);

        var regex = new Regex($@"^{movieName} \({movie.ProductionYear}\)(\.[a-z]+)?$");

        var result = new List<LocalImageInfo>();

        for (var i = _configuration.Folders.Count - 1; i >= 0; i--)
        {
            foreach (var file in _fileSystem.GetFiles(_configuration.Folders[i]))
            {
                var match = regex.Match(file.Name);

                _logger.LogDebug("File match: {FileName}, success: {Success}", file.Name, match.Success);

                if (!match.Success)
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
