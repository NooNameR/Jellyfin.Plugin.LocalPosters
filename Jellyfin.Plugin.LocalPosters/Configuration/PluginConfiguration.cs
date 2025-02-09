using System.Collections.ObjectModel;
using MediaBrowser.Model.IO;
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

    /// <summary>
    ///
    /// </summary>
    public bool RemoveBorder { get; set; } = true;

    /// <summary>
    ///
    /// </summary>
    public string AssetsPath { get; set; } = "/config/data/assets";
}

/// <summary>
///
/// </summary>
public static class PluginConfigurationExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="pluginConfiguration"></param>
    /// <param name="fileSystem"></param>
    /// <returns></returns>
    public static FileSystemMetadata AssetsPath(this PluginConfiguration pluginConfiguration, IFileSystem fileSystem)
    {
        return fileSystem.GetDirectoryInfo(pluginConfiguration.AssetsPath);
    }
}
