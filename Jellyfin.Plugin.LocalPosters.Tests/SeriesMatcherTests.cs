using Jellyfin.Plugin.LocalPosters.Matchers;
using Xunit;

namespace Jellyfin.Plugin.LocalPosters.Tests;

public class SeriesMatcherTests
{
    [Fact]
    public void MatchSuccessfully()
    {
        const int SeriesYear = 2016;
        const string SeriesName = "Dexter: Original Sins";

        var matcher = new SeriesMatcher(SeriesName, SeriesName, SeriesYear);

        Assert.True(matcher.IsMatch("Dexter Original Sins (2016).jpg"));
    }

    [Fact]
    public void MatchSuccessfullyWithWhitespaceBeforeExtensions()
    {
        const int SeriesYear = 2016;
        const string SeriesName = "Dexter: Original Sins";

        var matcher = new SeriesMatcher(SeriesName, SeriesName, SeriesYear);

        Assert.True(matcher.IsMatch("Dexter Original Sins (2016) .jpg"));
    }


    [Fact]
    public void MatchContainingYearInNameSuccessfully()
    {
        const int SeriesYear = 2016;
        string seriesName = $"Dexter: Original Sins ({SeriesYear})";

        var matcher = new SeriesMatcher(seriesName, seriesName, SeriesYear);

        Assert.True(matcher.IsMatch("Dexter Original Sins (2016).jpg"));
    }


    [InlineData("-")]
    [InlineData(":")]
    [InlineData("")]
    [Theory]
    public void MatchSuccessfullyOnSymbols(string symbol)
    {
        const int SeriesYear = 2016;
        const string SeriesName = "Dexter: Original Sins";

        var matcher = new SeriesMatcher(SeriesName, SeriesName, SeriesYear);

        Assert.True(matcher.IsMatch($"Dexter{symbol} Original Sins (2016).jpg"));
    }
}
