using Jellyfin.Data.Enums;
using Jellyfin.Plugin.LocalPosters.Utils;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.LocalPosters.Tests;

public class NoopImageProcessor :
    IImageProcessor
{
    public static readonly IImageProcessor Instance = new NoopImageProcessor();

    private NoopImageProcessor()
    {
    }

    public Stream Process(BaseItemKind kind, ImageType imageType, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream;
    }
}
