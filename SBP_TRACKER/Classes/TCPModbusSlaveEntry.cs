using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBP_TRACKER
{
    public class TCPModbusSlaveEntry
    {
        public string Name { get; set; }
        public string IP_primary { get; set; }

        public int Port { get; set; }

        public byte UnitId { get; set; }

        public bool Connected { get; set; }

        public bool Check_start_conn { get; set; }


        public List<TCPModbusVar> List_modbus_var {get; set;}
    }

    public class TCPModbusVar {

        public string Slave { get; set; }
        public string Name { get; set; }
        public int Dir { get; set; }

        public TypeCode Type { get; set; }

        public int Schema_pos { get; set; }

        public string SType { get; set; }

        public string Value { get; set; }
    }
}
