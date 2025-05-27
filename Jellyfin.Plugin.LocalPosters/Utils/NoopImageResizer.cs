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
        var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }
}
