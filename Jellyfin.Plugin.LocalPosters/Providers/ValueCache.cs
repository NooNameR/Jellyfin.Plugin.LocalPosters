using MediaBrowser.Controller.Providers;

namespace Jellyfin.Plugin.LocalPosters.Providers;

/// <summary>
///
/// </summary>
public static class ValueCache
{
    /// <summary>
    ///
    /// </summary>
    public static readonly Lazy<Task<DynamicImageResponse>> Empty = new(() => Task.FromResult(new DynamicImageResponse { HasImage = false }));
}
