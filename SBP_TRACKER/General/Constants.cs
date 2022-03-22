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
        public const string version = "1.2.0";

        public const int index_no_selected = -1;

        public static Color CONNECTED_COLOR= Colors.Green;
        public static Color DISCONNECTED_COLOR = Colors.Red;
        public static Color CHECK_COLOR = Colors.Blue;

        public const string SettingApp_dir = "SettingApp";
        public const string SettingModbus_dir = "SettingModbusSlave";
        public const string Log_dir = "LogFiles";
        public const string Record_dir = "Record";
        public const string Compress_dir = "Compress";

        public const int depur_disable_height = 720;
        public const int depur_enable_height = 750;

        public const int CMD_SCS_METEO_STATION_VALUES = 33;
    }
}
