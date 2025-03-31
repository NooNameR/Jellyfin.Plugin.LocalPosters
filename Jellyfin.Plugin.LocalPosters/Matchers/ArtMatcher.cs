using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <inheritdoc />
public partial class ArtMatcher : IMatcher
{
    private readonly ImageType _imageType;
    private readonly string _name;
    private readonly int? _year;

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <param name="year"></param>
    /// <param name="imageType"></param>
    public ArtMatcher(string name, int? year, ImageType imageType)
    {
        SearchPattern = $"{name.SanitizeName("*")}*{year}*{imageType}*.*".Replace("**", "*", StringComparison.Ordinal);
        _name = name.SanitizeName();
        _year = year;
        _imageType = imageType;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="item"></param>
    /// <param name="imageType"></param>
    public ArtMatcher(BaseItem item, ImageType imageType) : this(item.Name, item.ProductionYear, imageType)
    {
    }

    /// <inheritdoc />
    public string SearchPattern { get; }

    /// <inheritdoc />
    public bool IsMatch(string fileName)
    {
        if (string.IsNullOrEmpty(_name))
            return false;

        var match = ArtRegex().Match(fileName);
        if (!match.Success) return false;

        if (!int.TryParse(match.Groups[2].Value, out var year)) return false;
        if (!Enum.TryParse<ImageType>(match.Groups[3].Value, out var imageType)) return false;

        return year == _year && imageType == _imageType &&
               string.Equals(_name, match.Groups[1].Value.SanitizeName(), StringComparison.OrdinalIgnoreCase);
    }

    [GeneratedRegex(@"^(.*?)\s*\((\d{4})\)\s*-\s*([a-z]+)\s*(\.[a-z]{3,})$", RegexOptions.IgnoreCase)]
    private static partial Regex ArtRegex();
}
