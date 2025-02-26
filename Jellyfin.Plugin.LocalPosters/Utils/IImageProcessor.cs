using Jellyfin.Data.Enums;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.LocalPosters.Utils;

/// <summary>
///
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="kind"></param>
    /// <param name="imageType"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    Stream Process(BaseItemKind kind, ImageType imageType, Stream stream);
}
