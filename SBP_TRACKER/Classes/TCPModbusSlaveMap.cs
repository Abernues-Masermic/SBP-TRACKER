using CsvHelper.Configuration;

namespace SBP_TRACKER
{
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
            Map(m => m.Check_start_conn).Ignore();
        }
    }

    internal class TCPModbusVarMap : ClassMap<TCPModbusVar>
    {
        public TCPModbusVarMap()
        {
            AutoMap(System.Globalization.CultureInfo.CurrentCulture);

            Adjust_columns();
        }

        public void Adjust_columns()
        {
            Map(m => m.SType).Ignore();
            Map(m => m.Value).Ignore();
        }
    }
}
