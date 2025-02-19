using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;

namespace Jellyfin.Plugin.LocalPosters.Utils;

public static class ProviderManagerExtensions
{
    public static bool HasImageProviderEnabled(this IProviderManager manager, BaseItem item, ImageRefreshOptions refreshOptions)
    {
        var providers = manager.GetImageProviders(item, refreshOptions);
        return providers.Any(x => x.Name == LocalPostersPlugin.ProviderName);
    }
}
