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
    private readonly SemaphoreSlim _lock = new(1);
    private DriveService? _driveService;

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
                var clientSecretFile = fileSystem.GetFileInfo(configuration.GoogleClientSecretFile);
                if (clientSecretFile.Exists)
                {
                    var clientSecrets = await GoogleClientSecrets.FromFileAsync(clientSecretFile.FullName, cancellationToken)
                        .ConfigureAwait(false);
                    ArgumentNullException.ThrowIfNull(clientSecrets, nameof(clientSecrets));

                    var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        clientSecrets.Secrets,
                        [DriveService.Scope.Drive],
                        GDriveSyncClient.User, cancellationToken, dataStore).ConfigureAwait(false);

                    if (string.IsNullOrEmpty(credential.Token.RefreshToken))
                        logger.LogWarning("Refresh token is missing. Please revoke access from Google API Dashboard and request a new token.");

                    _driveService = new DriveService(new BaseClientService.Initializer
                    {
                        HttpClientInitializer = credential, ApplicationName = GDriveSyncClient.ApplicationName
                    });
                }
            }

            var saCredentialFile = fileSystem.GetFileInfo(configuration.GoogleSaCredentialFile);
            if (saCredentialFile.Exists)
            {
                var credential = (await GoogleCredential.FromFileAsync(saCredentialFile.FullName, cancellationToken)
                        .ConfigureAwait(false))
                    .CreateScoped(DriveService.Scope.Drive);

                _driveService = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential, ApplicationName = GDriveSyncClient.ApplicationName
                });
            }

            return _driveService ?? throw new ArgumentException("No Google credentials were found.");
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _lock.Dispose();
        _driveService?.Dispose();
    }
}
