namespace Jellyfin.Plugin.LocalPosters.GDrive;

/// <summary>
///
/// </summary>
public interface ISyncClient
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long> SyncAsync(IProgress<double> progress, CancellationToken cancellationToken = default);
}
