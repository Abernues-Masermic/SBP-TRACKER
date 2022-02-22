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


}
