using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfApp.Converters
{
    public sealed class CountToArrayConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return new Int32[System.Convert.ToUInt32(value)];
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        private static Lazy<CountToArrayConverter> _instance = new Lazy<CountToArrayConverter>();
        public static CountToArrayConverter Instance => _instance.Value;
    }
}
