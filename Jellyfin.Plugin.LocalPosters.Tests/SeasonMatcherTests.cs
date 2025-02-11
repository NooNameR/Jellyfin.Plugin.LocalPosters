using Jellyfin.Plugin.LocalPosters.Matchers;
using Xunit;

namespace Jellyfin.Plugin.LocalPosters.Tests;

public class SeasonMatcherTests
{
    [Fact]
    public void MatchSuccessfully()
    {
        const int SeasonYear = 2017;
        const int SeriesYear = 2016;
        const string SeriesName = "Dexter";
        const int SeasonNumber = 1;

        var matcher = new SeasonMatcher(SeriesName, SeriesYear, SeasonNumber, SeasonYear);

        Assert.True(matcher.IsMatch("Dexter (2016) - Season 1.jpg"));
    }

    [Fact]
    public void MatchContainingSeriesYearInNameSuccessfully()
    {
        const int SeasonYear = 2017;
        const int SeriesYear = 2016;
        string seriesName = $"Dexter ({SeriesYear})";
        const int SeasonNumber = 1;

        var matcher = new SeasonMatcher(seriesName, SeriesYear, SeasonNumber, SeasonYear);

        Assert.True(matcher.IsMatch("Dexter (2016) - Season 1.jpg"));
    }

    [Fact]
    public void MatchContainingSeasonYearInNameSuccessfully()
    {
        const int SeasonYear = 2017;
        const int SeriesYear = 2016;
        string seriesName = $"Dexter ({SeasonYear})";
        const int SeasonNumber = 1;

        var matcher = new SeasonMatcher(seriesName, SeriesYear, SeasonNumber, SeasonYear);

        Assert.True(matcher.IsMatch("Dexter (2016) - Season 1.jpg"));
    }

    [Fact]
    public void MatchSuccessfullyOnSeasonYear()
    {
        const int SeasonYear = 2016;
        const int SeriesYear = 2017;
        const string SeriesName = "Dexter";
        const int SeasonNumber = 1;


        var matcher = new SeasonMatcher(SeriesName, SeriesYear, SeasonNumber, SeasonYear);

        Assert.True(matcher.IsMatch("Dexter (2016) - Season 1.jpg"));
    }

    [InlineData("-")]
    [InlineData(":")]
    [InlineData("")]
    [Theory]
    public void MatchSuccessfullyOnSymbols(string symbol)
    {
        const int SeasonYear = 2017;
        const int SeriesYear = 2016;
        const string SeriesName = "Dexter: New Blood";
        const int SeasonNumber = 1;


        var matcher = new SeasonMatcher(SeriesName, SeriesYear, SeasonNumber, SeasonYear);

        Assert.True(matcher.IsMatch($"Dexter{symbol} new Blood (2016) - Season 1.jpg"));
    }
}
