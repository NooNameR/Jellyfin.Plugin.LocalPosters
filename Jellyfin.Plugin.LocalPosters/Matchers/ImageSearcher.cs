using System.Diagnostics;
using Jellyfin.Plugin.LocalPosters.Configuration;
using Jellyfin.Plugin.LocalPosters.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <inheritdoc />
public class ImageSearcher : IImageSearcher
{
    private static readonly Lazy<FileSystemMetadata> _emptyMetadata = new(() => new FileSystemMetadata { Exists = false });

    private static readonly Lazy<EnumerationOptions> _enumerationOptions =
        new(() => new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            MatchCasing = MatchCasing.CaseInsensitive,
            AttributesToSkip = FileAttributes.Hidden | FileAttributes.System | FileAttributes.Temporary
        });

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
    public HashSet<ImageType> SupportedImages(BaseItem item)
    {
        return _matcherFactory.SupportedImageTypes(item);
    }

    /// <inheritdoc />
    public FileSystemMetadata Search(ImageType imageType, BaseItem item, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var matcher = _matcherFactory.Create(imageType, item);

        for (var i = 0; i < _configuration.Folders.Length; i++)
        {
            if (string.IsNullOrEmpty(_configuration.Folders[i].LocalPath))
                continue;

            var folder = _fileSystem.GetDirectoryInfo(_configuration.Folders[i].LocalPath);

            if (!folder.Exists || !folder.IsDirectory)
                continue;

            // TODO: this is a workaround while waiting for: https://github.com/jellyfin/jellyfin/pull/13691
            foreach (var searchPattern in matcher.SearchPatterns)
            {
                foreach (var file in new DirectoryInfo(folder.FullName).EnumerateFiles(searchPattern, _enumerationOptions.Value))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    _logger.LogMatching(file, imageType, item);

                    var match = matcher.IsMatch(file.Name);

                    if (!match)
                        continue;

                    _logger.LogMatched(imageType, item, file, sw.Elapsed);

                    return _fileSystem.GetFileInfo(file.FullName);
                }
            }
        }

        _logger.LogMissing(imageType, item, sw.Elapsed);

        return _emptyMetadata.Value;
    }
}
