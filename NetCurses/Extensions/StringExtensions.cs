namespace NetCurses.Extensions;
public static class StringExtensions
{
    /// <summary>
    /// If the string is longer than the max length, shorten it and add "..."
    /// </summary>
    /// <param name="str"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    public static string ShortenString(this string str, int maxLength)
    {
        if (str.Length < maxLength) return str;
        var shorter = str.Substring(0, maxLength - 3);
        return $"{shorter}...";
    }
}
