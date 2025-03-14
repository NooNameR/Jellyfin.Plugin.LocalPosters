namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <summary>
///
/// </summary>
public interface IMatcher
{
    /// <summary>
    ///
    /// </summary>
    string SearchPattern { get; }

    /// <summary>
    ///
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    bool IsMatch(string fileName);
}
