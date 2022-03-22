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

        public int Dir_ini { get; set; }

        public int Read_bytes { get; set; }

        public bool TCU { get; set; }

        public bool Connected { get; set; }

        public List<TCPModbusVarEntry> List_modbus_var {get; set;}
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
        }
    }

}
