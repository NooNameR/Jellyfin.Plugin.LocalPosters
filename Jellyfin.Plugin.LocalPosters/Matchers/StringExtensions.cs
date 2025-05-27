using System.Globalization;
using System.Text;
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
    /// <param name="replacement"></param>
    /// <returns></returns>
    public static string SanitizeName(this string input, string replacement = " ")
    {
        input = RemoveDiacritics(input);

        input = YearRegex().Replace(input, string.Empty);
        input = SanitizeSpecialChars(input, replacement);
        return SpacesRegex().Replace(input, replacement).Trim();
    }

    public static string SanitizeSpecialChars(this string input, string replacement = "")
    {
        return SpecialCharsRegex().Replace(input, replacement);
    }

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(text.Length);

        for (int i = 0; i < normalizedString.Length; i++)
        {
            char c = normalizedString[i];
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}
