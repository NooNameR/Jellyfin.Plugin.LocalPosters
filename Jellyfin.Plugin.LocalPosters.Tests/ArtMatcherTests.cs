using Jellyfin.Plugin.LocalPosters.Matchers;
using MediaBrowser.Model.Entities;
using Xunit;

namespace Jellyfin.Plugin.LocalPosters.Tests;

public class ArtMatcherTests
{
    [Fact]
    public void MatchSuccessfully()
    {
        const int SeriesYear = 2025;
        const string SeriesName = "Daredevil: Born Again";
        var matcher = new ArtMatcher(SeriesName, SeriesYear, ImageType.Backdrop);

        Assert.True(matcher.IsMatch("Daredevil- Born Again (2025) - Backdrop.jpg"));
    }
}
