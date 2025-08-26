using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfApp.Converters
{
    /// <summary>
    /// 星期几转换器
    /// </summary>
    public sealed class DayOfWeekConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return culture.DateTimeFormat.GetDayName((DayOfWeek)value);
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        private static Lazy<DayOfWeekConverter> _instance = new Lazy<DayOfWeekConverter>();
        public static DayOfWeekConverter Instance => _instance.Value;
    }
}
