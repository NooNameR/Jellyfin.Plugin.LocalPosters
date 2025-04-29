using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities.TV;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <inheritdoc />
public partial class SeasonMatcher : IMatcher
{
    private readonly HashSet<string> _names;
    private readonly int? _seasonIndex;
    private readonly string _seasonName;
    private readonly int? _seasonProductionYear;
    private readonly int? _seriesProductionYear;

    /// <summary>
    ///
    /// </summary>
    /// <param name="seriesName"></param>
    /// <param name="originalName"></param>
    /// <param name="seriesProductionYear"></param>
    /// <param name="seasonName"></param>
    /// <param name="seasonIndex"></param>
    /// <param name="seasonProductionYear"></param>
    public SeasonMatcher(string seriesName, string originalName, int? seriesProductionYear, string seasonName, int? seasonIndex,
        int? seasonProductionYear)
    {
        var titles = new[] { seriesName, originalName }.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        SearchPatterns = titles.Select(x => $"{x.SanitizeName("*")}*.*".Replace("**", "*", StringComparison.Ordinal))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _names = titles.Select(x => x.SanitizeName()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        _seasonName = seasonName.SanitizeName();
        _seasonIndex = seasonIndex;
        _seriesProductionYear = seriesProductionYear;
        _seasonProductionYear = seasonProductionYear;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="season"></param>
    public SeasonMatcher(Season season) : this(season.Series.Name ?? string.Empty, season.Series.OriginalTitle ?? string.Empty,
        season.Series.ProductionYear, season.Name,
        season.IndexNumber,
        season.ProductionYear)
    {
    }

    /// <summary>
    ///
    /// </summary>
    public IReadOnlySet<string> SearchPatterns { get; }

    /// <inheritdoc />
    public bool IsMatch(string fileName)
    {
        if (_names.Count == 0)
            return false;

        var match = SeasonRegex().Match(fileName);
        if (!match.Success) return false;

        if (!int.TryParse(match.Groups[2].Value, out var year)) return false;

        return (year == _seasonProductionYear || year == _seriesProductionYear) &&
               (string.Equals(match.Groups[3].Value.SanitizeName(), _seasonName, StringComparison.OrdinalIgnoreCase) ||
                (int.TryParse(match.Groups[4].Value, out var seasonIndex) && seasonIndex == _seasonIndex)) &&
               _names.Contains(match.Groups[1].Value.SanitizeName());
    }

    [GeneratedRegex(@"^(.*?)\s*\((\d{4})\)\s*-\s*([a-z]+|Season (\d+))\s*(\.[a-z]{3,})$", RegexOptions.IgnoreCase)]
    private static partial Regex SeasonRegex();
}
