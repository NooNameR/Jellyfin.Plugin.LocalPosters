using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.IO;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <summary>
///
/// </summary>
public interface IImageSearcher
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    bool IsSupported(BaseItem item);

    /// <summary>
    ///
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    FileSystemMetadata Search(BaseItem item, CancellationToken cancellationToken);
}
