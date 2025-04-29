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
    private readonly HashSet<string> _seriesNames;
    private readonly int? _seriesProductionYear;

    /// <summary>
    ///
    /// </summary>
    /// <param name="seriesName"></param>
    /// <param name="originalName"></param>
    /// <param name="seriesProductionYear"></param>
    /// <param name="seasonIndex"></param>
    /// <param name="seasonProductionYear"></param>
    /// <param name="episodeIndex"></param>
    /// <param name="episodeProductionYear"></param>
    public EpisodeMatcher(string seriesName, string originalName, int? seriesProductionYear, int? seasonIndex, int? seasonProductionYear,
        int? episodeIndex,
        int? episodeProductionYear)
    {
        var titles = new[] { seriesName, originalName }.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        SearchPatterns = new[] { seriesName, originalName }.Select(x =>
                $"{x.SanitizeName("*")}*S{seasonIndex}*E{episodeIndex}*.*".Replace("**", "*", StringComparison.Ordinal))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _seriesNames = titles.Select(x => x.SanitizeName()).ToHashSet(StringComparer.OrdinalIgnoreCase);
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
    public EpisodeMatcher(Episode episode) : this(episode.Series?.Name ?? string.Empty, episode.Series?.OriginalTitle ?? string.Empty,
        episode.Series?.ProductionYear,
        episode.Season.IndexNumber,
        episode.Season.ProductionYear,
        episode.IndexNumber,
        episode.ProductionYear)
    {
    }

    /// <inheritdoc />
    public IReadOnlySet<string> SearchPatterns { get; }

    /// <inheritdoc />
    public bool IsMatch(string fileName)
    {
        if (_seriesNames.Count == 0)
            return false;

        var match = EpisodeRegex().Match(fileName);
        if (!match.Success) return false;

        if (!int.TryParse(match.Groups[3].Value, out var season)) return false;
        if (!int.TryParse(match.Groups[4].Value, out var episode)) return false;
        var seriesName = match.Groups[1].Value.SanitizeName();

        return IsYearMatch(match.Groups[2].Value) &&
               _seriesNames.Contains(seriesName) &&
               _seasonIndex == season && _episodeIndex == episode;
    }

    private bool IsYearMatch(string yearString)
    {
        if (!int.TryParse(yearString, out var year)) return true;

        return year == _episodeProductionYear || year == _seriesProductionYear || year == _seasonProductionYear;
    }

    [GeneratedRegex(@"^(.*?)(?:\s*\((\d{4})\))?\s*-\s*S(\d+)\s*E(\d+)\s*(\.[a-z]{3,})$", RegexOptions.IgnoreCase)]
    private static partial Regex EpisodeRegex();
}
