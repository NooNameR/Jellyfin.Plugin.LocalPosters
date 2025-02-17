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
#pragma warning disable IDISP025
public class LocalPostersPlugin : BasePlugin<PluginConfiguration>, IHasWebPages, IDisposable
#pragma warning restore IDISP025
{
    private readonly object _lock = new();
    private CancellationTokenSource? _cancellationTokenSource;

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

        ConfigurationChanged += (_, _) => ResetToken();

        var context = new Context(optionsBuilder.Options);
        try
        {
            context.ApplyMigration();
        }
        catch (Exception e)
        {
            loggerFactory.CreateLogger<LocalPostersPlugin>().LogWarning(e, "Failed to perform migrations.");
        }
        finally { context.Dispose(); }

        Instance = this;
    }

    /// <summary>
    ///
    /// </summary>
    public CancellationToken ConfigurationToken
    {
        get
        {
            lock (_lock)
            {
                _cancellationTokenSource ??= new CancellationTokenSource();
                return _cancellationTokenSource.Token;
            }
        }
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

    void ResetToken()
    {
        lock (_lock)
        {
            if (_cancellationTokenSource == null)
                return;

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        ResetToken();
    }
}
