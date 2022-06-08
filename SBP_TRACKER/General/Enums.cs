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

    public enum SETTING_OPTION { 
        MODBUS,
        APP,
        ADVANCED,
        MAIL,
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
        TRACKER_WD = 0,
        WIN_SPEED = 1,
        WIN_DIR = 2,
        AMBIENT_TEMP = 3,
        AMBIENT_PRESSURE = 4,
        DIRECT_IRRAD = 5,
        DIFUSSE_IRRAD = 6,
        METEO_WD = 7,
        SAFETY_SUPERVISOR = 8,
        NONE = 255,
    }

    public enum WD_SEND_TO_SAMCA_POS
    {
        SBPT_METEO = 9,
        SBP_TRACKER = 14
    }

    public enum LINK_TO_GRAPHIC_TCU
    {
        TRACKER_STATE_TCU = 0,
        TRACKER_MODE_TCU = 1,
        TRACKER_POS_EL = 2,
        TRACKER_SET_POINT_EL = 3,
        TRACKER_ERROR_EL = 4,
        MAIN_POWER = 5,
        LOCK1_POWER = 6,
        LOCK2_POWER = 7,
        DATE_RTC = 8,
        MILISECONDS = 9,
        VECTOR_SOLAR_EL = 10,
        WD_LMS = 11,
        SAFETY_SUPERVISOR = 12,
        WD_SCS = 13,
        ALARM_WARNING_INDEX = 14,
        LOCK1_CURRENT_EXTRACT = 15,
        LOCK2_CURRENT_EXTRACT = 16,
        LOCK1_CURRENT_RETRACT = 17,
        LOCK2_CURRENT_RETRACT = 18,
        LOCK1_SEC_EXTRACT = 19,
        LOCK2_SEC_EXTRACT = 20,
        LOCK1_SEC_RETRACT = 21,
        LOCK2_SEC_RETRACT = 22,
        DIGITAL_OUTPUTS = 23,
        LOCK_UNLOCK = 24,
        READ_OK_CONSTANT = 254,
        NONE = 255,
    }


    public enum LOCK_UNLOCK_BITS
    {
        RUN_LOCK_LD1 = 2,
        RUN_LOCK_LD2 = 3,
        RUN_UNLOCK_LD1 = 4,
        RUN_UNLOCK_LD2 = 5,
    }

    public enum LOCK_UNLOCK_STATE
    {
        NONE = 0,
        LOCK = 1,
        UNLOCK = 2
    }



    public enum LINK_TO_AVG
    {
        WIND_AVG_SAMCA = 0,
        WIND_AVG_SBPT = 1,
        INC1_SLOPE_LAT_AVG_SBPT = 2,
        INC2_SLOPE_LAT_AVG_SBPT = 3,
        INC3_SLOPE_LAT_AVG_SBPT = 4,
        INC4_SLOPE_LAT_AVG_SBPT = 5,
        INC5_SLOPE_LAT_AVG_SBPT = 6,
        INC_TCU_SLOPE_LAT_AVG_SBPT = 7,
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


    public enum TRACKER_STATE
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

    public enum TRACKER_MODE
    {
        LMS_MODE = 0x0001,
        SCS_MODE = 0x0002,
        LMS_WATCHDOG_OK = 0x0004,
        SCS_WATCHDOG_OK = 0x0008,
        LMS_EMET_WATCHDOG_OK = 0x0010,
        SCS_EMET_WATCHDOG_OK = 0x0020,
    }

    public enum TRACKER_ALARM_DIAGNOSIS { 
        DIAGNOSIS_NOT_OK = 1,
        EMERGENCY_PUSH_BUTTON = 2,
        FORCED_OUTPUTS = 3,
        CONFIG_VALUE_OUT_OF_LIMITS = 4,
        CONFIG_CRC_INCORRECT_VALUE = 5,
        VOLTAGE_VALUE_OUT_OF_LIMITS = 6,
        POS_OUT_OF_SOFT_LIMITS_MD = 7,
        A0_RESERVA_7 = 8,
        CONSIGNAA_OUT_OF_SOFT_LIMITS = 9,
        A0_RESERVA_9 = 10,
        SUPREVISRO_POS_OUT_OF_LIMITS = 11,
        SUPERVISOR_TARGET_OUT_OF_LIMITS = 12,
        A0_RESERVA12 = 13,
        A0_RESERVA13 = 14,
        A0_RESERVA14 = 15,
        A0_RESERVA15 = 16, 

        PCB_TEMP_ERROR = 17,
        DRV_TEMP_ERRROR_MD = 18,
        DRV_TEMP_ERROR_LD = 19,
        DRIVE_AI_100MS_INST_MOTOR_CURRENT_ERROR_MD =20,
        DRIVE_AI_100MS_INST_MOTOR_CURRENT_ERROR_LOCK1 = 21,
        DRIVE_AI_100MS_INST_MOTOR_CURRENT_ERROR_LOCK2 = 22,
        DRIVE_AI_1SEC_AVG_MOTOR_CURRENT_ERROR_MD = 23,
        DRIVE_AI_1SEC_AVG_MOTOR_CURRENT_ERROR_LOCK1 = 24,
        DRIVE_AI_1SEC_AVG_MOTOR_CURRENT_ERROR_LOCK2 = 25,
        A1_RESERVA9 = 26,
        PS_24V_POWER_UP_ERROR = 27,
        SWITCH_ERROR_24V = 28,
        AI_RESERVA12 = 29,
        AI_RESERVA13 = 30,
        AI_RESERVA14 = 31,
        AI_RESERVA15 = 32,

        DRV_MD_FAULT = 33,
        DRV_LOCK1_FAULT = 34,
        DRV_LOCK2_FAULT = 35,
        UNEXPECTED_MOV_DETECTED_WHEN_STOPPED_MD = 36,
        DRIVE_NOT_MOVING_ERROR_MD = 37,
        DRIVE_MOVING_RESERVED_ERROR_MD = 38,
        INC1_CONNECTED_SIMULATION = 39,
        INC_DISCONNECTED = 40,
        INC_READING_ERROR = 41,
        A2_RESERVED9 = 42,
        A2_RESERVED10 = 43,
        LOCK1_DRIVE_LOCK_ERROR = 44,
        LOCK1_DRIVE_UNLOCK_ERROR = 45,
        LOCK2_DRIVE_LOCK_ERROR = 46,
        LOCK2_DRIVE_UNLOCK_ERROR = 47,
        UNEXPECTED_MAINTENANCE_LD_ERROR = 48,

        RESET_CONDITIONS_NOT_OK = 49,
        SFTY_SBPT_HDCAN_INC_IN_RANGES_FOR_ALARM_NOT_OK = 50,
        SFTY_SBPT_EXCESIVE_FORCE_KN_FOR_ALARM_NOT_OK = 51,
        A3_RESERVE3 = 52,
        A3_RESERVE4 = 53,
        A3_RESERVE5 = 54,
        A3_RESERVE6 = 55,
        A3_RESERVE7 = 56,
        A3_RESERVE8 = 57,
        A3_RESERVE9 = 58,
        A3_RESERVE10 = 59,
        A3_RESERVE11 = 60,
        A3_RESERVE12 = 61,
        A3_RESERVE13 = 62,
        A3_RESERVE14 = 63,
        A3_RESERVE15 = 64,

        EMERG0_CMD_STOW = 65,
        EMERG0_POWER_ON_PREVIOUS_STATE_NOT_OK = 66,
        EMERG0_UPS_PS_STATUS_NOT_OK = 67,
        EMERG0_RESERVA3 = 68,
        EMERG0_RESERVA4 = 69,
        EMERG0_RESERVA5 = 70,
        EMERG0_RESERVA6 = 71,
        EMERG0_RESERVA7 = 72,
        EMERG0_MODE_SAFETY_LMS1 = 73,
        EMERG0_RESERVA9 = 74,
        EMERG0_MODE_SAFETY_SCS1 = 75,
        EMERG0_RESERVA11 = 76,
        EMERG0_METEO_SAFETY_WD1 = 77,
        EMERG0_METEO_SAFETY_WIND_SPEED_GUST1 = 78,
        EMERG0_METEO_SAFETY_WIND_SPEED_MEAN1 = 79,
        EMERG0_METEO_SAFETY_TEMP1 = 80,

        EMERG1_SAFETY_SBPT_SLAVE_COM_ALL_NOT_OK = 81,
        EMERG1_SAFETY_SBPT_SCADA_EMERG_STOW_PB_NOT_OK = 82,
        EMERG1_SAFETY_SBPT_HDCAN_INC_IN_RANGES_FOR_EMERG_STOW_NOT_OK = 83,
        EMERG1_SAFETY_SBPT_EXCESIVE_FORCE_KN_FOR_EMERGENCY_STOW_OK = 84,
        EMERG1_RESERVA4 = 85,
        EMERG1_RESERVA5 = 86,
        EMERG1_RESERVA6 = 87,
        EMERG1_RESERVA7 = 88,
        EMERG1_RESERVA8 = 89,
        EMERG1_RESERVA9 = 90,
        EMERG1_RESERVA10 = 91,
        EMERG1_RESERVA11 = 92,
        EMERG1_RESERVA12 = 93,
        EMERG1_RESERVA13 = 94,
        EMERG1_RESERVA14 = 95,
        EMERG1_RESERVA15 = 96,

        EMERG2_RESERVA0 = 97,
        EMERG2_RESERVA1 = 98,
        EMERG2_RESERVA2 = 99,
        EMERG2_RESERVA3 = 100,
        EMERG2_RESERVA4 = 101,
        EMERG2_RESERVA5 = 102,
        EMERG2_RESERVA6 = 103,
        EMERG2_RESERVA7 = 104,
        EMERG2_RESERVA8 = 105,
        EMERG2_RESERVA9 = 106,
        EMERG2_RESERVA10 = 107,
        EMERG2_RESERVA11 = 108,
        EMERG2_RESERVA12 = 109,
        EMERG2_RESERVA13 = 110,
        EMERG2_RESERVA14 = 111,
        EMERG2_RESERVA15 = 112,

        EMERG3_RESERVA0 = 113,
        EMERG3_RESERVA1 = 114,
        EMERG3_RESERVA2 = 115,
        EMERG3_RESERVA3 = 116,
        EMERG3_RESERVA4 = 117,
        EMERG3_RESERVA5 = 118,
        EMERG3_RESERVA6 = 119,
        EMERG3_RESERVA7 = 120,
        EMERG3_RESERVA8 = 121,
        EMERG3_RESERVA9 = 122,
        EMERG3_RESERVA10 = 123,
        EMERG3_RESERVA11 = 124,
        EMERG3_RESERVA12 = 125,
        EMERG3_RESERVA13 = 126,
        EMERG3_RESERVA14 = 127,
        EMERG3_RESERVA15 = 128,
    }


    public enum DIGITAL_OUTPUT_BITS
    {
        ECU_ENABLE_12V_SETA =0,
        ECU_EXPS_ON = 1,
        ECU_EXPS_PRECARGA_ON = 2,
        ECU_MOTOR24V_ON = 3,
        ECU_MOTOR_24V_PRECARGA_ON = 4,
        ECU_DRV_ENABLE_MAIN = 5,
        ECU_DRV_ENABLE_LOCK1 = 6,
        ECU_DRV_ENABLE_LOCK2 = 7,
        ECU_DRV_DIR_MAIN = 8,
        ECU_DRV_DIR_LOCK1 = 9,
        ECU_DRV_DIR_LOCK2 = 10,
        ECU_DI_ENABLE_24V = 11,
        ECU_AI_ENABLE_24V = 12,
    }

    public enum FIELD_SAFETY_VALUE_BIT
    {
        AUTOTRACK_DISABLE = 0,
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



    public enum READ_WRITE_STATE : int {
        first_read_scs_finish = 0,
        first_read_tcu_finish = 1,
        first_read_samca_finish = 2,

        read_scs_modbus = 3, 
        read_write_tcu_modbus = 4,
        read_write_samca_modbus = 5,
    }
}
