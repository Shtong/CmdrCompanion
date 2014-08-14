using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CmdrCompanion.Interface.Converters
{
    public class AgeConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value is DateTime)
            {
                DateTime date = (DateTime)value;
                TimeSpan age = DateTime.Now - date;

                if (age.TotalMinutes < 2)
                    return "Now";
                else if (age.TotalMinutes < 60)
                    return String.Format("{0:N0}mins ago", age.TotalMinutes);
                else if (age.TotalHours < 24)
                    return String.Format("{0:N0}h {1:N0}mins ago", age.TotalHours, age.Minutes);
                else
                    return date.ToShortDateString() + " " + date.ToShortTimeString();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
