using System.Globalization;
using Jellyfin.Plugin.LocalPosters.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
    public LocalPostersPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILoggerFactory loggerFactory) : base(
        applicationPaths,
        xmlSerializer)
    {
        const string PluginDirName = "local-posters";
        var dataFolder = Path.Join(applicationPaths.DataPath, PluginDirName);

        if (!Directory.Exists(dataFolder))
            Directory.CreateDirectory(dataFolder);

        DbPath = Path.Join(dataFolder, Context.DbName);
        GDriveTokenFolder = Path.Join(dataFolder, "gdrive");

        if (Directory.Exists(GDriveTokenFolder))
            Directory.CreateDirectory(GDriveTokenFolder);

        var optionsBuilder = new DbContextOptionsBuilder<Context>();
        optionsBuilder.UseSqlite($"Data Source={DbPath}")
            .UseLoggerFactory(loggerFactory)
            .EnableSensitiveDataLogging(false);

        var context = new Context(optionsBuilder.Options);
        try
        {
            context.ApplyMigration();
        }
        catch (Exception e)
        {
#pragma warning disable CA1848
            loggerFactory.CreateLogger<LocalPostersPlugin>().LogWarning(e, "Failed to perform migrations.");
#pragma warning restore CA1848
        }
        finally { context.Dispose(); }

        Instance = this;
    }

    /// <summary>
    ///
    /// </summary>
    public string GDriveTokenFolder { get; }

    /// <summary>
    ///
    /// </summary>
    public static LocalPostersPlugin? Instance { get; private set; }

    /// <inheritdoc />
    public override string Name => "Local Posters";

    /// <inheritdoc />
    public override Guid Id => new("3938fe98-b7b2-4333-b678-c4c4e339d232");

    /// <summary>
    ///
    /// </summary>
    public string DbPath { get; }

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html",
                    GetType().Namespace)
            }
        ];
    }
}
