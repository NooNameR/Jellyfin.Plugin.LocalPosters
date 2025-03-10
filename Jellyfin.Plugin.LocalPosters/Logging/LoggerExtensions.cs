using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.LocalPosters.Logging;

/// <summary>
///
/// </summary>
public static class LoggerExtensions
{
    private static readonly Action<ILogger, ImageType, string, int?, string, int?, Exception?> _missingSeasonMessage =
        LoggerMessage.Define<ImageType, string, int?, string, int?>(LogLevel.Information, 1,
            "[{ImageType}]: Was not able to match series: {SeriesName} ({Year}), {Season} (Season {SeasonNumber})");

    private static readonly Action<ILogger, ImageType, string, int?, string, int?, string, Exception?> _missingEpisodeMessage =
        LoggerMessage.Define<ImageType, string, int?, string, int?, string>(LogLevel.Information, 1,
            "[{ImageType}]: Was not able to match episode: {SeriesName} ({Year}), {Season}, Episode {EpisodeNumber} - {EpisodeName}");

    private static readonly Action<ILogger, ImageType, string, int?, Exception?> _missingSeriesMessage =
        LoggerMessage.Define<ImageType, string, int?>(LogLevel.Information, 1,
            "[{ImageType}]: Was not able to match series: {SeriesName} ({Year})");

    private static readonly Action<ILogger, ImageType, string, int?, Exception?> _missingMovieMessage =
        LoggerMessage.Define<ImageType, string, int?>(LogLevel.Information, 1,
            "[{ImageType}]: Was not able to match movie: {Name} ({Year})");

    private static readonly Action<ILogger, ImageType, string, Exception?> _missingCollectionMessage =
        LoggerMessage.Define<ImageType, string>(LogLevel.Information, 1, "[{ImageType}]: Was not able to match collection: {Name}");

    private static readonly Action<ILogger, ImageType, string, string, int?, string, int?, Exception?> _matchingEpisodeMessage =
        LoggerMessage.Define<ImageType, string, string, int?, string, int?>(LogLevel.Debug, 2,
            "[{ImageType}]: Matching file {FilePath} for episode: {SeriesName} ({Year}), {Season}, Episode {EpisodeNumber}...");

    private static readonly Action<ILogger, ImageType, string, string, int?, string, int?, Exception?> _matchingSeasonMessage =
        LoggerMessage.Define<ImageType, string, string, int?, string, int?>(LogLevel.Debug, 2,
            "[{ImageType}]: Matching file {FilePath} for series: {SeriesName} ({Year}), {Season} (Season {SeasonNumber})...");

    private static readonly Action<ILogger, ImageType, string, string, int?, Exception?> _matchingSeriesMessage =
        LoggerMessage.Define<ImageType, string, string, int?>(LogLevel.Debug, 2,
            "[{ImageType}]: Matching file {FilePath} for series: {SeriesName} ({Year})...");

    private static readonly Action<ILogger, ImageType, string, string, int?, Exception?> _matchingMovieMessage =
        LoggerMessage.Define<ImageType, string, string, int?>(LogLevel.Debug, 2,
            "[{ImageType}]: Matching file {FilePath} for movie: {Name} ({Year})...");

    private static readonly Action<ILogger, ImageType, string, string, Exception?> _matchingCollectionMessage =
        LoggerMessage.Define<ImageType, string, string>(LogLevel.Debug, 2, "[{ImageType}]: Matching file {FilePath} for movie: {Name}...");

    private static readonly Action<ILogger, ImageType, string, string, int?, string, int?, Exception?> _matchedEpisodeMessage =
        LoggerMessage.Define<ImageType, string, string, int?, string, int?>(LogLevel.Debug, 3,
            "[{ImageType}]: File {FilePath} match series: {SeriesName} ({Year}), {Season}, Episode {EpisodeNumber}");

    private static readonly Action<ILogger, ImageType, string, string, int?, string, int?, Exception?> _matchedSeasonMessage =
        LoggerMessage.Define<ImageType, string, string, int?, string, int?>(LogLevel.Debug, 3,
            "[{ImageType}]: File {FilePath} match episode: {Name} ({Year}), {Season} (Season {SeasonNumber})");

    private static readonly Action<ILogger, ImageType, string, string, int?, Exception?> _matchedSeriesMessage =
        LoggerMessage.Define<ImageType, string, string, int?>(LogLevel.Debug, 3,
            "[{ImageType}]: File {FilePath} match series: {SeriesName} ({Year})");

    private static readonly Action<ILogger, ImageType, string, string, int?, Exception?> _matchedMovieMessage =
        LoggerMessage.Define<ImageType, string, string, int?>(LogLevel.Debug, 3,
            "[{ImageType}]: File {FilePath} match movie: {Name} ({Year})");

    private static readonly Action<ILogger, ImageType, string, string, Exception?> _matchedCollectionMessage =
        LoggerMessage.Define<ImageType, string, string>(LogLevel.Debug, 3, "[{ImageType}]: File {FilePath} match movie: {Name}");


    /// <summary>
    ///
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="file"></param>
    /// <param name="imageType"></param>
    /// <param name="item"></param>
    public static void LogMatching(this ILogger logger, FileSystemMetadata file, ImageType imageType, BaseItem item)
    {
        switch (item)
        {
            case Episode episode:
                _matchingEpisodeMessage(logger, imageType, file.FullName, episode.Series?.Name ?? "Unknown Series",
                    episode.Series?.ProductionYear ?? episode.Season.ProductionYear ?? episode.ProductionYear,
                    episode.Season?.Name ?? "Unknown Season",
                    episode.IndexNumber,
                    null);
                break;
            case Season season:
                _matchingSeasonMessage(logger, imageType, file.FullName, season.Series?.Name ?? "Unknown Series",
                    season.Series?.ProductionYear ?? season.ProductionYear,
                    season.Name,
                    season.IndexNumber,
                    null);
                break;
            case Series series:
                _matchingSeriesMessage(logger, imageType, file.FullName, series.Name, series.ProductionYear, null);
                break;
            case Movie movie:
                _matchingMovieMessage(logger, imageType, file.FullName, movie.Name, movie.ProductionYear, null);
                break;
            case BoxSet boxSet:
                _matchingCollectionMessage(logger, imageType, file.FullName, boxSet.Name, null);
                break;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="imageType"></param>
    /// <param name="item"></param>
    /// <param name="file"></param>
    public static void LogMatched(this ILogger logger, ImageType imageType, BaseItem item, FileSystemMetadata file)
    {
        switch (item)
        {
            case Episode episode:
                _matchedEpisodeMessage(logger, imageType, file.FullName, episode.Series?.Name ?? "Unknown Series",
                    episode.Series?.ProductionYear ?? episode.Season.ProductionYear ?? episode.ProductionYear,
                    episode.Season?.Name ?? "Unknown Season",
                    episode.IndexNumber,
                    null);
                break;
            case Season season:
                _matchedSeasonMessage(logger, imageType, file.FullName, season.Series?.Name ?? "Unknown Series",
                    season.Series?.ProductionYear ?? season.ProductionYear,
                    season.Name,
                    season.IndexNumber,
                    null);
                break;
            case Series series:
                _matchedSeriesMessage(logger, imageType, file.FullName, series.Name, series.ProductionYear, null);
                break;
            case Movie movie:
                _matchedMovieMessage(logger, imageType, file.FullName, movie.Name, movie.ProductionYear, null);
                break;
            case BoxSet boxSet:
                _matchedCollectionMessage(logger, imageType, file.FullName, boxSet.Name, null);
                break;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="imageType"></param>
    /// <param name="item"></param>
    public static void LogMissing(this ILogger logger, ImageType imageType, BaseItem item)
    {
        switch (item)
        {
            case Series series:
                _missingSeriesMessage(logger, imageType, series.Name, series.ProductionYear, null);
                break;
            case Season season:
                _missingSeasonMessage(logger, imageType, season.Series?.Name ?? "Unknown Series",
                    season.Series?.ProductionYear ?? season.ProductionYear, season.Name, season.IndexNumber, null);
                break;
            case Episode episode:
                _missingEpisodeMessage(logger, imageType, episode.Series?.Name ?? "Unknown Series",
                    episode.Series?.ProductionYear ?? episode.Season.ProductionYear ?? episode.ProductionYear,
                    episode.Season?.Name ?? "Unknown Season",
                    episode.IndexNumber, episode.Name,
                    null);
                break;
            case Movie movie:
                _missingMovieMessage(logger, imageType, movie.Name, movie.ProductionYear, null);
                break;
            case BoxSet boxSet:
                _missingCollectionMessage(logger, imageType, boxSet.Name, null);
                break;
        }
    }
}
