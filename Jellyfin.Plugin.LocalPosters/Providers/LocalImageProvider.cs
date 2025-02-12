using System.Diagnostics.CodeAnalysis;
using Jellyfin.Plugin.LocalPosters.Configuration;
using Jellyfin.Plugin.LocalPosters.Entities;
using Jellyfin.Plugin.LocalPosters.Logging;
using Jellyfin.Plugin.LocalPosters.Matchers;
using Jellyfin.Plugin.LocalPosters.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    ///
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="fileSystem"></param>
    /// <param name="matcherFactory"></param>
    /// <param name="serviceScopeFactory"></param>
    /// <param name="timeProvider"></param>
    public LocalImageProvider(ILogger<LocalImageProvider> logger,
        IFileSystem fileSystem, IMatcherFactory matcherFactory,
        IServiceScopeFactory serviceScopeFactory, TimeProvider timeProvider)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _matcherFactory = matcherFactory;
        _serviceScopeFactory = serviceScopeFactory;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public bool Supports(BaseItem item)
    {
        return _matcherFactory.IsSupported(item);
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
    [SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
    public async Task<DynamicImageResponse> GetImage(BaseItem item, ImageType type, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var serviceScope = _serviceScopeFactory.CreateAsyncScope();

        var context = serviceScope.ServiceProvider.GetRequiredService<Context>();
        var record = await context.Set<PosterRecord>().FindAsync([item.Id], cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (record != null && record.MatchedAt > _timeProvider.GetLocalNow().AddHours(-1))
        {
            var file = record.PosterFile(_fileSystem);
            if (file.Exists)
                return ProcessFile(file, serviceScope.ServiceProvider);
        }

        var configuration = serviceScope.ServiceProvider.GetRequiredService<PluginConfiguration>();
        var matcher = _matcherFactory.Create(item);

        for (var i = configuration.Folders.Length - 1; i >= 0; i--)
        {
            foreach (var file in _fileSystem.GetFiles(configuration.Folders[i]))
            {
                _logger.LogMatching(file, item);

                var match = matcher.IsMatch(file.Name);

                if (!match)
                    continue;

                var now = _timeProvider.GetLocalNow();
                if (record == null)
                {
                    record = new PosterRecord(item.Id, now, file);
                    record.SetPosterFile(file, now);
                    await context.Set<PosterRecord>().AddAsync(record, cancellationToken);
                }
                else
                {
                    record.SetPosterFile(file, now);
                    context.Set<PosterRecord>().Update(record);
                }

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogMatched(item, file);

                return ProcessFile(file, serviceScope.ServiceProvider);
            }
        }

        _logger.LogMissing(item);
        return ValueCache.Empty.Value;

        DynamicImageResponse ProcessFile(FileSystemMetadata file, IServiceProvider serviceProvider)
        {
            var borderReplacer = serviceProvider.GetRequiredService<IBorderReplacer>();

            return new DynamicImageResponse
            {
                Stream = borderReplacer.Replace(file.FullName), HasImage = true, Format = ImageFormat.Jpg, Protocol = MediaProtocol.File
            };
        }
    }
}
