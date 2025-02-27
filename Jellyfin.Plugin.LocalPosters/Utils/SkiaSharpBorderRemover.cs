using Jellyfin.Data.Enums;
using MediaBrowser.Model.Entities;
using SkiaSharp;

namespace Jellyfin.Plugin.LocalPosters.Utils;

/// <summary>
///
/// </summary>
public class SkiaSharpBorderRemover(IImageProcessor next) : IImageProcessor
{
    /// <inheritdoc />
    public Stream Process(BaseItemKind kind, ImageType imageType, Stream stream)
    {
        using var bitmap = SKBitmap.Decode(stream);
        int width = bitmap.Width;
        int height = bitmap.Height;

        using var cropped = new SKBitmap(width - 50, height - 25);
        using var canvas = new SKCanvas(cropped);
        // Copy cropped region
        canvas.DrawBitmap(bitmap,
            new SKRect(25, 25, width - 25, height), // Source (crop area)
            new SKRect(0, 0, width - 50, height - 25) // Destination
        );

        // Draw black bottom border
        using var paint = new SKPaint();
        paint.Color = SKColors.Black;
        canvas.DrawRect(0, cropped.Height - 25, width - 50, 25, paint);

        using var memoryStream = new MemoryStream();
        cropped.Encode(memoryStream, SKEncodedImageFormat.Jpeg, 100);
        // Reset the memory stream position to the start before returning
        memoryStream.Seek(0, SeekOrigin.Begin);
        return next.Process(kind, imageType, memoryStream);
    }
}
