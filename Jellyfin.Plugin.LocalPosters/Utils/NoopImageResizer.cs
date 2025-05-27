using Jellyfin.Data.Enums;
using MediaBrowser.Model.Entities;
using SkiaSharp;

namespace Jellyfin.Plugin.LocalPosters.Utils;

public class NoopImageResizer : IImageProcessor
{
    public static readonly Lazy<IImageProcessor> Instance = new(() => new NoopImageResizer());

    private NoopImageResizer()
    {
    }

    public Stream Process(BaseItemKind kind, ImageType imageType, Stream stream)
    {
        using var bitmap = SKBitmap.Decode(stream);

        // Use MemoryStream to store the result
        var memoryStream = new MemoryStream();
        bitmap.Encode(memoryStream, SKEncodedImageFormat.Jpeg, 100);

        // Reset the memory stream position to the start before returning
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }
}
