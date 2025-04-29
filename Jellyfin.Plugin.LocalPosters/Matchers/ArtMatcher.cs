using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <inheritdoc />
public partial class ArtMatcher : IMatcher
{
    private readonly ImageType _imageType;
    private readonly HashSet<string> _names;
    private readonly int? _year;

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <param name="originalName"></param>
    /// <param name="year"></param>
    /// <param name="imageType"></param>
    public ArtMatcher(string name, string originalName, int? year, ImageType imageType)
    {
        var titles = new[] { name, originalName }.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        SearchPatterns =
            titles.Select(x => $"{x.SanitizeName("*")}*{year}*{imageType}*.*".Replace("**", "*", StringComparison.Ordinal)).ToHashSet();
        _names = titles.Select(x => x.SanitizeName()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        _year = year;
        _imageType = imageType;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="item"></param>
    /// <param name="imageType"></param>
    public ArtMatcher(BaseItem item, ImageType imageType) : this(item.Name, item.OriginalTitle, item.ProductionYear, imageType)
    {
    }

    /// <inheritdoc />
    public IReadOnlySet<string> SearchPatterns { get; }

    /// <inheritdoc />
    public bool IsMatch(string fileName)
    {
        if (_names.Count == 0)
            return false;

        var match = ArtRegex().Match(fileName);
        if (!match.Success) return false;

        if (!int.TryParse(match.Groups[2].Value, out var year)) return false;
        if (!Enum.TryParse<ImageType>(match.Groups[3].Value, out var imageType)) return false;

        var name = match.Groups[1].Value.SanitizeName();

        return year == _year && imageType == _imageType && _names.Contains(name);
    }

    [GeneratedRegex(@"^(.*?)\s*\((\d{4})\)\s*-\s*([a-z]+)\s*(\.[a-z]{3,})$", RegexOptions.IgnoreCase)]
    private static partial Regex ArtRegex();
}
