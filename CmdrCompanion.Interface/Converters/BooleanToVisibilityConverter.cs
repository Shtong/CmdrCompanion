using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CmdrCompanion.Interface.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value is Boolean)
            {
                bool bValue = (bool)value;
                if (bValue)
                    return Visibility.Visible;
                else
                    return UseHiding ? Visibility.Hidden : Visibility.Collapsed;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Visibility && ((Visibility)value) == Visibility.Visible)
                return true;
            return false;
        }

        public bool UseHiding { get; set; }
    }
}
