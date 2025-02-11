using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities.Movies;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <inheritdoc />
public class MovieMatcher : RegexMatcher
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <param name="productionYear"></param>
    /// <param name="premiereYear"></param>
    public MovieMatcher(string name, int? productionYear, int? premiereYear) : base(
        Regexes(name, productionYear, premiereYear))
    {
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="movie"></param>
    public MovieMatcher(Movie movie) : this(movie.Name, movie.ProductionYear, movie.PremiereDate?.Year)
    {
    }

    static IEnumerable<Regex> Regexes(string name, int? productionYear, int? premiereYear)
    {
        var sanitizedName = name.Replace($" ({productionYear})", "", StringComparison.OrdinalIgnoreCase)
            .Replace($" ({premiereYear})", "", StringComparison.OrdinalIgnoreCase)
            .Replace("–", "-", StringComparison.OrdinalIgnoreCase)
            .Replace("–", @"[-\u2013]", StringComparison.OrdinalIgnoreCase);

        yield return new Regex($@"^{sanitizedName} \({productionYear}\)(\.[a-z]+)?$", RegexOptions.IgnoreCase);

        yield return new Regex(
            $@"^{sanitizedName.Replace(":", @"([:_\-\u2013])?", StringComparison.OrdinalIgnoreCase)} \({productionYear}\)(\.[a-z]+)?$",
            RegexOptions.IgnoreCase);

        if (premiereYear.HasValue && premiereYear != productionYear)
        {
            yield return new Regex($@"^{sanitizedName} \({premiereYear}\)(\.[a-z]+)?$", RegexOptions.IgnoreCase);

            yield return new Regex(
                $@"^{sanitizedName.Replace(":", @"([:_\-\u2013])?", StringComparison.OrdinalIgnoreCase)} \({premiereYear}\)(\.[a-z]+)?$",
                RegexOptions.IgnoreCase);
        }

        var split = sanitizedName.Split(":");
        if (split.Length > 1)
            yield return
                new Regex($@"^{split[0]} \({productionYear}\)(\.[a-z]+)?$", RegexOptions.IgnoreCase);
    }
}
