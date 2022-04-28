namespace SBP_TRACKER
{
    public enum BIT_STATE
    {
        OFF = 0,
        ON = 1
    }

    public enum FORM_ACTION
    {
        CREATE = 0,
        UPDATE = 1,
        REMOVE = 2,
        CANCEL = 3
    }

    public enum TCP_ACTION
    {
        CONNECT = 0,
        DISCONNECT = 1,
        REQUEST = 2,
        RESPONSE = 3,
        READ = 4,
        WRITE = 5,
        TIMEOUT = 6,
        ERROR_CONNECT = 7,
        ERROR_READ = 8,
        ERROR_WRITE = 9,
        RECONNECT = 10,
        RECOVERY = 11
    }

    public enum READ_STATE { 
        OK =0,
        ERROR = 1,
        WAIT = 2
    }

    public enum DECIMAL_SEP
    {
        PUNTO = 0,
        COMA = 1,
    }

    public enum FIELD_SEP
    {
        COMA = 0,
        PUNTO_COMA = 1,
    }

    public enum MAIL_STATE
    {
        COMPRESS = 0,
        SEND = 1,
        END = 2,
    }

    public enum COMMAND_FIELD
    {
        INDEX = 0,
        NAME = 1,
        NUM_PARAM = 2,
        VAR_NAME = 3,
        VAR_TYPE = 4,
        SEPARATION = 5,
    }


    public enum LINK_TO_SEND_TCU
    {
        SCADA_WD = 0,
        WIN_SPEED = 1,
        WIN_DIR = 2,
        AMBIENT_TEMP = 3,
        AMBIENT_PRESSURE = 4,
        DIRECT_IRRAD = 5,
        DIFUSSE_IRRAD = 6,
        FIELD_SAFETY = 7,
        METEO_WD = 8,
        NONE = 255,
    }


    public enum LINK_TO_AVG
    {
        WIND_AVG_SAMCA = 0,
        WIND_AVG_SBPT = 1,
        INC1_SLOPE_AVG_SBPT = 2,
        INC2_SLOPE_AVG_SBPT = 3,
        INC3_SLOPE_AVG_SBPT = 4,
        INC4_SLOPE_AVG_SBPT = 5,
        INC5_SLOPE_AVG_SBPT = 6,
        INC6_SLOPE_AVG_SBPT = 7,
        DYN1_AVG_SBPT = 8,
        DYN2_AVG_SBPT = 9,
        DYN3_AVG_SBPT = 10,
        NONE = 255,
    }

    public enum MODBUS_FUNCION
    {
        READ_HOLDING_REG = 0,
        READ_INPUT_REG = 1,
        UNKNOWN = 2
    }


    public enum CODIFIED_STATUS_STATE
    {
        POWER_ON_DIAGNOSIS = 0,
        OFFLINE = 1,
        OFFLINE_WITH_ERROR = 2,
        ONLINE_WAITING_CMD = 3,
        POSITION_EL_DEG = 4,
        POSITION_MAIN_DRIVE_MM = 5,
        STOW = 6,
        TRACKING_SUN_XYZ = 7,
        TRACKING_DEF_XYZ = 8,
        MAINT_CHK_LOCK_1 = 9,
        MAINT_CHK_LOCK_2 = 10,
        MAIN_CHK_1_LH = 11,
        DRIVE_SPEED = 12,
        EMERG_CMD_POS_OS_STOW = 13,
        EMERG_AUTO_POS_OS_STOW = 14,
        NONE = 255
    }


    public enum LINK_TO_GRAPHIC
    {
        ESTADO_CODIF_TCU = 0,
        CODIF_BITS_TO_TCU1 = 1,
        TRACKER_POS_EL = 2,
        TRACKER_SET_POINT_EL = 3,
        TRACKER_ERROR_EL = 4,
        TRACKER_CONSIGNA_DESFASE_EL = 5,
        MAIN_DRIVE_POS = 6,
        TRACKER_SPEED = 7,
        MAIN_DRIVE_POWER = 8,
        LOCK1_DRIVE_POWER = 9,
        LOCK2_DRIVE_POWER = 10,
        FECHA_RTC = 11,
        MILISECONDS = 12,
        NONE = 255,
    }

    public enum CODIFIED_STATUS_DRIVE_BIT
    {
        MAIN_DRIVE =0,
        MAIN_DRIVE_FORWARD = 1,
        MAIN_DRIVE_BACKWARD = 2,
        LOCK1_DRIVE = 3,
        LOCK1_DRIVE_FORWARD = 4,
        LOCK1_DRIVE_BACKWARD = 5,
        LOCK2_DRIVE = 6,
        LOCK2_DRIVE_FORWARD = 7,
        LOCK2_DRIVE_BACKWARD = 8,
    }

    public enum FIELD_SAFETY_VALUE_BIT
    {
        AUTOTRACK_SCADA = 0,
        WIND_CONDITIONS_OK = 1,
        AUTOTRACK_SAMCA = 2,
        SLAVE_COMM_OK = 8,
        EMERGENCY_STOW_BUTTON = 9,
        INC_IN_RANGES_FOR_EMERGENCY_STOW = 10,
        DYN_IN_RANGES_FOR_EMERGENCY_STOW = 11,
        INC_IN_RANGES_FOR_ALARM = 14,
        DYN_IN_RANGES_FOR_ALARM = 15
    }

    public enum FIELD_SAFETY_CHECK
    {
        AUTOTRACK_SCADA = 0,
        WIND_CONDITIONS_OK = 1,
        AUTOTRACK_SAMCA = 2,
        SBPT_SLAVES_COMM_OK = 3,
        SBPT_EMERGENCY_STOW = 4,
        SBPT_INC_IN_RANGES_FOR_EMERG_STOW = 5,
        SBPT_INC_IN_RANGES_FOR_ALARM = 6,
        SBPT_DYN_IN_RANGES_FOR_EMERG_STOW = 7,
        SBPT_DYN_IN_RANGES_FOR_ALARM = 8,
    }


    public enum GRAPHIC_SCADA_COMMANDS
    {
        OFFLINE = 0,
        ONLINE = 1,
        STOW = 5,
        TRACKING = 11
    }


    public enum SLAVE_TYPE
    {

        GENERAL = 0,
        TCU = 1,
        SAMCA = 2
    }

    public enum WIND_AVG_POSITION : int
    {
        SBPT_3SEC = 0,
        SAMCA_3SEC = 1,
        SBPT_10MIN = 2,
        SAMCA_10MIN = 3,
    }

    public enum WIND_AVG_INTERVAL : int
    {
        SHORT_SEC = 3,
        LONG_MIN= 10
    }

    public enum INC_LABEL_AVG_POSITION : int
    {
        INC1 = 0,
        INC2 = 1,
        INC3 = 2,
        INC4 = 3,
        INC5 = 4,
        TCU = 5,
    }

    public enum DYN_LABEL_AVG_POSITION : int
    {
        DYN1 = 0,
        DYN2 = 1,
        DYN3 = 2,
    }

    public enum WIND_DATE_TRIGGER_3SEC : int
    {
        SBPT_3SEC = 0,
        SAMCA_3SEC = 1
    }

    public enum WIND_LOW_HIST_10MIN : int
    {
        SBPT_10MIN = 0,
        SAMCA_10MIN = 1
    }

}
