using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities.Movies;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <inheritdoc />
public partial class MovieMatcher : IMatcher
{
    private readonly string _name;
    private readonly int? _productionYear;
    private readonly int? _premiereYear;

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <param name="productionYear"></param>
    /// <param name="premiereYear"></param>
    public MovieMatcher(string name, int? productionYear, int? premiereYear)
    {
        _name = name;
        _productionYear = productionYear;
        _premiereYear = premiereYear;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="movie"></param>
    public MovieMatcher(Movie movie) : this(movie.Name, movie.ProductionYear, movie.PremiereDate?.Year)
    {
    }

    /// <inheritdoc />
    public bool IsMatch(string fileName)
    {
        var match = MovieRegex().Match(fileName);
        if (!match.Success) return false;

        if (!int.TryParse(match.Groups[2].Value, out var year)) return false;
        return (year == _productionYear || year == _premiereYear) &&
               _name.EqualsSanitizing(match.Groups[1].Value) || IsPartialMatch(_name, match.Groups[1].Value);
    }

    private static bool IsPartialMatch(string name, string fileName)
    {
        var split = name.Split(":", StringSplitOptions.RemoveEmptyEntries);
        return split.Length > 1 && split[0].EqualsSanitizing(fileName);
    }

    [GeneratedRegex(@"^(.*?)\s*\((\d{4})\)", RegexOptions.IgnoreCase)]
    private static partial Regex MovieRegex();
}
