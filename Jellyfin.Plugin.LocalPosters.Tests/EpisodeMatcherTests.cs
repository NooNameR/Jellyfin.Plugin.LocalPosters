using Jellyfin.Plugin.LocalPosters.Matchers;
using Xunit;

namespace Jellyfin.Plugin.LocalPosters.Tests;

public class EpisodeMatcherTests
{
    [Fact]
    public void MatchSuccessfully()
    {
        const int SeasonYear = 2017;
        const int SeriesYear = 2016;
        const string SeriesName = "Dexter";
        const int SeasonIndex = 1;
        const int EpisodeIndex = 2;
        const int EpisodeYear = 2018;
        var matcher = new EpisodeMatcher(SeriesName, SeriesYear, SeasonIndex, SeasonYear, EpisodeIndex, EpisodeYear);

        Assert.True(matcher.IsMatch("Dexter (2016) - S1 E2.jpg"));
    }

    [Fact]
    public void MatchSuccessfullyForDoubleDigit()
    {
        const int SeasonYear = 2017;
        const int SeriesYear = 2016;
        const string SeriesName = "Dexter";
        const int SeasonIndex = 1;
        const int EpisodeIndex = 12;
        const int EpisodeYear = 2018;
        var matcher = new EpisodeMatcher(SeriesName, SeriesYear, SeasonIndex, SeasonYear, EpisodeIndex, EpisodeYear);

        Assert.True(matcher.IsMatch("Dexter (2016) - S01 E12.jpg"));
    }

    [Fact]
    public void MatchContainingSeriesYearInNameSuccessfully()
    {
        const int SeasonYear = 2017;
        const int SeriesYear = 2016;
        const int SeasonIndex = 1;
        const int EpisodeIndex = 12;
        const int EpisodeYear = 2018;
        string seriesName = $"Dexter ({SeriesYear})";

        var matcher = new EpisodeMatcher(seriesName, SeriesYear, SeasonIndex, SeasonYear, EpisodeIndex, EpisodeYear);

        Assert.True(matcher.IsMatch("Dexter (2016) - S01 E12.jpg"));
    }

    [Fact]
    public void MatchContainingSeasonYearInNameSuccessfully()
    {
        const int SeasonYear = 2017;
        const int SeriesYear = 2016;
        const int SeasonIndex = 1;
        const int EpisodeIndex = 12;
        const int EpisodeYear = 2018;
        string seriesName = $"Dexter ({SeasonYear})";

        var matcher = new EpisodeMatcher(seriesName, SeriesYear, SeasonIndex, SeasonYear, EpisodeIndex, EpisodeYear);

        Assert.True(matcher.IsMatch("Dexter (2016) - S01 E12.jpg"));
    }

    [Fact]
    public void MatchSuccessfullyOnEpisodeYear()
    {
        const int SeasonYear = 2017;
        const int SeriesYear = 2016;
        const int SeasonIndex = 1;
        const int EpisodeIndex = 12;
        const int EpisodeYear = 2018;
        const string SeriesName = "Dexter";

        var matcher = new EpisodeMatcher(SeriesName, SeriesYear, SeasonIndex, SeasonYear, EpisodeIndex, EpisodeYear);

        Assert.True(matcher.IsMatch("Dexter (2018) - S01 E12.jpg"));
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
        const int EpisodeIndex = 12;
        const int EpisodeYear = 2018;
        const string SeriesName = "Dexter: New Blood";

        var matcher = new EpisodeMatcher(SeriesName, SeriesYear, SeasonIndex, SeasonYear, EpisodeIndex, EpisodeYear);

        Assert.True(matcher.IsMatch($"Dexter{symbol} new Blood (2016) - S1 E12.jpg"));
    }
}
