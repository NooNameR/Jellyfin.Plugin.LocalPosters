using System.Drawing;
using SkiaSharp;

namespace Jellyfin.Plugin.LocalPosters.Utils;

using static File;

/// <summary>
///
/// </summary>
public class SkiaDefaultBorderReplacer(Size size) : IBorderReplacer
{
    /// <inheritdoc />
    public Stream Replace(string source)
    {
        using var stream = OpenRead(source);
        using var bitmap = SKBitmap.Decode(stream);

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
