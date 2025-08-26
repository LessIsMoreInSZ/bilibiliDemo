using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfApp.Converters
{
    public sealed class EvenToBooleanConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return System.Convert.ToUInt32(value) % 2 == 0;
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        private static Lazy<EvenToBooleanConverter> _instance = new Lazy<EvenToBooleanConverter>();
        public static EvenToBooleanConverter Instance => _instance.Value;
    }
}
