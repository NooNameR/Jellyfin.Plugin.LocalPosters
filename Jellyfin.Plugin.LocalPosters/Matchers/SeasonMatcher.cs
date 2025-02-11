using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities.TV;

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
        var sanitizedSeriesName =
            seriesName.Replace($" ({seriesProductionYear})", "", StringComparison.OrdinalIgnoreCase)
                .Replace($" ({seasonProductionYear})", "", StringComparison.OrdinalIgnoreCase)
                .Replace("–", "-", StringComparison.OrdinalIgnoreCase)
                .Replace("–", @"[-\u2013]", StringComparison.OrdinalIgnoreCase);

        yield return
            new Regex($@"^{sanitizedSeriesName} \({seasonProductionYear}\)\s?[-\u2013]?\s?{seasonName}(\.[a-z]+)?$",
                RegexOptions.IgnoreCase);
        yield return new Regex(
            $@"^{sanitizedSeriesName} \({seriesProductionYear}\)\s?[-\u2013]?\s?{seasonName}(\.[a-z]+)?$",
            RegexOptions.IgnoreCase);

        yield return new Regex(
            $@"^{sanitizedSeriesName.Replace(":", @"([:_\-\u2013])?", StringComparison.OrdinalIgnoreCase)} \({seasonProductionYear}\)\s?[-\u2013]?\s?{seasonName}(\.[a-z]+)?$",
            RegexOptions.IgnoreCase);

        yield return new Regex(
            $@"^{sanitizedSeriesName.Replace(":", @"([:_\-\u2013])?", StringComparison.OrdinalIgnoreCase)} \({seriesProductionYear}\)\s?[-\u2013]?\s?{seasonName}(\.[a-z]+)?$",
            RegexOptions.IgnoreCase);
    }
}
