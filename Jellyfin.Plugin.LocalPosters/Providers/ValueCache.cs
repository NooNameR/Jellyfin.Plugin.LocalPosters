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
    public static readonly Lazy<DynamicImageResponse> Empty = new(() => new DynamicImageResponse { HasImage = false });
}
