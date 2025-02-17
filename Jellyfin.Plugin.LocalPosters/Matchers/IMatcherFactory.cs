using Jellyfin.Data.Enums;
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
        { typeof(BoxSet), item => new MovieCollectionMatcher((BoxSet)item) }
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
        if (!_factories.TryGetValue(item.GetType(), out var factory))
            throw new InvalidOperationException($"No factory registered for type {item.GetType()}");

        return factory(item);
    }
}
