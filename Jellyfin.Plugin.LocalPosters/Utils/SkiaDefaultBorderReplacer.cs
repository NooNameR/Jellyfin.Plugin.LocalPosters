using SkiaSharp;

namespace Jellyfin.Plugin.LocalPosters.Utils;

using static File;

/// <summary>
///
/// </summary>
public class SkiaDefaultBorderReplacer : IBorderReplacer
{
    /// <inheritdoc />
    public Stream Replace(string source)
    {
        using var stream = OpenRead(source);
        using var bitmap = SKBitmap.Decode(stream);

        // Resize to 1000x1500
        using var resizedImage = new SKBitmap(1000, 1500);
        using var canvas = new SKCanvas(resizedImage);
        canvas.DrawBitmap(bitmap, new SKRect(0, 0, 1000, 1500));

        // Use MemoryStream to store the result
        var memoryStream = new MemoryStream();
        resizedImage.Encode(memoryStream, SKEncodedImageFormat.Jpeg, 100);

        // Reset the memory stream position to the start before returning
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }
}
