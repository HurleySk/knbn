using System;
using System.Globalization;
using System.Windows.Data;

namespace Knbn.Extension.UI.Converters
{
    public class TimeAgoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
            {
                var diff = DateTime.UtcNow - dt;
                if (diff.TotalMinutes < 1) return "just now";
                if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
                if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
                return $"{(int)diff.TotalDays}d ago";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
