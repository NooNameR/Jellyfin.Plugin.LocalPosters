using Jellyfin.Plugin.LocalPosters.Matchers;
using Xunit;

namespace Jellyfin.Plugin.LocalPosters.Tests;

public class MovieCollectionMatcherTests
{
    [Fact]
    public void MatchSuccessfully()
    {
        const string CollectionName = "Aquaman Collection";

        var matcher = new MovieCollectionMatcher(CollectionName, CollectionName);

        Assert.True(matcher.IsMatch("Aquaman Collection.jpg"));
    }


    [Fact]
    public void SearchPatternMatchSuccessfully()
    {
        const string FileName = "Aquaman Collection.jpg";
        var folder = Path.GetTempPath();
        var filePath = Path.Combine(folder, FileName);
        File.Create(filePath).Dispose();
        try
        {
            const string CollectionName = "Aquaman Collection";

            var matcher = new MovieCollectionMatcher(CollectionName, CollectionName);

            foreach (var searchPattern in matcher.SearchPatterns)
            {
                foreach (var file in new DirectoryInfo(folder).EnumerateFiles(searchPattern,
                             new EnumerationOptions
                             {
                                 RecurseSubdirectories = true,
                                 IgnoreInaccessible = true,
                                 MatchCasing = MatchCasing.CaseInsensitive,
                                 AttributesToSkip = FileAttributes.Hidden | FileAttributes.System | FileAttributes.Temporary
                             }))
                {
                    Assert.True(matcher.IsMatch(file.Name));
                }
            }
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void MatchSuccessfullyWithTmdb()
    {
        const string CollectionName = "Aquaman Collection";

        var matcher = new MovieCollectionMatcher(CollectionName, CollectionName);

        Assert.True(matcher.IsMatch("Aquaman Collection {tmdb-random}.jpg"));
    }

    [Fact]
    public void MatchSuccessfullyWithWhitespaceBeforeExtensions()
    {
        const string CollectionName = "Aquaman Collection";

        var matcher = new MovieCollectionMatcher(CollectionName, CollectionName);

        Assert.True(matcher.IsMatch("Aquaman Collection .jpg"));
    }
}
