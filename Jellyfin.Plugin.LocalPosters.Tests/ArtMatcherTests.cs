using Jellyfin.Plugin.LocalPosters.Matchers;
using MediaBrowser.Model.Entities;
using Xunit;

namespace Jellyfin.Plugin.LocalPosters.Tests;

public class ArtMatcherTests
{
    [Fact]
    public void MatchSuccessfully()
    {
        const int SeriesYear = 2016;
        const string SeriesName = "Dexter";
        var matcher = new ArtMatcher(SeriesName, SeriesYear, ImageType.Backdrop);

        Assert.True(matcher.IsMatch("Dexter (2016) - Backdrop.jpg"));
    }
}
