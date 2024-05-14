namespace Allegro.Extensions.Configuration.Extensions;

internal static class StringExtensions
{
    internal static string ToContextName(this string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        return filePath.Split('/').LastOrDefault()?.Split('.').FirstOrDefault() ?? string.Empty;
    }
}