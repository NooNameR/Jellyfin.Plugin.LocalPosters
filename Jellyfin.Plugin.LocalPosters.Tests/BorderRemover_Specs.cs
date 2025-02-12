using Jellyfin.Plugin.LocalPosters.Utils;
using SkiaSharp;

namespace Jellyfin.Plugin.LocalPosters.Tests;

public class BorderRemoverTests
{
    private readonly FileInfo _source;
    private readonly SkiaSharpBorderRemover _borderReplacer;

    public BorderRemoverTests()
    {
        _source = new FileInfo("");
        _borderReplacer = new SkiaSharpBorderRemover();
    }

    // [Fact]
    public void TestBorderRemover()
    {
        var target = new FileInfo(_source.FullName.Replace(_source.Extension, "", StringComparison.OrdinalIgnoreCase) + "_border_removed" +
                                  _source.Extension);

        using var image = _borderReplacer.Replace(_source.FullName);
        using var fileStream = new FileStream(target.FullName, FileMode.Create, FileAccess.Write);
        image.CopyTo(fileStream);
    }
}
