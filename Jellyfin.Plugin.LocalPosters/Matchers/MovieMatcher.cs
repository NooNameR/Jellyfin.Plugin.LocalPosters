using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities.Movies;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <inheritdoc />
public partial class MovieMatcher : IMatcher
{
    private readonly string _name;
    private readonly int? _premiereYear;
    private readonly int? _productionYear;
    private readonly string[] _splitName;

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <param name="productionYear"></param>
    /// <param name="premiereYear"></param>
    public MovieMatcher(string name, int? productionYear, int? premiereYear)
    {
        _name = name.SanitizeName();
        _splitName = name.Split(":", StringSplitOptions.RemoveEmptyEntries);
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
        var name = match.Groups[1].Value.SanitizeName();
        return (year == _productionYear || year == _premiereYear) &&
               (string.Equals(_name, name, StringComparison.OrdinalIgnoreCase) ||
                IsPartialMatch(_splitName, name));
    }

    private static bool IsPartialMatch(string[] split, string fileName)
    {
        return split.Length > 1 && string.Equals(split[0], fileName, StringComparison.OrdinalIgnoreCase);
    }

    [GeneratedRegex(@"^(.*?)\s*\((\d{4})\)(\.[a-z]+)?$", RegexOptions.IgnoreCase)]
    private static partial Regex MovieRegex();
}
