using Jellyfin.Plugin.LocalPosters.Utils;
using SkiaSharp;
using Xunit;

namespace Jellyfin.Plugin.LocalPosters.Tests;

public class SkiaSharpBorderRemoverTests
{
    private readonly FileInfo _source;
    private readonly SkiaSharpBorderRemover _borderReplacer;

    public SkiaSharpBorderRemoverTests()
    {
        _source = new FileInfo("abc.jpg");
        _borderReplacer = new SkiaSharpBorderRemover();
    }

    [Fact]
    public void Test()
    {
    }

    // [Fact]
    private void TestBorderRemover()
    {
        var target = new FileInfo(_source.FullName.Replace(_source.Extension, "", StringComparison.OrdinalIgnoreCase) + "_border_removed" +
                                  _source.Extension);

        using var image = _borderReplacer.Replace(_source.FullName);
        using var fileStream = new FileStream(target.FullName, FileMode.Create, FileAccess.Write);
        image.CopyTo(fileStream);
    }
}
