using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities.TV;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <inheritdoc />
public partial class SeasonMatcher : IMatcher
{
    private readonly string _seasonName;
    private readonly int? _seasonProductionYear;
    private readonly string _seriesName;
    private readonly int? _seriesProductionYear;

    /// <summary>
    ///
    /// </summary>
    /// <param name="seriesName"></param>
    /// <param name="seriesProductionYear"></param>
    /// <param name="seasonName"></param>
    /// <param name="seasonProductionYear"></param>
    public SeasonMatcher(string seriesName, int? seriesProductionYear, string seasonName,
        int? seasonProductionYear)
    {
        _seriesName = seriesName.SanitizeName();
        _seasonName = seasonName.SanitizeName();
        _seriesProductionYear = seriesProductionYear;
        _seasonProductionYear = seasonProductionYear;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="season"></param>
    public SeasonMatcher(Season season) : this(season.SeriesName, season.Series.ProductionYear, season.Name,
        season.ProductionYear)
    {
    }

    /// <inheritdoc />
    public bool IsMatch(string fileName)
    {
        var match = SeasonRegex().Match(fileName);
        if (!match.Success) return false;

        if (!int.TryParse(match.Groups[2].Value, out var year)) return false;
        return (year == _seasonProductionYear || year == _seriesProductionYear) &&
               string.Equals(_seriesName, match.Groups[1].Value.SanitizeName(), StringComparison.OrdinalIgnoreCase) &&
               string.Equals(match.Groups[3].Value.SanitizeName(), _seasonName, StringComparison.OrdinalIgnoreCase);
    }

    [GeneratedRegex(@"^(.*?)\s*\((\d{4})\)\s*-\s*(Season \d+)(\.[a-z]{3,})$", RegexOptions.IgnoreCase)]
    private static partial Regex SeasonRegex();
}
