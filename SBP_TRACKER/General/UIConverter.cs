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

    public class TypeSlaveToColorConverterCheck : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((SLAVE_TYPE)value ==  SLAVE_TYPE.TCU)
                return new SolidColorBrush(Constants.TCU_COLOR);

            else if ((SLAVE_TYPE)value == SLAVE_TYPE.SAMCA)
                return new SolidColorBrush(Constants.SAMCA_COLOR);

            else
                return new SolidColorBrush(Colors.Transparent);

        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((SolidColorBrush)value == new SolidColorBrush(Constants.TCU_COLOR))
                return SLAVE_TYPE.TCU;

            else if ((SolidColorBrush)value == new SolidColorBrush(Constants.SAMCA_COLOR))
                return SLAVE_TYPE.SAMCA;

            else
                return SLAVE_TYPE.GENERAL;

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


    public class LinkVarConverterCheck : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Enum.GetName(typeof(LINK_TO_SEND_TCU), value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value;
        }
    }

    public class LinkAvgConverterCheck : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Enum.GetName(typeof(LINK_TO_AVG), value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value;
        }
    }

    public class LinkGraphicConverterCheck : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Enum.GetName(typeof(LINK_TO_GRAPHIC_TCU), value);
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



    public class FunctionConverterCheck: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString() == "READ_HOLDING_REG")
                return "0x03";

            else if (value.ToString() == "READ_INPUT_REG")
                return "0x04";

            else
                return "0xFF";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString() == "0x03")
                return MODBUS_FUNCION.READ_HOLDING_REG;

            else if (value.ToString() == "0x04")
                return MODBUS_FUNCION.READ_INPUT_REG;

            else
                return MODBUS_FUNCION.UNKNOWN;
        }
    }


}
