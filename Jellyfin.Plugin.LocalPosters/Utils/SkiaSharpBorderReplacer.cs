using System.Drawing;
using SkiaSharp;
using static System.IO.File;

namespace Jellyfin.Plugin.LocalPosters.Utils;

/// <summary>
///
/// </summary>
public class SkiaSharpBorderReplacer(Size size, SKColor color) : IBorderReplacer
{
    /// <inheritdoc />
    public Stream Replace(string source)
    {
        using var stream = OpenRead(source);
        using var bitmap = SKBitmap.Decode(stream);
        int width = bitmap.Width;
        int height = bitmap.Height;

        // Crop 25px from all sides
        using SKBitmap cropped = new SKBitmap(width - 50, height - 50);
        using (var canvas = new SKCanvas(cropped))
        {
            canvas.DrawBitmap(bitmap,
                new SKRect(25, 25, width - 25, height - 25), // Source (crop area)
                new SKRect(0, 0, width - 50, height - 50) // Destination
            );
        }

        // Create new image with custom color background
        using SKBitmap newImage = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(newImage))
        {
            canvas.Clear(color); // Fill background with custom color
            canvas.DrawBitmap(cropped, new SKPoint(25, 25)); // Paste cropped image
        }

        // Resize to 1000x1500
        using var resizedImage = new SKBitmap(size.Width, size.Height);
        using (var canvas = new SKCanvas(resizedImage))
        {
            canvas.DrawBitmap(newImage, new SKRect(0, 0, resizedImage.Width, resizedImage.Height));

            // Use MemoryStream to store the result
            var memoryStream = new MemoryStream();
            resizedImage.Encode(memoryStream, SKEncodedImageFormat.Jpeg, 100);

            // Reset the memory stream position to the start before returning
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }
    }
}
