using Jellyfin.Plugin.LocalPosters.Matchers;
using MediaBrowser.Model.Entities;
using Xunit;

namespace Jellyfin.Plugin.LocalPosters.Tests;

public class ArtMatcherTests
{
    private readonly DirectoryInfo _folder;

    public ArtMatcherTests()
    {
        var folder = "art-matchers";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        _folder = new DirectoryInfo(folder);
    }

    [Fact]
    public void MatchSuccessfully()
    {
        const int SeriesYear = 2025;
        const string SeriesName = "Daredevil: Born Again";
        var matcher = new ArtMatcher(SeriesName, SeriesYear, ImageType.Backdrop);

        Assert.True(matcher.IsMatch("Daredevil- Born Again (2025) - Backdrop.jpg"));
    }

    [Fact]
    public async Task SearchPatternWorksWell()
    {
        const int Year = 2013;
        const string MovieName = "2 Guns";

        var expectedFileName = "2 Guns (2013) - Backdrop.jpg";

        var matcher = new ArtMatcher(MovieName, Year, ImageType.Backdrop);

        var filePath = Path.Combine(_folder.FullName, expectedFileName);
        await File.Create(filePath).DisposeAsync();

        var files = new DirectoryInfo(_folder.FullName).EnumerateFiles(matcher.SearchPattern).ToArray();

        Assert.Single(files);

        Assert.Equal(filePath, files[0].FullName);

        File.Delete(filePath);
    }
}
