using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <inheritdoc />
public partial class SeriesMatcher : IMatcher
{
    private readonly string _name;
    private readonly int? _productionYear;

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <param name="productionYear"></param>
    public SeriesMatcher(string name, int? productionYear)
    {
        _name = name;
        _productionYear = productionYear;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="series"></param>
    public SeriesMatcher(Series series) : this(series.Name, series.ProductionYear)
    {
    }

    /// <inheritdoc />
    public bool IsMatch(string fileName)
    {
        var match = SeasonRegex().Match(fileName);
        if (!match.Success) return false;

        if (!int.TryParse(match.Groups[2].Value, out var year)) return false;
        return (year == _productionYear) &&
               _name.EqualsSanitizing(match.Groups[1].Value);
    }

    [GeneratedRegex(@"^(.*?)\s*\((\d{4})\)(\.[a-z]+)?$", RegexOptions.IgnoreCase)]
    private static partial Regex SeasonRegex();
}
