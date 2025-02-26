using Jellyfin.Data.Enums;
using Jellyfin.Plugin.LocalPosters.Utils;
using MediaBrowser.Model.Entities;
using SkiaSharp;
using Xunit;

namespace Jellyfin.Plugin.LocalPosters.Tests;

public class SkiaSharpImageProcessorTests
{
    private readonly SkiaSharpImageProcessor _imageProcessor;
    private readonly ImageType _imageType;
    private readonly BaseItemKind _kind;
    private readonly FileInfo _source;

    public SkiaSharpImageProcessorTests()
    {
        _kind = BaseItemKind.Movie;
        _imageType = ImageType.Primary;
        _source = new FileInfo("abc.jpg");
        _imageProcessor = new SkiaSharpImageProcessor(SKColors.SkyBlue, NoopImageProcessor.Instance);
    }

    [Fact]
    public void Test()
    {
    }

    // [Fact]
    private void TestBorderReplacer()
    {
        var target = new FileInfo(_source.FullName.Replace(_source.Extension, "", StringComparison.OrdinalIgnoreCase) + "_border_replaced" +
                                  _source.Extension);

        using var stream = File.OpenRead(_source.FullName);
        using var image = _imageProcessor.Process(_kind, _imageType, stream);
        using var fileStream = new FileStream(target.FullName, FileMode.Create, FileAccess.Write);
        image.CopyTo(fileStream);
    }
}
