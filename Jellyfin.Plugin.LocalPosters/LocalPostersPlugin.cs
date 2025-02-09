using System.Globalization;
using Jellyfin.Plugin.LocalPosters.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.LocalPosters;

/// <summary>
///
/// </summary>
public class LocalPostersPlugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public const string ProviderName = "Local Posters";

    /// <inheritdoc />
    public LocalPostersPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(applicationPaths,
        xmlSerializer)
    {
        Instance = this;
    }

    /// <summary>
    ///
    /// </summary>
    public static LocalPostersPlugin? Instance { get; private set; }

    /// <inheritdoc />
    public override string Name => "Local Posters";

    /// <inheritdoc />
    public override Guid Id => new("E0FAD3EA-8996-4003-8C88-B7E77B26309A");

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace)
            }
        ];
    }
}
