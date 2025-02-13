using Jellyfin.Plugin.LocalPosters.Configuration;
using Jellyfin.Plugin.LocalPosters.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <summary>
///
/// </summary>
public interface IImageSearcher
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    bool IsSupported(BaseItem item);

    /// <summary>
    ///
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    FileSystemMetadata Search(BaseItem item, CancellationToken cancellationToken);
}

/// <inheritdoc />
public class ImageSearcher : IImageSearcher
{
    private static readonly Lazy<FileSystemMetadata> _emptyMetadata = new(() => new FileSystemMetadata { Exists = false });
    private readonly PluginConfiguration _configuration;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<ImageSearcher> _logger;
    private readonly IMatcherFactory _matcherFactory;

    /// <summary>
    ///
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="matcherFactory"></param>
    /// <param name="fileSystem"></param>
    /// <param name="configuration"></param>
    public ImageSearcher(ILogger<ImageSearcher> logger, IMatcherFactory matcherFactory, IFileSystem fileSystem,
        PluginConfiguration configuration)
    {
        _logger = logger;
        _matcherFactory = matcherFactory;
        _fileSystem = fileSystem;
        _configuration = configuration;
    }

    /// <inheritdoc />
    public bool IsSupported(BaseItem item)
    {
        return _matcherFactory.IsSupported(item);
    }

    /// <inheritdoc />
    public FileSystemMetadata Search(BaseItem item, CancellationToken cancellationToken)
    {
        var matcher = _matcherFactory.Create(item);
        for (var i = _configuration.Folders.Length - 1; i >= 0; i--)
        {
            foreach (var file in _fileSystem.GetFiles(_configuration.Folders[i]))
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogMatching(file, item);

                var match = matcher.IsMatch(file.Name);

                if (!match)
                    continue;

                _logger.LogMatched(item, file);

                return file;
            }
        }

        _logger.LogMissing(item);

        return _emptyMetadata.Value;
    }
}
