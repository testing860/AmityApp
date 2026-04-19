using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmityApp.Converters;
public class RelativeTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not DateTime dateTime)
            return string.Empty;

        var now = DateTime.Now;
        var diff = now - dateTime;

        if (diff.TotalSeconds < 0 || Math.Abs(diff.TotalSeconds) < 1)
            return "Just now";

        if (diff.TotalSeconds < 60)
        {
            var seconds = (int)diff.TotalSeconds;
            return $"{seconds}s";
        }

        if (diff.TotalMinutes < 60)
        {
            var minutes = (int)diff.TotalMinutes;
            return $"{minutes}m";
        }

        if (diff.TotalHours < 24)
        {
            var hours = (int)diff.TotalHours;
            return $"{hours}h";
        }

        if (diff.TotalDays < 7)
        {
            var days = (int)diff.TotalDays;
            return $"{days}d";
        }

        // Older than a week: show actual date
        return dateTime.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}