using Jellyfin.Plugin.LocalPosters.Utils;
using SkiaSharp;

namespace Jellyfin.Plugin.LocalPosters.Tests;

public class SkiaSharpBorderReplacerTests
{
    private readonly FileInfo _source;

    public SkiaSharpBorderReplacerTests()
    {
        _source = new FileInfo("");
    }

    // [Fact]
    public void TestBorderRemover()
    {
        var target = new FileInfo(_source.FullName.Replace(_source.Extension, "", StringComparison.OrdinalIgnoreCase) + "_border_removed" +
                                  _source.Extension);
        var borderReplacer = new SkiaSharpBorderReplacer();

        using var image = borderReplacer.RemoveBorder(_source.FullName);
        using var fileStream = new FileStream(target.FullName, FileMode.Create, FileAccess.Write);
        image.CopyTo(fileStream);
    }

    // [Fact]
    public void TestBorderReplacer()
    {
        var target = new FileInfo(_source.FullName.Replace(_source.Extension, "", StringComparison.OrdinalIgnoreCase) + "_border_replaced" +
                                  _source.Extension);
        var borderReplacer = new SkiaSharpBorderReplacer();

        using var image = borderReplacer.ReplaceBorder(_source.FullName, SKColors.SkyBlue);
        using var fileStream = new FileStream(target.FullName, FileMode.Create, FileAccess.Write);
        image.CopyTo(fileStream);
    }
}
