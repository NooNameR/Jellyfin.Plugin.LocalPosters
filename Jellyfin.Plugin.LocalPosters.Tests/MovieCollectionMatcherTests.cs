using Jellyfin.Plugin.LocalPosters.Matchers;
using Xunit;

namespace Jellyfin.Plugin.LocalPosters.Tests;

public class MovieCollectionMatcherTests
{
    [Fact]
    public void MatchSuccessfully()
    {
        const string CollectionName = "Aquaman Collection";

        var matcher = new MovieCollectionMatcher(CollectionName);

        Assert.True(matcher.IsMatch("Aquaman Collection.jpg"));
    }
}
