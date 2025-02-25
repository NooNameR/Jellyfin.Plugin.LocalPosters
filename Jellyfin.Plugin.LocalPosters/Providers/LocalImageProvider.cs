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
public class LocalImageProvider(
    IServiceScopeFactory serviceScopeFactory,
    TimeProvider timeProvider,
    IImageSearcher searcher)
    : IDynamicImageProvider, IHasOrder
{
    /// <inheritdoc />
    public bool Supports(BaseItem item)
    {
        return searcher.IsSupported(item);
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

        await using var serviceScope = serviceScopeFactory.CreateAsyncScope();

        var context = serviceScope.ServiceProvider.GetRequiredService<Context>();
        var dbSet = context.Set<PosterRecord>();
        var record = await dbSet.FindAsync([item.Id], cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var file = searcher.Search(item, cancellationToken);

        if (!file.Exists)
        {
            if (record == null)
                return ValueCache.Empty.Value;

            // remove existing record if were not able to match
            dbSet.Remove(record);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return ValueCache.Empty.Value;
        }

        var now = timeProvider.GetLocalNow();
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

        var borderReplacerProvider = serviceScope.ServiceProvider.GetRequiredService<BorderReplacerProvider>();
        var borderReplacer = borderReplacerProvider.Provide(item.GetBaseItemKind(), type);

        return new DynamicImageResponse
        {
            Stream = borderReplacer.Replace(file.FullName), HasImage = true, Format = ImageFormat.Jpg, Protocol = MediaProtocol.File
        };
    }

    /// <inheritdoc />
    public int Order => 1;
}
