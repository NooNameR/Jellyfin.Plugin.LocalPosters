using SkiaSharp;
using static System.IO.File;

namespace Jellyfin.Plugin.LocalPosters.Utils;

/// <summary>
///
/// </summary>
public interface IBorderReplacer
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    Stream RemoveBorder(string source, string destination);

    /// <summary>
    ///
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="color"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    Stream ReplaceBorder(string source, string destination, SKColor color);
}

/// <summary>
///
/// </summary>
public class SkiaSharpBorderReplacer : IBorderReplacer
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    public Stream RemoveBorder(string source, string destination)
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

    /// <summary>
    ///
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="color"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public Stream ReplaceBorder(string source, string destination, SKColor color)
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
        using var resizedImage = new SKBitmap(1000, 1500);
        using (var canvas = new SKCanvas(resizedImage))
        {
            canvas.DrawBitmap(newImage, new SKRect(0, 0, 1000, 1500));

            // Use MemoryStream to store the result
            var memoryStream = new MemoryStream();
            resizedImage.Encode(memoryStream, SKEncodedImageFormat.Jpeg, 100);

            // Reset the memory stream position to the start before returning
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }
    }
}
