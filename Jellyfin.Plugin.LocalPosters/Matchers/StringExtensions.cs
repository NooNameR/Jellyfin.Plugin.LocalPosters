using System.Text.RegularExpressions;

namespace Jellyfin.Plugin.LocalPosters.Matchers;

/// <summary>
///
/// </summary>
public static partial class StringExtensions
{
    [GeneratedRegex(@"\(\d{4}\)")]
    private static partial Regex YearRegex();

    [GeneratedRegex("[^A-z0-9]")]
    private static partial Regex SpecialCharsRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpacesRegex();

    /// <summary>
    ///
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private static string SanitizeName(this string input)
    {
        input = YearRegex().Replace(input, String.Empty);
        input = SpecialCharsRegex().Replace(input, " ");
        return SpacesRegex().Replace(input, " ").Trim();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="input1"></param>
    /// <param name="input2"></param>
    /// <returns></returns>
    public static bool EqualsSanitizing(this string input1, string input2)
    {
        return string.Equals(SanitizeName(input1), SanitizeName(input2), StringComparison.OrdinalIgnoreCase);
    }
}
