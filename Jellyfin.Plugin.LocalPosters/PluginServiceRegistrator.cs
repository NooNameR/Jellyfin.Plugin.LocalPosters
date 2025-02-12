using Jellyfin.Plugin.LocalPosters.Configuration;
using Jellyfin.Plugin.LocalPosters.Matchers;
using Jellyfin.Plugin.LocalPosters.Utils;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.LocalPosters;

/// <summary>
///
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<IMatcherFactory, MatcherFactory>();
        serviceCollection.AddSingleton<Func<PluginConfiguration, IBorderReplacer>>(CreateBorderReplacer);
    }

    static IBorderReplacer CreateBorderReplacer(PluginConfiguration pluginConfiguration)
    {
        if (!pluginConfiguration.EnableBorderReplacer)
            return new SkiaDefaultBorderReplacer();

        if (pluginConfiguration.RemoveBorder || !pluginConfiguration.SkColor.HasValue)
            return new SkiaSharpBorderRemover();

        if (pluginConfiguration.SkColor.HasValue)
            return new SkiaSharpBorderReplacer(pluginConfiguration.SkColor.Value);

        return new SkiaDefaultBorderReplacer();
    }
}
