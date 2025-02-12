namespace Jellyfin.Plugin.LocalPosters.Utils;

/// <summary>
///
/// </summary>
public interface IBorderReplacer
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    Stream Replace(string source);
}
