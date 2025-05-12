using System.Text.RegularExpressions;

namespace Formula.Extensions;

public static class StringExtensions
{
    public static bool In(this string str, params char[] chars)
    {
        foreach (var c in chars)
        {
            if (str.Contains(c)) return true;
        }

        return false;
    }

    public static bool IsMatch(this Regex regex, char character)
    {
        return regex.IsMatch(character.ToString());
    }
}