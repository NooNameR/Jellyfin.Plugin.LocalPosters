using Jellyfin.Data.Enums;
using Jellyfin.Plugin.LocalPosters.Utils;
using MediaBrowser.Model.Entities;
using Xunit;

namespace Jellyfin.Plugin.LocalPosters.Tests;

public class SkiaSharpBorderRemoverTests
{
    private readonly SkiaSharpBorderRemover _borderReplacer;
    private readonly ImageType _imageType;
    private readonly BaseItemKind _kind;
    private readonly FileInfo _source;

    public SkiaSharpBorderRemoverTests()
    {
        _kind = BaseItemKind.Movie;
        _imageType = ImageType.Primary;
        _source = new FileInfo("abc.jpg");
        _borderReplacer = new SkiaSharpBorderRemover(NoopImageProcessor.Instance);
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

        using var stream = File.OpenRead(_source.FullName);
        using var image = _borderReplacer.Process(_kind, _imageType, stream);
        using var fileStream = new FileStream(target.FullName, FileMode.Create, FileAccess.Write);
        image.CopyTo(fileStream);
    }
}
