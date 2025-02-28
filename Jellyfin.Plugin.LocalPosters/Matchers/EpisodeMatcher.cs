using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities.TV;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <inheritdoc />
public partial class EpisodeMatcher : IMatcher
{
    private readonly int? _episodeIndex;
    private readonly int? _episodeProductionYear;
    private readonly int? _seasonIndex;
    private readonly int? _seasonProductionYear;
    private readonly string _seriesName;
    private readonly int? _seriesProductionYear;

    /// <summary>
    ///
    /// </summary>
    /// <param name="seriesName"></param>
    /// <param name="seriesProductionYear"></param>
    /// <param name="seasonIndex"></param>
    /// <param name="seasonProductionYear"></param>
    /// <param name="episodeIndex"></param>
    /// <param name="episodeProductionYear"></param>
    public EpisodeMatcher(string seriesName, int? seriesProductionYear, int? seasonIndex, int? seasonProductionYear, int? episodeIndex,
        int? episodeProductionYear)
    {
        _seriesName = seriesName.SanitizeName();
        _episodeIndex = episodeIndex;
        _seriesProductionYear = seriesProductionYear;
        _seasonIndex = seasonIndex;
        _seasonProductionYear = seasonProductionYear;
        _episodeProductionYear = episodeProductionYear;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="episode"></param>
    public EpisodeMatcher(Episode episode) : this(episode.Series?.Name ?? string.Empty, episode.Series?.ProductionYear,
        episode.Season.IndexNumber,
        episode.Season.ProductionYear,
        episode.IndexNumber,
        episode.ProductionYear)
    {
    }

    /// <inheritdoc />
    public bool IsMatch(string fileName)
    {
        if (string.IsNullOrEmpty(_seriesName))
            return false;

        var match = EpisodeRegex().Match(fileName);
        if (!match.Success) return false;

        if (!int.TryParse(match.Groups[3].Value, out var season)) return false;
        if (!int.TryParse(match.Groups[4].Value, out var episode)) return false;

        return IsYearMatch(match.Groups[2].Value) &&
               string.Equals(_seriesName, match.Groups[1].Value.SanitizeName(), StringComparison.OrdinalIgnoreCase) &&
               _seasonIndex == season && _episodeIndex == episode;
    }

    private bool IsYearMatch(string yearString)
    {
        if (!int.TryParse(yearString, out var year)) return true;

        return year == _episodeProductionYear || year == _seriesProductionYear || year == _seasonProductionYear;
    }

    [GeneratedRegex(@"^(.*?)(?:\s*\((\d{4})\))?\s*-\s*S(\d+)\s*E(\d+)(\.[a-z]{3,})$", RegexOptions.IgnoreCase)]
    private static partial Regex EpisodeRegex();
}
