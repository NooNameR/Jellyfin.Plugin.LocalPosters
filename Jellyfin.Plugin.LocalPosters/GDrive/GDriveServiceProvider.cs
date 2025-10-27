using System.Diagnostics.CodeAnalysis;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Jellyfin.Plugin.LocalPosters.Configuration;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.LocalPosters.GDrive;

/// <summary>
///
/// </summary>
public sealed class GDriveServiceProvider(
    PluginConfiguration configuration,
    LocalPostersPlugin plugin,
    IFileSystem fileSystem,
    IDataStore dataStore,
    ILogger<GDriveServiceProvider> logger)
    : IDisposable
{
    public const string User = "local-posters-user";

    private const string ApplicationName = "Jellyfin.Plugin.LocalPosters";
    public static readonly HashSet<string> Scopes = [DriveService.Scope.DriveReadonly];
    private readonly SemaphoreSlim _lock = new(1);
    private DriveService? _driveService;

    /// <inheritdoc />
    public void Dispose()
    {
        _lock.Dispose();
        _driveService?.Dispose();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP003:Dispose previous before re-assigning")]
    public async Task<DriveService> Provide(CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_driveService is not null)
                return _driveService;

            if (fileSystem.GetFiles(plugin.GDriveTokenFolder).Any())
            {
                var clientSecretFile = configuration.GoogleClientSecretFile(fileSystem);
                if (clientSecretFile.Exists)
                {
                    logger.LogDebug("Using token from: {GDriveTokenFolder} and {GoogleClientSecretFile} client secret file",
                        plugin.GDriveTokenFolder, clientSecretFile.FullName);

                    var clientSecrets = await GoogleClientSecrets.FromFileAsync(clientSecretFile.FullName, cancellationToken)
                        .ConfigureAwait(false);
                    ArgumentNullException.ThrowIfNull(clientSecrets, nameof(clientSecrets));

                    var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        clientSecrets.Secrets, Scopes, User, cancellationToken, dataStore).ConfigureAwait(false);

                    if (string.IsNullOrEmpty(credential.Token.RefreshToken))
                        logger.LogWarning(
                            "Refresh token is missing. Please revoke access from Google API Dashboard and request a new token.");

                    _driveService = new DriveService(new BaseClientService.Initializer
                    {
                        HttpClientInitializer = credential, ApplicationName = ApplicationName
                    });
                }
            }

            var saCredentialFile = configuration.GoogleSaCredentialFile(fileSystem);
            if (saCredentialFile.Exists)
            {
                logger.LogDebug("Using Service Account credentials file: {GoogleSaCredentialFile}",
                    saCredentialFile.FullName);

                var credential = (await CredentialFactory.FromFileAsync<GoogleCredential>(saCredentialFile.FullName, cancellationToken)
                    .ConfigureAwait(false)).CreateScoped(Scopes);

                _driveService = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential, ApplicationName = ApplicationName
                });
            }

            return _driveService ?? throw new ArgumentException("No Google credentials were found.");
        }
        finally
        {
            _lock.Release();
        }
    }
}
