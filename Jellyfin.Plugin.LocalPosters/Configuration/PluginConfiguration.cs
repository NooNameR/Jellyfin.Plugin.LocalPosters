using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using MediaBrowser.Model.Plugins;
using SkiaSharp;

namespace Jellyfin.Plugin.LocalPosters.Configuration;

/// <summary>
///
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    private static readonly Lazy<FolderItem[]> _cache = new(ReadJson);
    /// <summary>
    ///
    /// </summary>
#pragma warning disable CA1819
    public FolderItem[] Folders { get; set; } = _cache.Value;
#pragma warning restore CA1819

    /// <summary>
    ///
    /// </summary>
    public bool EnableBorderReplacer { get; set; } = true;

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
    public int ConcurrentDownloadLimit { get; set; } = Environment.ProcessorCount * 10;

    /// <summary>
    ///
    /// </summary>
    public string GoogleClientSecretFile { get; set; } = "/gdrive/client_secrets.json";

    /// <summary>
    ///
    /// </summary>
    public string GoogleSaCredentialFile { get; set; } = "/gdrive/rclone_sa.json";

    /// <summary>
    ///
    /// </summary>
    public SKColor? SkColor => !string.IsNullOrEmpty(BorderColor) && SKColor.TryParse(BorderColor, out var c) ? c : null;

    static FolderItem[] ReadJson()
    {
        var type = typeof(PluginConfiguration);
        using Stream? stream = type.Assembly.GetManifestResourceStream(string.Format(CultureInfo.InvariantCulture,
            "{0}.Configuration.gdrives.json", type));
        if (stream == null) return [];
        return JsonSerializer.Deserialize<FolderItem[]>(stream) ?? [];
    }
}

/// <summary>
///
/// </summary>
public class FolderItem
{
    /// <summary>
    ///
    /// </summary>
    public string? RemoteId { get; set; }

    /// <summary>
    ///
    /// </summary>
    public required string LocalPath { get; set; }

    /// <summary>
    ///
    /// </summary>
    [MemberNotNullWhen(true, nameof(RemoteId))]
    public bool IsRemote => !string.IsNullOrEmpty(RemoteId);
}
