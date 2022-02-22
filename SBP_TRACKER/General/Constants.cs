using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SBP_TRACKER
{
    public static class Constants
    {
        public const string version = "1.0.0";

        public const int index_no_selected = -1;

        public const int MAX_MODBUS_REG = 120;

        public static Color CONNECTED_COLOR= Colors.Green;
        public static Color DISCONNECTED_COLOR = Colors.Red;

        public const string SettingApp_dir = "SettingApp";
        public const string SettingModbus_dir = "SettingModbusSlave";
        public const string Log_dir = "LogFiles";
        public const string Report_dir = "Report";
    }
}
