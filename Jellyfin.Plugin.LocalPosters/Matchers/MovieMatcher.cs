using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities.Movies;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <inheritdoc />
public partial class MovieMatcher : IMatcher
{
    private readonly string _name;
    private readonly int? _premiereYear;
    private readonly int? _productionYear;
    private readonly string _splitName;

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <param name="productionYear"></param>
    /// <param name="premiereYear"></param>
    public MovieMatcher(string name, int? productionYear, int? premiereYear)
    {
        _splitName = name.Split(":", StringSplitOptions.RemoveEmptyEntries)[0];
        SearchPattern = $"{_splitName.SanitizeName("*")}*.*".Replace("**", "*", StringComparison.Ordinal);
        _name = name.SanitizeName();
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
    public string SearchPattern { get; }

    /// <inheritdoc />
    public bool IsMatch(string fileName)
    {
        var match = MovieRegex().Match(fileName);
        if (!match.Success) return false;

        if (!int.TryParse(match.Groups[2].Value, out var year)) return false;
        var name = match.Groups[1].Value.SanitizeName();
        return (year == _productionYear || year == _premiereYear) &&
               (string.Equals(_name, name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(_splitName, name, StringComparison.OrdinalIgnoreCase));
    }

    [GeneratedRegex(@"^(.*?)\s*\((\d{4})\)\s*(\.[a-z]{3,})$", RegexOptions.IgnoreCase)]
    private static partial Regex MovieRegex();
}
