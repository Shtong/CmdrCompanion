using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CmdrCompanion.Interface.Converters
{
    /// <summary>
    /// A converter that converts from simple boolean to nullable boolean.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool?))]
    public class NullableBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value is bool)
            {
                return new Nullable<bool>((bool)value);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value is Nullable<bool>)
            {
                bool? bVal = (bool?)value;
                return bVal.HasValue && bVal.Value;
            }

            return value;
        }
    }
}
