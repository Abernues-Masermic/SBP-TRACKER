using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public enum TCP_ACTION {
        CONNECT = 0,
        DISCONNECT = 1,
        REQUEST = 2,
        RESPONSE = 3,
        READ = 4,
        WRITE = 5,
        TIMEOUT = 6,
        ERROR_CONNECT = 7,
        ERROR_READ= 8

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

    public enum EMET_WATCHDOG_ASSOC { 
        NONE =0,
        WIN_SPEED = 1,
        WIN_DIRECTION = 2,
        TEMP = 3,
        RADIATION = 4,
        FIELD_SAFETY_SUPERVISOR = 5,
    }
}
