using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <inheritdoc />
public class SeriesMatcher : RegexMatcher
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <param name="productionYear"></param>
    public SeriesMatcher(string name, int? productionYear) : base(
        Regexes(name, productionYear))
    {
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="series"></param>
    public SeriesMatcher(Series series) : this(series.Name, series.ProductionYear)
    {
    }

    static IEnumerable<Regex> Regexes(string name, int? productionYear)
    {
        var sanitizedName = name.Replace($" ({productionYear})", "", StringComparison.OrdinalIgnoreCase)
            .Replace("–", "-", StringComparison.OrdinalIgnoreCase)
            .Replace("–", @"[-\u2013]", StringComparison.OrdinalIgnoreCase);

        yield return
            new Regex($@"^{sanitizedName} \({productionYear}\)(\.[a-z]+)?$", RegexOptions.IgnoreCase);

        yield return new Regex(
            $@"^{sanitizedName.Replace(":", @"([:_\-\u2013])?", StringComparison.OrdinalIgnoreCase)} \({productionYear}\)(\.[a-z]+)?$",
            RegexOptions.IgnoreCase);
    }
}
