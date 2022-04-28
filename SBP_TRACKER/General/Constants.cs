using System.Windows.Media;

namespace SBP_TRACKER
{
    public static class Constants
    {
        public const string version = "1.5.0";

        public const int index_no_selected = -1;

        public static Color CONNECTED_COLOR= Colors.Green;
        public static Color DISCONNECTED_COLOR = Colors.Red;
        public static Color CHECK_COLOR = Colors.Blue;
        public static Color TCU_COLOR = Colors.Purple;
        public static Color SAMCA_COLOR = Colors.Orange;

        public const string SettingApp_dir = "SettingApp";
        public const string SettingModbus_dir = "SettingModbusSlave";
        public const string Log_dir = "LogFiles";
        public const string Record_dir = "Record";
        public const string Compress_dir = "Compress";

        public const string Record_scs1 = "SBPT_record_normal_";
        public const string Record_scs2 = "SBPT_record_fast_";
        public const string Record_tcu = "SBPT_record_tcu_";
        public const string Record_samca = "SBPT_record_samca_";


        public const int depur_disable_height = 720;
        public const int depur_enable_height = 750;

        public const int CMD_SCS_METEO_STATION_VALUES = 33;
        public const int MAX_WIND_AVG = 4;
        public const int MAX_INC_AVG = 6;
        public const int MAX_DYN_AVG = 3;
        public const int MAX_SCADA_WD = 9999;

        public const int Error_code = -9999;
    }
}
