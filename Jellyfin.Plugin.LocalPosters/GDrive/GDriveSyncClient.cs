using Google.Apis.Drive.v3;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using File = Google.Apis.Drive.v3.Data.File;

namespace Jellyfin.Plugin.LocalPosters.GDrive;

/// <summary>
///
/// </summary>
public sealed class GDriveSyncClient : ISyncClient
{
    private readonly ILogger _logger;
    private readonly DriveService _driveService;
    private readonly string _folderId;
    private readonly string _path;
    private readonly IFileSystem _fileSystem;
    private readonly SemaphoreSlim _limiter;

    /// <summary>
    ///
    /// </summary>
    public const string ApplicationName = "Jellyfin.Plugin.LocalPosters";

    /// <summary>
    ///
    /// </summary>
    public const string DownloadLimiterKey = "DownloadLimiterKey";

    /// <summary>
    ///
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="driveService"></param>
    /// <param name="folderId"></param>
    /// <param name="path"></param>
    /// <param name="fileSystem"></param>
    /// <param name="limiter"></param>
    public GDriveSyncClient(ILogger logger, DriveService driveService, string folderId, string path,
        IFileSystem fileSystem, SemaphoreSlim limiter)
    {
        _logger = logger;
        _driveService = driveService;
        _folderId = folderId;
        _path = path;
        _fileSystem = fileSystem;
        _limiter = limiter;
    }

    /// <inheritdoc />
    public async Task<long> SyncAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        var itemIds = new List<(string, FileSystemMetadata)>();

        await _limiter.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var queue = new Queue<(string, FileSystemMetadata)>();
            queue.Enqueue((_folderId, _fileSystem.GetDirectoryInfo(_path)));
            while (queue.TryDequeue(out var q))
            {
                var folder = q.Item2;
                if (!folder.Exists)
                    Directory.CreateDirectory(folder.FullName);

                var request = _driveService.Files.List();
                request.Q = $"'{q.Item1}' in parents and trashed=false and (mimeType contains 'image/' or mimeType='application/vnd.google-apps.folder')";
                request.Fields = "nextPageToken, files(id, name, size, mimeType, modifiedTime)";
                request.PageSize = 1000;

                do
                {
                    var result = await request.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                    foreach (var file in result.Files)
                    {
                        var filePath = _fileSystem.GetFileInfo(Path.Combine(folder.FullName, file.Name));

                        if (file.MimeType == "application/vnd.google-apps.folder")
                            queue.Enqueue((file.Id, filePath));
                        else if (ShouldDownload(filePath, file))
                            itemIds.Add((file.Id, filePath));
                    }

                    request.PageToken = result.NextPageToken;
                } while (!string.IsNullOrEmpty(request.PageToken));
            }
        }
        finally
        {
            _limiter.Release();
        }

        progress.Report(10);

        var completed = 0;

        var tasks = itemIds.Select((item, _) => Task.Run(async () =>
        {
            await _limiter.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await DownloadFile(_logger, _driveService, item.Item1, item.Item2, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _limiter.Release();

                progress.Report((Interlocked.Increment(ref completed) / (double)itemIds.Count) * 90.0);
            }
        }, cancellationToken));

        await Task.WhenAll(tasks).ConfigureAwait(false);

        progress.Report(100);
        return itemIds.Count;

        static async Task DownloadFile(ILogger logger, DriveService service, string fileId,
            FileSystemMetadata saveTo,
            CancellationToken cancellationToken)
        {
            var request = service.Files.Get(fileId);
            using (var stream = new FileStream(saveTo.FullName, FileMode.Create, FileAccess.Write))
                await request.DownloadAsync(stream, cancellationToken).ConfigureAwait(false);
        }

        static bool ShouldDownload(FileSystemMetadata file, File driveFile)
        {
            return !file.Exists || file.Length != driveFile.Size || driveFile.ModifiedTimeDateTimeOffset > file.LastWriteTimeUtc;
        }
    }
}
