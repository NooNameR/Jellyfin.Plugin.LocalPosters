using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities.TV;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <inheritdoc />
public class SeasonMatcher : RegexMatcher
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="seriesName"></param>
    /// <param name="seriesProductionYear"></param>
    /// <param name="seasonName"></param>
    /// <param name="seasonProductionYear"></param>
    public SeasonMatcher(string seriesName, int? seriesProductionYear, string seasonName,
        int? seasonProductionYear) : base(
        Regexes(seriesName, seriesProductionYear, seasonName, seasonProductionYear))
    {
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="season"></param>
    public SeasonMatcher(Season season) : this(season.SeriesName, season.Series.ProductionYear, season.Name,
        season.ProductionYear)
    {
    }

    static IEnumerable<Regex> Regexes(string seriesName, int? seriesProductionYear, string seasonName,
        int? seasonProductionYear)
    {
        var name = SanitizedName(seriesName, seriesProductionYear, seasonProductionYear);

        foreach (var year in new HashSet<int?> { seasonProductionYear, seriesProductionYear }.OfType<int?>())
        {
            yield return
                new Regex($@"^{name} \({year}\)\s?[-\u2013]?\s?{seasonName}(\.[a-z]+)?$",
                    RegexOptions.IgnoreCase);
            yield return new Regex(
                $@"^{name.Replace(":", @"([:_\-\u2013])?", StringComparison.OrdinalIgnoreCase)} \({year}\)\s?[-\u2013]?\s?{seasonName}(\.[a-z]+)?$",
                RegexOptions.IgnoreCase);
        }
    }

    private static string SanitizedName(string seriesName, int? seriesProductionYear, int? seasonProductionYear)
    {
        return
            seriesName.Replace($" ({seriesProductionYear})", "", StringComparison.OrdinalIgnoreCase)
                .Replace($" ({seasonProductionYear})", "", StringComparison.OrdinalIgnoreCase)
                .Replace("–", "-", StringComparison.OrdinalIgnoreCase)
                .Replace("–", @"[-\u2013]", StringComparison.OrdinalIgnoreCase);
    }
}
