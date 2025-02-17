using System.Net.Mime;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;
using Jellyfin.Plugin.LocalPosters.Configuration;
using Jellyfin.Plugin.LocalPosters.GDrive;
using MediaBrowser.Controller;
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
    IServerApplicationHost host,
    PluginConfiguration configuration,
    GDriveServiceProvider provider,
    IFileSystem fileSystem,
    IDataStore dataStore,
    ILogger<GoogleAuthorizationController> logger) : ControllerBase
{
    private string? RedirectUrl()
    {
        var url = host.GetSmartApiUrl(Request);
        return string.IsNullOrEmpty(url)
            ? Url.Action("Callback", "GoogleAuthorization", null, Request.Scheme)
            : new Uri(new Uri(url), Url.Action("Callback", "GoogleAuthorization")).ToString();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<string>> Authorize()
    {
        var clientSecretFile = fileSystem.GetFileInfo(configuration.GoogleClientSecretFile);
        if (!clientSecretFile.Exists)
            return BadRequest("Client secret file does not exist");

        var clientSecrets = await GoogleClientSecrets.FromFileAsync(clientSecretFile.FullName, HttpContext.RequestAborted)
            .ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(clientSecrets, nameof(clientSecrets));

        using var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = clientSecrets.Secrets, Scopes = [DriveService.Scope.Drive], DataStore = dataStore
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
        var driveService = await provider.Provide(HttpContext.RequestAborted).ConfigureAwait(false);
        var request = driveService.Files.List();
        request.PageSize = 1;
        await request.ExecuteAsync(HttpContext.RequestAborted).ConfigureAwait(false);
        return true;
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
        var clientSecretFile = fileSystem.GetFileInfo(configuration.GoogleClientSecretFile);
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
