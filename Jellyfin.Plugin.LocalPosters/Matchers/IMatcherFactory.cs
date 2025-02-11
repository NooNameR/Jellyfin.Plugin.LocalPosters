using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <summary>
///
/// </summary>
public interface IMatcherFactory
{
    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    IMatcher Create(BaseItem item);
}

/// <inheritdoc />
public class MatcherFactory : IMatcherFactory
{
    /// <summary>
    ///
    /// </summary>
    public MatcherFactory()
    {
    }

    /// <inheritdoc />
    public IMatcher Create(BaseItem item)
    {
        return item switch
        {
            Season season => new SeasonMatcher(season),
            Series series => new SeriesMatcher(series),
            Movie movie => new MovieMatcher(movie),
            _ => throw new ArgumentException("Unknown item type")
        };
    }
}
