using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfApp1
{
    public class StringConcatConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // 使用string.Join来拼接字符串，并移除null或空字符串
            //return string.Join(" ", values.Where(x => x != null && x.ToString() != string.Empty));
            return values[0].ToString() + parameter.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            // 双向绑定通常不需要实现ConvertBack
            throw new NotImplementedException();
        }
    }
}
