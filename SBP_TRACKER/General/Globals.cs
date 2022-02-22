using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBP_TRACKER
{
    internal class Globals
    {
        private static Globals? instance = null;
        public static Globals GetTheInstance()
        {
            instance ??= new Globals();

            return instance;
        }

        public Manage_delegate Manage_delegate { get; set; }


        public List<Manage_thread> List_manage_thread { get; set; }

        public List<TCPModbusSlaveEntry> List_modbus_slave_entry { get; set; }


        public BIT_STATE Depur_enable { get; set; }

        public int Record_data_interval { get; set; }

        public int Modbus_start_address{ get; set; }
        public int Modbus_read_interval { get; set; }
        public int Modbus_timeout { get; set; }

    }
}
