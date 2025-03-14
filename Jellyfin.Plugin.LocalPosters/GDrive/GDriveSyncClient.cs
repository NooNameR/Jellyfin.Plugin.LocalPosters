using Google.Apis.Drive.v3;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.LocalPosters.GDrive;

/// <summary>
///
/// </summary>
public sealed class GDriveSyncClient(
    ILogger logger,
    GDriveServiceProvider driveServiceProvider,
    string folderId,
    string path,
    IFileSystem fileSystem,
    SemaphoreSlim limiter) : ISyncClient
{
    /// <summary>
    ///
    /// </summary>
    public const string DownloadLimiterKey = "DownloadLimiterKey";

    private const string FolderMimeType = "application/vnd.google-apps.folder";

    /// <inheritdoc />
    public async Task<long> SyncAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        var itemIds = new List<(string, FileSystemMetadata)>();

        await limiter.WaitAsync(cancellationToken).ConfigureAwait(false);

        var driveService = await driveServiceProvider.Provide(cancellationToken).ConfigureAwait(false);

        try
        {
            var queue = new Queue<(string, FileSystemMetadata)>();
            queue.Enqueue((folderId, fileSystem.GetDirectoryInfo(path)));
            while (queue.TryDequeue(out var q))
            {
                var folder = q.Item2;
                if (!folder.Exists)
                    Directory.CreateDirectory(folder.FullName);

                var request = driveService.Files.List();
                request.Q =
                    $"'{q.Item1}' in parents and trashed=false and (mimeType contains 'image/' or mimeType='{FolderMimeType}')";
                request.Fields = "nextPageToken, files(id, name, size, mimeType, md5Checksum, modifiedTime)";
                request.PageSize = 1000;

                do
                {
                    var result = await request.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                    logger.LogDebug("Discovered {NumFiles} (PageSize: {PageSize}) files in: {FolderId}", result.Files.Count,
                        request.PageSize, q.Item1);

                    foreach (var file in result.Files)
                    {
                        var filePath = fileSystem.GetFileInfo(Path.Combine(folder.FullName, file.Name));

                        if (file.MimeType == FolderMimeType)
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
            limiter.Release();
        }

        progress.Report(10);

        var completed = 0;

        logger.LogInformation("Retrieved {Number} new files from {FolderId}", itemIds.Count, folderId);

        var tasks = itemIds.Select((item, _) => Task.Run(async () =>
        {
            await limiter.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                logger.LogDebug("Starting file: {FileId} download to: {Folder}", item.Item1, item.Item2.FullName);

                await DownloadFile(driveService, item.Item1, item.Item2).ConfigureAwait(false);
                logger.LogDebug("File: {FileId} download completed to: {Folder}", item.Item1, item.Item2.FullName);
            }
            catch (OperationCanceledException e) when (e.CancellationToken == cancellationToken)
            {
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "File: {FileId} download failed to: {Folder}", item.Item1, item.Item2.FullName);
                throw;
            }
            finally
            {
                limiter.Release();

                progress.Report((Interlocked.Increment(ref completed) / (double)itemIds.Count) * 90.0);
            }
        }, cancellationToken));

        await Task.WhenAll(tasks).ConfigureAwait(false);

        progress.Report(100);
        return itemIds.Count;

        async Task DownloadFile(DriveService service, string fileId,
            FileSystemMetadata saveTo)
        {
            var request = service.Files.Get(fileId);
            try
            {
                await using var stream = new FileStream(saveTo.FullName, FileMode.Create, FileAccess.Write);
                await request.DownloadAsync(stream, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                TryRemoveFile(saveTo);
            }
        }

        static void TryRemoveFile(FileSystemMetadata file)
        {
            try
            {
                File.Delete(file.FullName);
            }
            catch
            {
                //ignore
            }
        }

        static bool ShouldDownload(FileSystemMetadata file, Google.Apis.Drive.v3.Data.File driveFile)
        {
            return !file.Exists || file.Length != driveFile.Size || driveFile.ModifiedTimeDateTimeOffset > file.LastWriteTimeUtc;
        }
    }
}
