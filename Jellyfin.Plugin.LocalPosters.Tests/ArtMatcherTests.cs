using Jellyfin.Plugin.LocalPosters.Matchers;
using MediaBrowser.Model.Entities;
using Xunit;

namespace Jellyfin.Plugin.LocalPosters.Tests;

public sealed class ArtMatcherTests : IDisposable
{
    const string Folder = "art-matchers";

    private readonly DirectoryInfo _folder;

    public ArtMatcherTests()
    {
        if (!Directory.Exists(Folder))
            Directory.CreateDirectory(Folder);

        _folder = new DirectoryInfo(Folder);
    }

#pragma warning disable CA1063
#pragma warning disable CA1816
    public void Dispose()
#pragma warning restore CA1816
#pragma warning restore CA1063
    {
        Directory.Delete(_folder.FullName, true);
    }

    [Fact]
    public void MatchSuccessfully()
    {
        const int SeriesYear = 2025;
        const string SeriesName = "Daredevil: Born Again";
        const string OriginalName = "Daredevil: Born Again";
        var matcher = new ArtMatcher(SeriesName, OriginalName, SeriesYear, ImageType.Backdrop);

        Assert.True(matcher.IsMatch("Daredevil- Born Again (2025) - Backdrop.jpg"));
    }

    [Fact]
    public void MatchSuccessfullyWithoutYear()
    {
        const string SeriesName = "Daredevil: Born Again";
        const string OriginalName = "Daredevil: Born Again";
        var matcher = new ArtMatcher(SeriesName, OriginalName, null, ImageType.Backdrop);

        Assert.True(matcher.IsMatch("Daredevil- Born Again - Backdrop.jpg"));
    }

    [Fact]
    public void MatchSuccessfullyWith0Year()
    {
        const string SeriesName = "Daredevil: Born Again";
        const string OriginalName = "Daredevil: Born Again";
        var matcher = new ArtMatcher(SeriesName, OriginalName, 0, ImageType.Backdrop);

        Assert.True(matcher.IsMatch("Daredevil- Born Again - Backdrop.jpg"));
    }

    [Fact]
    public void MatchSuccessfullyWithProviderId()
    {
        const int SeriesYear = 2025;
        const string SeriesName = "Daredevil: Born Again";
        const string OriginalName = "Daredevil: Born Again";
        var matcher = new ArtMatcher(SeriesName, OriginalName, SeriesYear, ImageType.Backdrop);

        Assert.True(matcher.IsMatch("Daredevil- Born Again (2025) {tmdb-random} {imdb-random} - Backdrop.jpg"));
    }

    [Fact]
    public async Task SearchPatternWorksWell()
    {
        const int Year = 2013;
        const string MovieName = "2 Guns";
        const string OriginalName = "2 Guns";

        var expectedFileName = "2 Guns (2013) - Backdrop.jpg";

        var matcher = new ArtMatcher(MovieName, OriginalName, Year, ImageType.Backdrop);

        var filePath = Path.Combine(_folder.FullName, expectedFileName);
        await File.Create(filePath).DisposeAsync();

        foreach (var searchPattern in matcher.SearchPatterns)
        {
            var files = _folder.EnumerateFiles(searchPattern).ToArray();

            Assert.Single(files);

            Assert.Equal(filePath, files[0].FullName);

            return;
        }

        Assert.Fail();
    }

    [Fact]
    public async Task SearchPatternWorksWellWithoutYear()
    {
        const string MovieName = "2 Guns";
        const string OriginalName = "2 Guns";

        var expectedFileName = "2 Guns - Backdrop.jpg";

        var matcher = new ArtMatcher(MovieName, OriginalName, null, ImageType.Backdrop);

        var filePath = Path.Combine(_folder.FullName, expectedFileName);
        await File.Create(filePath).DisposeAsync();

        foreach (var searchPattern in matcher.SearchPatterns)
        {
            var files = _folder.EnumerateFiles(searchPattern).ToArray();

            Assert.Single(files);

            Assert.Equal(filePath, files[0].FullName);

            return;
        }

        Assert.Fail();
    }
}
