using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <inheritdoc />
public class CachedImageSearcher(IImageSearcher searcher, IMemoryCache cache, IFileSystem fileSystem, LocalPostersPlugin plugin)
    : IImageSearcher
{
    private static readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

    /// <inheritdoc />
    public bool IsSupported(BaseItem item)
    {
        return searcher.IsSupported(item);
    }

    /// <inheritdoc />
    public FileSystemMetadata Search(BaseItem item, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKey(item);
        FileSystemMetadata file;
        if (cache.TryGetValue<string>(cacheKey, out var cachedPath) && cachedPath != null)
        {
            file = fileSystem.GetFileInfo(cachedPath);

            if (file.Exists)
                return file;

            cache.Remove(cacheKey);
        }

        file = searcher.Search(item, cancellationToken);
        if (file.Exists)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .AddExpirationToken(new CancellationChangeToken(plugin.ConfigurationToken))
                .SetAbsoluteExpiration(_cacheDuration);
            cache.Set(CacheKey(item), file.FullName, cacheEntryOptions);
        }

        return file;
    }

    static string CacheKey(BaseItem item)
    {
        return $"image-searcher-{item.Id}";
    }
}
