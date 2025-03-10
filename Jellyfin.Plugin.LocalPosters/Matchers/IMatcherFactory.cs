using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <summary>
///
/// </summary>
public interface IMatcherFactory
{
    /// <summary>
    ///
    /// </summary>
    HashSet<BaseItemKind> SupportedItemKinds { get; }

    /// <summary>
    ///
    /// </summary>
    HashSet<ImageType> SupportedImageTypes(BaseItem item);

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    IMatcher Create(ImageType imageType, BaseItem item);
}

/// <inheritdoc />
public class MatcherFactory : IMatcherFactory
{
    static readonly Dictionary<BaseItemKind, Func<BaseItem, IMatcher>> _factories = new()
    {
        { BaseItemKind.Movie, item => new MovieMatcher((Movie)item) },
        { BaseItemKind.Season, item => new SeasonMatcher((Season)item) },
        { BaseItemKind.Series, item => new SeriesMatcher((Series)item) },
        { BaseItemKind.Episode, item => new EpisodeMatcher((Episode)item) },
        { BaseItemKind.BoxSet, item => new MovieCollectionMatcher((BoxSet)item) }
    };

    private static readonly HashSet<BaseItemKind> _kinds = [.._factories.Keys];

    private static readonly HashSet<ImageType> _imageTypes =
        [ImageType.Primary, ImageType.Art, ImageType.Backdrop, ImageType.Logo, ImageType.Banner];

    /// <inheritdoc />
    public HashSet<BaseItemKind> SupportedItemKinds => _kinds;

    /// <inheritdoc />
    public HashSet<ImageType> SupportedImageTypes(BaseItem item)
    {
        if (item.GetBaseItemKind() == BaseItemKind.Episode || item.GetBaseItemKind() == BaseItemKind.Season)
            return [ImageType.Primary];

        return _imageTypes;
    }

    /// <inheritdoc />
    public IMatcher Create(ImageType imageType, BaseItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (imageType != ImageType.Primary)
            return new ArtMatcher(item, imageType);

        if (!_factories.TryGetValue(item.GetBaseItemKind(), out var factory))
            throw new InvalidOperationException($"No factory registered for type {item.GetType()}");

        return factory(item);
    }
}

/// <summary>
///
/// </summary>
public static class MatcherFactoryExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static bool IsSupported(this IMatcherFactory factory, BaseItem item) =>
        factory.SupportedItemKinds.Contains(item.GetBaseItemKind());
}
