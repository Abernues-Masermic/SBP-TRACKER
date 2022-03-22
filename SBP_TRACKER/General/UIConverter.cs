using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SBP_TRACKER.UIConverter
{
    public class BooleanToColorConverterConnect : IValueConverter
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


    public class BooleanToColorConverterCheck: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value == true)
                return new SolidColorBrush(Constants.CHECK_COLOR);

            else
                return new SolidColorBrush(Colors.Transparent);

        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((SolidColorBrush)value == new SolidColorBrush(Constants.CHECK_COLOR))
                return true;

            else
                return false;

        }
    }


    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value == true)
                return Visibility.Visible;

            else
                return Visibility.Collapsed;

        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Visible)
                return true;

            else
                return false;

        }
    }


    public class EmetWatchdogConverterCheck : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Enum.GetName(typeof(EMET_WATCHDOG_ASSOC), value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value;
        }
    }



    public class TypeVarConverterCheck : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DataConverter.Type_code_to_string((TypeCode) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DataConverter.String_to_type_code(value.ToString());
        }
    }
}
