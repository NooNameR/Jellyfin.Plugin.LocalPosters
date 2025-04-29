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
        const string SeasonName = "Season 1";
        const int SeasonIndex = 1;
        var matcher = new SeasonMatcher(SeriesName, SeriesName, SeriesYear, SeasonName, SeasonIndex, SeasonYear);

        Assert.True(matcher.IsMatch("Dexter (2016) - Season 1.jpg"));
    }

    [Fact]
    public void MatchSuccessfullyWithWhitespaceBeforeExtensions()
    {
        const int SeasonYear = 2017;
        const int SeriesYear = 2016;
        const string SeriesName = "Dexter";
        const string SeasonName = "Season 1";
        const int SeasonIndex = 1;
        var matcher = new SeasonMatcher(SeriesName, SeriesName, SeriesYear, SeasonName, SeasonIndex, SeasonYear);

        Assert.True(matcher.IsMatch("Dexter (2016) - Season 1 .jpg"));
    }

    [Fact]
    public void DoesNotMatchWhenSeasonIsDifferent()
    {
        const int SeasonYear = 2017;
        const int SeriesYear = 2016;
        const string SeriesName = "Dexter";
        const string SeasonName = "Season 2";
        const int SeasonIndex = 2;
        var matcher = new SeasonMatcher(SeriesName, SeriesName, SeriesYear, SeasonName, SeasonIndex, SeasonYear);

        Assert.False(matcher.IsMatch("Dexter (2016) - Season 1.jpg"));
    }

    [Fact]
    public void MatchSuccessfullyOnIndex()
    {
        const int SeasonYear = 2017;
        const int SeriesYear = 2016;
        const string SeriesName = "Dexter";
        const string SeasonName = "SomeRandomSeason";
        const int SeasonIndex = 1;
        var matcher = new SeasonMatcher(SeriesName, SeriesName, SeriesYear, SeasonName, SeasonIndex, SeasonYear);

        Assert.True(matcher.IsMatch("Dexter (2016) - Season 1.jpg"));
    }

    [Fact]
    public void MatchSuccessfullyOnSeasonName()
    {
        const int SeasonYear = 2017;
        const int SeriesYear = 2016;
        const string SeriesName = "Dexter";
        const string SeasonName = "Season 1";
        const int SeasonIndex = 2;
        var matcher = new SeasonMatcher(SeriesName, SeriesName, SeriesYear, SeasonName, SeasonIndex, SeasonYear);

        Assert.True(matcher.IsMatch("Dexter (2016) - Season 1.jpg"));
    }

    [Fact]
    public void MatchContainingSeriesYearInNameSuccessfully()
    {
        const int SeasonYear = 2017;
        const int SeriesYear = 2016;
        const string SeasonName = "Season 1";
        const int SeasonIndex = 1;
        string seriesName = $"Dexter ({SeriesYear})";

        var matcher = new SeasonMatcher(seriesName, seriesName, SeriesYear, SeasonName, SeasonIndex, SeasonYear);

        Assert.True(matcher.IsMatch("Dexter (2016) - Season 1.jpg"));
    }

    [Fact]
    public void MatchContainingSeasonYearInNameSuccessfully()
    {
        const int SeasonYear = 2017;
        const int SeriesYear = 2016;
        string seriesName = $"Dexter ({SeasonYear})";
        const string SeasonName = "Season 1";
        const int SeasonIndex = 1;
        var matcher = new SeasonMatcher(seriesName, seriesName, SeriesYear, SeasonName, SeasonIndex, SeasonYear);

        Assert.True(matcher.IsMatch("Dexter (2016) - Season 1.jpg"));
    }

    [Fact]
    public void MatchSuccessfullyOnSeasonYear()
    {
        const int SeasonYear = 2016;
        const int SeriesYear = 2017;
        const int SeasonIndex = 1;
        const string SeasonName = "Season 1";
        const string SeriesName = "Dexter";

        var matcher = new SeasonMatcher(SeriesName, SeriesName, SeriesYear, SeasonName, SeasonIndex, SeasonYear);

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
        const int SeasonIndex = 1;
        const string SeasonName = "Season 1";
        const string SeriesName = "Dexter: New Blood";

        var matcher = new SeasonMatcher(SeriesName, SeriesName, SeriesYear, SeasonName, SeasonIndex, SeasonYear);

        Assert.True(matcher.IsMatch($"Dexter{symbol} new Blood (2016) - Season 1.jpg"));
    }
}
