using System.Globalization;

namespace AmityApp.Converters;

public class EmulatorImageConverter : IValueConverter
{
    private const string ApiBaseUrl = "http://10.0.2.2:5009/";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string url || string.IsNullOrWhiteSpace(url))
            return "user.png";

        // Replace Development HTTPS URLs with HTTP emulator URL
        url = url.Replace("https://localhost:7134", ApiBaseUrl)
                 .Replace("https://10.0.2.2:7134", ApiBaseUrl);

        // If still a relative path, Prepend Base URL
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = $"{ApiBaseUrl}{url.Replace("\\", "/")}";
        }

        return url;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}