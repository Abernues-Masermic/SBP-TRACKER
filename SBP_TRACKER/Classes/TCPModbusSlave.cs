using CsvHelper.Configuration;
using System;
using System.Collections.Generic;


namespace SBP_TRACKER
{
    public class TCPModbusSlaveEntry
    {
        public string Name { get; set; }
        public string IP_primary { get; set; }

        public int Port { get; set; }

        public byte UnitId { get; set; }

        public ushort Dir_ini { get; set; }

        public ushort Read_reg { get; set; }

        public SLAVE_TYPE Slave_type { get; set; }

        public bool Fast_mode { get; set; }

        public bool Enable_communication { get; set; }

        public MODBUS_FUNCION Modbus_function { get; set; }

        public bool Connected { get; set; }

        public bool Reconnect { get; set; }

        public int Num_communication_error { get; set; }

        public List<TCPModbusVarEntry> List_var_entry {get; set;}
    }



    internal class TCPModbusSlaveMap : ClassMap<TCPModbusSlaveEntry>
    {
        public TCPModbusSlaveMap()
        {
            AutoMap(System.Globalization.CultureInfo.CurrentCulture);

            Adjust_columns();
        }

        public void Adjust_columns()
        {
            Map(m => m.Connected).Ignore();
            Map(m => m.Reconnect).Ignore();
            Map(m => m.Num_communication_error).Ignore();
        }
    }

}
