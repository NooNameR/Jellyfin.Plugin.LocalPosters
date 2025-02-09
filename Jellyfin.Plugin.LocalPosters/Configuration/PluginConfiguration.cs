using System.Collections.ObjectModel;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.LocalPosters.Configuration;

/// <summary>
///
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    ///
    /// </summary>
    public Collection<string> Folders { get; } = new([
        "/posters/christophedcgdriveposters",
        "/posters/reitenthgdriveposters",
        "/posters/mareaugdriveposters",
        "/posters/dsaqgdriveposters",
        "/posters/quafleygdriveposters",
        "/posters/stupifierrgdriveposters",
        "/posters/saharagdriveposters",
        "/posters/majorgiant1gdriveposters",
        "/posters/lioncitygamingdriveposters",
        "/posters/iamspartacus1gdriveposters",
        "/posters/bzwartgdriveposters",
        "/posters/solengdriveposters",
        "/posters/solen1gdriveposters",
        "/posters/zaroxgdriveposters",
        "/posters/drazzilbgdriveposters"
    ]);
}
