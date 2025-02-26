using Jellyfin.Data.Enums;
using MediaBrowser.Model.Entities;
using SkiaSharp;

namespace Jellyfin.Plugin.LocalPosters.Utils;

/// <summary>
///
/// </summary>
public class SkiaImageResizer(ImageSizeProvider sizeProvider) : IImageProcessor
{
    /// <inheritdoc />
    public Stream Process(BaseItemKind kind, ImageType imageType, Stream stream)
    {
        using var bitmap = SKBitmap.Decode(stream);

        var size = sizeProvider.GetImageSize(kind, imageType);
        using var resizedImage = new SKBitmap(size.Width, size.Height);
        using var canvas = new SKCanvas(resizedImage);
        canvas.DrawBitmap(bitmap, new SKRect(0, 0, resizedImage.Width, resizedImage.Height));

        // Use MemoryStream to store the result
        var memoryStream = new MemoryStream();
        resizedImage.Encode(memoryStream, SKEncodedImageFormat.Jpeg, 100);

        // Reset the memory stream position to the start before returning
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }
}
