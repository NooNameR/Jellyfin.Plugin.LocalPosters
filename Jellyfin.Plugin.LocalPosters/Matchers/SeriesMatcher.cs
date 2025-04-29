using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities.TV;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <inheritdoc />
public partial class SeriesMatcher : IMatcher
{
    private readonly HashSet<string> _names;
    private readonly int? _productionYear;

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <param name="originalName"></param>
    /// <param name="productionYear"></param>
    public SeriesMatcher(string name, string originalName, int? productionYear)
    {
        var titles = new[] { name, originalName }.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        SearchPatterns = titles.Select(x => $"{x.SanitizeName("*")}*{productionYear}*.*".Replace("**", "*", StringComparison.Ordinal))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _names = titles.Select(x => x.SanitizeName()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        _productionYear = productionYear;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="series"></param>
    public SeriesMatcher(Series series) : this(series.Name, series.OriginalTitle, series.ProductionYear)
    {
    }

    /// <inheritdoc />
    public IReadOnlySet<string> SearchPatterns { get; }

    /// <inheritdoc />
    public bool IsMatch(string fileName)
    {
        if (_names.Count == 0)
            return false;

        var match = SeasonRegex().Match(fileName);
        if (!match.Success) return false;

        if (!int.TryParse(match.Groups[2].Value, out var year)) return false;
        return (year == _productionYear) &&
               _names.Contains(match.Groups[1].Value.SanitizeName());
    }

    [GeneratedRegex(@"^(.*?)\s*\((\d{4})\)\s*(\.[a-z]{3,})$", RegexOptions.IgnoreCase)]
    private static partial Regex SeasonRegex();
}
