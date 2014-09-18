using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CmdrCompanion.Interface.Converters
{
    public class DistanceDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is float?)
            {
                float? fValue = (float?)value;
                if (!fValue.HasValue)
                    return "<unknown>";
                else
                    return fValue.Value.ToString(NumberFormatInfo.InvariantInfo) + "Ly";
            }
            else if (value is float)
            {
                return ((float)value).ToString(NumberFormatInfo.InvariantInfo) + "Ly";
            }
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
