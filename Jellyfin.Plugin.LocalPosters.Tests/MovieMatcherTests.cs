using Jellyfin.Plugin.LocalPosters.Matchers;
using Xunit;

namespace Jellyfin.Plugin.LocalPosters.Tests;

public class MovieMatcherTests
{
    [Fact]
    public void MatchSuccessfully()
    {
        const int MovieYear = 2016;
        const int PremiereYear = 2016;
        const string MovieName = "Dune: Part One";

        var matcher = new MovieMatcher(MovieName, MovieName, MovieYear, PremiereYear);

        Assert.True(matcher.IsMatch("Dune: Part One (2016).jpg"));
    }

    [Fact]
    public void MatchSuccessfullyWithProviderId()
    {
        const int MovieYear = 2016;
        const int PremiereYear = 2016;
        const string MovieName = "Dune: Part One";

        var matcher = new MovieMatcher(MovieName, MovieName, MovieYear, PremiereYear);

        Assert.True(matcher.IsMatch("Dune: Part One (2016) {tmdb-random} {imdb-random}.jpg"));
    }

    [Fact]
    public void MatchSuccessfullyWithWhitespaceBeforeExtensions()
    {
        const int MovieYear = 2016;
        const int PremiereYear = 2016;
        const string MovieName = "Dune: Part One";

        var matcher = new MovieMatcher(MovieName, MovieName, MovieYear, PremiereYear);

        Assert.True(matcher.IsMatch("Dune: Part One (2016) .jpg"));
    }


    [Fact]
    public void MatchSuccessfullyWithOddCharacters()
    {
        const int MovieYear = 2019;
        const int PremiereYear = 2019;
        const string MovieName = "Pokémon Detective Pikachu (2019)";

        var matcher = new MovieMatcher(MovieName, MovieName, MovieYear, PremiereYear);

        Assert.True(matcher.IsMatch("Pokemon detective pikachu (2019).jpg"));
    }

    [Fact]
    public void MatchSuccessfullyOnPremiere()
    {
        const int MovieYear = 2016;
        const int PremiereYear = 2017;
        const string MovieName = "Dune: Part One";

        var matcher = new MovieMatcher(MovieName, MovieName, MovieYear, PremiereYear);

        Assert.True(matcher.IsMatch("Dune: Part One (2017).jpg"));
    }

    [Fact]
    public void MatchContainingYearInNameSuccessfully()
    {
        const int MovieYear = 2016;
        const int PremiereYear = 2016;
        string movieName = $"Dune: Part One ({MovieYear})";

        var matcher = new MovieMatcher(movieName, movieName, MovieYear, PremiereYear);

        Assert.True(matcher.IsMatch("Dune: Part One (2016).jpg"));
    }

    [Fact]
    public void MatchContainingPremiereYearInNameSuccessfully()
    {
        const int MovieYear = 2016;
        const int PremiereYear = 2018;
        string movieName = $"Dune: Part One ({PremiereYear})";

        var matcher = new MovieMatcher(movieName, movieName, MovieYear, PremiereYear);

        Assert.True(matcher.IsMatch("Dune: Part One (2016).jpg"));
    }

    [Fact]
    public void MatchSuccessfullyOnSimpleName()
    {
        const int MovieYear = 2016;
        const int PremiereYear = 2016;
        const string MovieName = "Dune: Part One";

        var matcher = new MovieMatcher(MovieName, MovieName, MovieYear, PremiereYear);

        Assert.True(matcher.IsMatch("Dune (2016).jpg"));
    }


    [InlineData("-")]
    [InlineData(":")]
    [InlineData("")]
    [Theory]
    public void MatchSuccessfullyOnSymbols(string symbol)
    {
        const int MovieYear = 2016;
        const int PremiereYear = 2016;
        const string MovieName = "Dune: Part One";

        var matcher = new MovieMatcher(MovieName, MovieName, MovieYear, PremiereYear);

        Assert.True(matcher.IsMatch($"Dune{symbol} Part One (2016).jpg"));
    }
}
