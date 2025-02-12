using Jellyfin.Plugin.LocalPosters.Utils;
using SkiaSharp;
using Xunit;

namespace Jellyfin.Plugin.LocalPosters.Tests;

public class SkiaSharpBorderReplacerTests
{
    private readonly FileInfo _source;
    private readonly SkiaSharpBorderReplacer _borderReplacer;

    public SkiaSharpBorderReplacerTests()
    {
        _source = new FileInfo("abc.jpg");
        _borderReplacer = new SkiaSharpBorderReplacer(SKColors.SkyBlue);
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

        using var image = _borderReplacer.Replace(_source.FullName);
        using var fileStream = new FileStream(target.FullName, FileMode.Create, FileAccess.Write);
        image.CopyTo(fileStream);
    }
}
