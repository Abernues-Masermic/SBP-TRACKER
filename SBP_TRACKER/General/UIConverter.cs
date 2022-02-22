using System;
using System.Windows.Media;

namespace SBP_TRACKER.ValueConverters
{
    public class BooleanToColorConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value == true)
                return new SolidColorBrush(Constants.CONNECTED_COLOR);
            
            else 
                return new SolidColorBrush(Constants.DISCONNECTED_COLOR);

        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((SolidColorBrush) value == new SolidColorBrush(Constants.CONNECTED_COLOR))
                return true;
            
            else
                return false;
            
        }
    }
}
