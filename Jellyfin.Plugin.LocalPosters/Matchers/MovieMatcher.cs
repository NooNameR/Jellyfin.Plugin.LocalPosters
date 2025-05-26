using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities.Movies;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <inheritdoc />
public partial class MovieMatcher : IMatcher
{
    private readonly HashSet<string> _names;
    private readonly int? _premiereYear;
    private readonly int? _productionYear;

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <param name="originalName"></param>
    /// <param name="productionYear"></param>
    /// <param name="premiereYear"></param>
    public MovieMatcher(string name, string originalName, int? productionYear, int? premiereYear)
    {
        var titles = new[] { name, originalName }.Where(x => !string.IsNullOrWhiteSpace(x)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var title in titles.ToList())
            titles.Add(title.Split(":", StringSplitOptions.RemoveEmptyEntries)[0]);
        SearchPatterns = titles.Select(x => $"{x.SanitizeName("*")}*.*".Replace("**", "*", StringComparison.Ordinal))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _names = titles.Select(x => x.SanitizeName()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        _productionYear = productionYear;
        _premiereYear = premiereYear;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="movie"></param>
    public MovieMatcher(Movie movie) : this(movie.Name, movie.OriginalTitle, movie.ProductionYear, movie.PremiereDate?.Year)
    {
    }

    /// <inheritdoc />
    public IReadOnlySet<string> SearchPatterns { get; }

    /// <inheritdoc />
    public bool IsMatch(string fileName)
    {
        var match = MovieRegex().Match(fileName);
        if (!match.Success) return false;

        if (!int.TryParse(match.Groups[2].Value, out var year)) return false;
        var name = match.Groups[1].Value.SanitizeName();
        return (year == _productionYear || year == _premiereYear) && _names.Contains(name);
    }

    [GeneratedRegex(@"^(.*?)\s*\((\d{4})\)(?:\s*\{[^}]+\})*\s*(\.[a-z]{3,})$", RegexOptions.IgnoreCase)]
    private static partial Regex MovieRegex();
}
