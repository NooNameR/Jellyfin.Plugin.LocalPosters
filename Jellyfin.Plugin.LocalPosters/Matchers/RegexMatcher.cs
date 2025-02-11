using System.Text.RegularExpressions;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <summary>
///
/// </summary>
public abstract class RegexMatcher : IMatcher
{
    private readonly IEnumerable<Regex> _regexes;

    /// <summary>
    ///
    /// </summary>
    /// <param name="regexes"></param>
    protected RegexMatcher(IEnumerable<Regex> regexes)
    {
        _regexes = regexes;
    }

    /// <inheritdoc />
    public bool IsMatch(string fileName)
    {
        return _regexes.Any(t => t.IsMatch(fileName));
    }
}
