using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.LocalPosters.Logging;

/// <summary>
///
/// </summary>
public static class LoggerExtensions
{
    private static readonly Action<ILogger, string, int?, string, int?, Exception?> _missingSeasonMessage =
        LoggerMessage.Define<string, int?, string, int?>(LogLevel.Information, 1,
            "Was not able to match series: {SeriesName} ({Year}), {Season} (Season {SeasonNumber})");

    private static readonly Action<ILogger, string, int?, string, int?, string, Exception?> _missingEpisodeMessage =
        LoggerMessage.Define<string, int?, string, int?, string>(LogLevel.Information, 1,
            "Was not able to match episode: {SeriesName} ({Year}), {Season}, Episode {EpisodeNumber} - {EpisodeName}");

    private static readonly Action<ILogger, string, int?, Exception?> _missingSeriesMessage =
        LoggerMessage.Define<string, int?>(LogLevel.Information, 1, "Was not able to match series: {SeriesName} ({Year})");

    private static readonly Action<ILogger, string, int?, Exception?> _missingMovieMessage =
        LoggerMessage.Define<string, int?>(LogLevel.Information, 1, "Was not able to match movie: {Name} ({Year})");

    private static readonly Action<ILogger, string, Exception?> _missingCollectionMessage =
        LoggerMessage.Define<string>(LogLevel.Information, 1, "Was not able to match collection: {Name}");

    private static readonly Action<ILogger, string, string, int?, string, int?, string, Exception?> _matchingEpisodeMessage =
        LoggerMessage.Define<string, string, int?, string, int?, string>(LogLevel.Debug, 2,
            "Matching file {FilePath} for episode: {SeriesName} ({Year}), {Season}, Episode {EpisodeNumber} - {EpisodeName}...");

    private static readonly Action<ILogger, string, string, int?, string, int?, Exception?> _matchingSeasonMessage =
        LoggerMessage.Define<string, string, int?, string, int?>(LogLevel.Debug, 2,
            "Matching file {FilePath} for series: {SeriesName} ({Year}), {Season} (Season {SeasonNumber})...");

    private static readonly Action<ILogger, string, string, int?, Exception?> _matchingSeriesMessage =
        LoggerMessage.Define<string, string, int?>(LogLevel.Debug, 2, "Matching file {FilePath} for series: {SeriesName} ({Year})...");

    private static readonly Action<ILogger, string, string, int?, Exception?> _matchingMovieMessage =
        LoggerMessage.Define<string, string, int?>(LogLevel.Debug, 2, "Matching file {FilePath} for movie: {Name} ({Year})...");

    private static readonly Action<ILogger, string, string, Exception?> _matchingCollectionMessage =
        LoggerMessage.Define<string, string>(LogLevel.Debug, 2, "Matching file {FilePath} for movie: {Name}...");

    private static readonly Action<ILogger, string, string, int?, string, int?, string, Exception?> _matchedEpisodeMessage =
        LoggerMessage.Define<string, string, int?, string, int?, string>(LogLevel.Debug, 3,
            "File {FilePath} match series: {SeriesName} ({Year}), {Season}, Episode {EpisodeNumber} - {EpisodeName}");

    private static readonly Action<ILogger, string, string, int?, string, int?, Exception?> _matchedSeasonMessage =
        LoggerMessage.Define<string, string, int?, string, int?>(LogLevel.Debug, 3,
            "File {FilePath} match episode: {Name} ({Year}), {Season} (Season {SeasonNumber})");

    private static readonly Action<ILogger, string, string, int?, Exception?> _matchedSeriesMessage =
        LoggerMessage.Define<string, string, int?>(LogLevel.Debug, 3, "File {FilePath} match series: {SeriesName} ({Year})");

    private static readonly Action<ILogger, string, string, int?, Exception?> _matchedMovieMessage =
        LoggerMessage.Define<string, string, int?>(LogLevel.Debug, 3, "File {FilePath} match movie: {Name} ({Year})");

    private static readonly Action<ILogger, string, string, Exception?> _matchedCollectionMessage =
        LoggerMessage.Define<string, string>(LogLevel.Debug, 3, "File {FilePath} match movie: {Name}");


    /// <summary>
    ///
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="file"></param>
    /// <param name="item"></param>
    public static void LogMatching(this ILogger logger, FileSystemMetadata file, BaseItem item)
    {
        switch (item)
        {
            case Episode episode:
                _matchingEpisodeMessage(logger, file.FullName, episode.Series?.Name ?? "Unknown Series",
                    episode.Series?.ProductionYear ?? episode.Season.ProductionYear ?? episode.ProductionYear,
                    episode.Season?.Name ?? "Unknown Season",
                    episode.IndexNumber, episode.Name,
                    null);
                break;
            case Season season:
                _matchingSeasonMessage(logger, file.FullName, season.Series?.Name ?? "Unknown Series",
                    season.Series?.ProductionYear ?? season.ProductionYear,
                    season.Name,
                    season.IndexNumber,
                    null);
                break;
            case Series series:
                _matchingSeriesMessage(logger, file.FullName, series.Name, series.ProductionYear, null);
                break;
            case Movie movie:
                _matchingMovieMessage(logger, file.FullName, movie.Name, movie.ProductionYear, null);
                break;
            case BoxSet boxSet:
                _matchingCollectionMessage(logger, file.FullName, boxSet.Name, null);
                break;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="item"></param>
    /// <param name="file"></param>
    public static void LogMatched(this ILogger logger, BaseItem item, FileSystemMetadata file)
    {
        switch (item)
        {
            case Episode episode:
                _matchedEpisodeMessage(logger, file.FullName, episode.Series?.Name ?? "Unknown Series",
                    episode.Series?.ProductionYear ?? episode.Season.ProductionYear ?? episode.ProductionYear,
                    episode.Season?.Name ?? "Unknown Season",
                    episode.IndexNumber, episode.Name,
                    null);
                break;
            case Season season:
                _matchedSeasonMessage(logger, file.FullName, season.Series?.Name ?? "Unknown Series",
                    season.Series?.ProductionYear ?? season.ProductionYear,
                    season.Name,
                    season.IndexNumber,
                    null);
                break;
            case Series series:
                _matchedSeriesMessage(logger, file.FullName, series.Name, series.ProductionYear, null);
                break;
            case Movie movie:
                _matchedMovieMessage(logger, file.FullName, movie.Name, movie.ProductionYear, null);
                break;
            case BoxSet boxSet:
                _matchedCollectionMessage(logger, file.FullName, boxSet.Name, null);
                break;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="item"></param>
    public static void LogMissing(this ILogger logger, BaseItem item)
    {
        switch (item)
        {
            case Series series:
                _missingSeriesMessage(logger, series.Name, series.ProductionYear, null);
                break;
            case Season season:
                _missingSeasonMessage(logger, season.Series?.Name ?? "Unknown Series",
                    season.Series?.ProductionYear ?? season.ProductionYear, season.Name, season.IndexNumber, null);
                break;
            case Episode episode:
                _missingEpisodeMessage(logger, episode.Series?.Name ?? "Unknown Series",
                    episode.Series?.ProductionYear ?? episode.Season.ProductionYear ?? episode.ProductionYear,
                    episode.Season?.Name ?? "Unknown Season",
                    episode.IndexNumber, episode.Name,
                    null);
                break;
            case Movie movie:
                _missingMovieMessage(logger, movie.Name, movie.ProductionYear, null);
                break;
            case BoxSet boxSet:
                _missingCollectionMessage(logger, boxSet.Name, null);
                break;
        }
    }
}
