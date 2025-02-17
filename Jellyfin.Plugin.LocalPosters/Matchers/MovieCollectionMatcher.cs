using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities.Movies;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <inheritdoc />
public partial class MovieCollectionMatcher : IMatcher
{
    private readonly string _name;

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    public MovieCollectionMatcher(string name)
    {
        _name = name;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="collection"></param>
    public MovieCollectionMatcher(BoxSet collection) : this(collection.Name)
    {
    }

    /// <inheritdoc />
    public bool IsMatch(string fileName)
    {
        var match = CollectionRegex().Match(fileName);
        return match.Success && _name.EqualsSanitizing(match.Groups[1].Value);
    }

    [GeneratedRegex(@"^(.*? Collection)(\.[a-z]+)?$", RegexOptions.IgnoreCase)]
    private static partial Regex CollectionRegex();
}
