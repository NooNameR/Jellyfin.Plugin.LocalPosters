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
    /// <param name="item"></param>
    /// <returns></returns>
    bool IsSupported(BaseItem item);

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    IMatcher Create(BaseItem item);
}

/// <inheritdoc />
public class MatcherFactory : IMatcherFactory
{
    static readonly Dictionary<Type, Func<BaseItem, IMatcher>> _factories = new()
    {
        { typeof(Movie), item => new MovieMatcher((Movie)item) },
        { typeof(Season), item => new SeasonMatcher((Season)item) },
        { typeof(Series), item => new SeriesMatcher((Series)item) },
    };

    /// <inheritdoc />
    public bool IsSupported(BaseItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return _factories.ContainsKey(item.GetType());
    }

    /// <inheritdoc />
    public IMatcher Create(BaseItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return item switch
        {
            Season season => new SeasonMatcher(season),
            Series series => new SeriesMatcher(series),
            Movie movie => new MovieMatcher(movie),
            _ => throw new ArgumentException("Unknown item type")
        };
    }
}
