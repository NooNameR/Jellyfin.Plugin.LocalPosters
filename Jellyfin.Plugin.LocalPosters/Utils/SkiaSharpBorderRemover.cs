using SkiaSharp;
using static System.IO.File;

namespace Jellyfin.Plugin.LocalPosters.Utils;

/// <summary>
///
/// </summary>
public class SkiaSharpBorderRemover : IBorderReplacer
{
    /// <inheritdoc />
    public Stream Replace(string source)
    {
        using var stream = OpenRead(source);
        using var bitmap = SKBitmap.Decode(stream);
        int width = bitmap.Width;
        int height = bitmap.Height;

        using var cropped = new SKBitmap(width - 50, height - 25);
        using (var canvas = new SKCanvas(cropped))
        {
            // Copy cropped region
            canvas.DrawBitmap(bitmap,
                new SKRect(25, 25, width - 25, height), // Source (crop area)
                new SKRect(0, 0, width - 50, height - 25) // Destination
            );

            // Draw black bottom border
            using var paint = new SKPaint();
            paint.Color = SKColors.Black;
            canvas.DrawRect(0, cropped.Height - 25, width - 50, 25, paint);
        }

        // Resize to 1000x1500
        using var resizedImage = new SKBitmap(1000, 1500);
        using (var canvas = new SKCanvas(resizedImage))
        {
            canvas.DrawBitmap(cropped, new SKRect(0, 0, 1000, 1500));

            // Use MemoryStream to store the result
            var memoryStream = new MemoryStream();
            resizedImage.Encode(memoryStream, SKEncodedImageFormat.Jpeg, 100);

            // Reset the memory stream position to the start before returning
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }
    }
}
