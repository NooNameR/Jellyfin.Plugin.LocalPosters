using Jellyfin.Data.Enums;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.LocalPosters.Utils;

public class NoopImageResizer : IImageProcessor
{
    public static readonly Lazy<IImageProcessor> Instance = new(() => new NoopImageResizer());

    private NoopImageResizer()
    {
    }

    public Stream Process(BaseItemKind kind, ImageType imageType, Stream stream)
    {
        return stream;
    }
}
