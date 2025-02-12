using MediaBrowser.Model.Plugins;
using SkiaSharp;

namespace Jellyfin.Plugin.LocalPosters.Configuration;

/// <summary>
///
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    ///
    /// </summary>
#pragma warning disable CA1819
    public string[] Folders { get; set; }
#pragma warning restore CA1819

    /// <summary>
    ///
    /// </summary>
    public PluginConfiguration()
    {
        Folders =
        [
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
        ];
    }


    /// <summary>
    ///
    /// </summary>
    public bool RemoveBorder { get; set; } = true;

    /// <summary>
    /// Hex color for border
    /// </summary>
    public string BorderColor { get; set; } = string.Empty;

    /// <summary>
    ///
    /// </summary>
    public SKColor? SkColor => !string.IsNullOrEmpty(BorderColor) && SKColor.TryParse(BorderColor, out var c) ? c : null;
}
