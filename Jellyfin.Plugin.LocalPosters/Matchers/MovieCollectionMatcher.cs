using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities.Movies;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <inheritdoc />
public partial class MovieCollectionMatcher : IMatcher
{
    private readonly HashSet<string> _names;

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <param name="originalName"></param>
    public MovieCollectionMatcher(string name, string originalName)
    {
        var titles = new[] { name, originalName }.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        SearchPatterns = titles.Select(x => $"{x.SanitizeName("*")}*Collection*.*".Replace("**", "*", StringComparison.Ordinal))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _names = titles.Select(x => x.SanitizeName()).ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="collection"></param>
    public MovieCollectionMatcher(BoxSet collection) : this(collection.Name, collection.OriginalTitle)
    {
    }

    /// <inheritdoc />
    public IReadOnlySet<string> SearchPatterns { get; }

    /// <inheritdoc />
    public bool IsMatch(string fileName)
    {
        if (_names.Count == 0)
            return false;

        var match = CollectionRegex().Match(fileName);
        var name = match.Groups[1].Value.SanitizeName();
        return match.Success && _names.Contains(name);
    }

    [GeneratedRegex(@"^(.*? Collection)(?:\s*\{[^}]+\})*\s*(\.[a-z]{3,})$", RegexOptions.IgnoreCase)]
    private static partial Regex CollectionRegex();
}
