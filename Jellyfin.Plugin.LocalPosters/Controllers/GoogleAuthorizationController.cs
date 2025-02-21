using System.Net.Mime;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;
using Jellyfin.Plugin.LocalPosters.Configuration;
using Jellyfin.Plugin.LocalPosters.GDrive;
using MediaBrowser.Model.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.LocalPosters.Controllers;

/// <summary>
///
/// </summary>
[Authorize]
[ApiController]
[Route("LocalPosters/[controller]/[action]")]
[Produces(MediaTypeNames.Application.Json)]
public class GoogleAuthorizationController(
    PluginConfiguration configuration,
    GDriveServiceProvider provider,
    IFileSystem fileSystem,
    IDataStore dataStore,
    ILogger<GoogleAuthorizationController> logger) : ControllerBase
{
    private string? RedirectUrl() => Url.Action("Callback", "GoogleAuthorization", null, Request.Scheme);

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<string>> Authorize()
    {
        var clientSecretFile = configuration.GoogleClientSecretFile(fileSystem);
        if (!clientSecretFile.Exists)
            return BadRequest("Client secret file does not exist");

        var clientSecrets = await GoogleClientSecrets.FromFileAsync(clientSecretFile.FullName, HttpContext.RequestAborted)
            .ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(clientSecrets, nameof(clientSecrets));

        using var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = clientSecrets.Secrets, Scopes = [DriveService.Scope.Drive], DataStore = dataStore, Prompt = "consent"
        });

        var redirectUrl = RedirectUrl();
        var authUrl = flow.CreateAuthorizationCodeRequest(redirectUrl).Build();
        logger.LogInformation("Redirecting to Google API: {AuthUrl}, with callback to: {CallbackUrl}", authUrl, redirectUrl);

        return Ok(authUrl.ToString());
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<bool>> Verify()
    {
        try
        {
            var driveService = await provider.Provide(HttpContext.RequestAborted).ConfigureAwait(false);
            var request = driveService.Files.List();
            request.PageSize = 1;
            await request.ExecuteAsync(HttpContext.RequestAborted).ConfigureAwait(false);
            return true;
        }
        catch (Exception e)
        {
            logger.LogDebug(e, "Google API verification failed");
            return false;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet]
    [Produces(MediaTypeNames.Text.Html)]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        var clientSecretFile = configuration.GoogleClientSecretFile(fileSystem);
        var clientSecrets = await GoogleClientSecrets.FromFileAsync(clientSecretFile.FullName, CancellationToken.None)
            .ConfigureAwait(false);

        ArgumentNullException.ThrowIfNull(clientSecrets, nameof(clientSecrets));

        using var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = clientSecrets.Secrets, Scopes = [DriveService.Scope.Drive], DataStore = dataStore
        });

        await flow.ExchangeCodeForTokenAsync(GDriveSyncClient.User, code, RedirectUrl(), CancellationToken.None)
            .ConfigureAwait(false);

        const string Html = """
                                        <html>
                                        <body>
                                            <script>
                                                window.opener?.postMessage('auth_success', '*');
                                                window.close();
                                            </script>
                                            <p>You may close this window.</p>
                                        </body>
                                        </html>
                            """;
        return Content(Html, "text/html");
    }
}
