using System.Diagnostics.CodeAnalysis;
using Jellyfin.Plugin.LocalPosters.Entities;
using Jellyfin.Plugin.LocalPosters.Matchers;
using Jellyfin.Plugin.LocalPosters.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.LocalPosters.Providers;

/// <summary>
///
/// </summary>
public class LocalImageProvider : IDynamicImageProvider, IHasOrder
{
    private readonly IImageSearcher _searcher;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    ///
    /// </summary>
    /// <param name="serviceScopeFactory"></param>
    /// <param name="timeProvider"></param>
    /// <param name="searcher"></param>
    public LocalImageProvider(IServiceScopeFactory serviceScopeFactory, TimeProvider timeProvider, IImageSearcher searcher)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _timeProvider = timeProvider;
        _searcher = searcher;
    }

    /// <inheritdoc />
    public bool Supports(BaseItem item)
    {
        return _searcher.IsSupported(item);
    }

    /// <inheritdoc />
    public string Name => LocalPostersPlugin.ProviderName;

    /// <inheritdoc />
    public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
    {
        return [ImageType.Primary];
    }

    /// <inheritdoc />
    public async Task<DynamicImageResponse> GetImage(BaseItem item, ImageType type, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var serviceScope = _serviceScopeFactory.CreateAsyncScope();

        var context = serviceScope.ServiceProvider.GetRequiredService<Context>();
        var dbSet = context.Set<PosterRecord>();
        var record = await dbSet.FindAsync([item.Id], cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var file = _searcher.Search(item, cancellationToken);

        if (!file.Exists)
        {
            if (record == null)
                return ValueCache.Empty.Value;

            // remove existing record if were not able to match
            dbSet.Remove(record);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return ValueCache.Empty.Value;
        }

        var now = _timeProvider.GetLocalNow();
        if (record == null)
        {
            record = new PosterRecord(item.Id, now, file);
            record.SetPosterFile(file, now);
            await dbSet.AddAsync(record, cancellationToken);
        }
        else
        {
            record.SetPosterFile(file, now);
            dbSet.Update(record);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var borderReplacer = serviceScope.ServiceProvider.GetRequiredService<IBorderReplacer>();
        return new DynamicImageResponse
        {
            Stream = borderReplacer.Replace(file.FullName), HasImage = true, Format = ImageFormat.Jpg, Protocol = MediaProtocol.File
        };
    }

    /// <inheritdoc />
    public int Order => 1;
}
