using CsvHelper.Configuration;
using System;


namespace SBP_TRACKER
{
    public class TCPModbusVarEntry
    {
        public string? Slave { get; set; }
        public string? Name { get; set; }

        public string? Description { get; set; }

        public int DirModbus { get; set; }

        public TypeCode TypeVar { get; set; }

        public string? Unit { get; set; }

        public int Watchdog_assoc { get; set; }

        public int Schema_pos { get; set; }



        public int Read_range_min { get; set; }
        public int Read_range_max { get; set; }


        public double Scaled_range_min { get; set; }
        public double Scaled_range_max { get; set; }




        #region NOT SAVE FIELDS

        public double Scale_factor { get; set; }
        public string? Read_range_grid { get; set; }
        public string? Scaled_range_grid { get; set; }
        public string? Value { get; set; }

        #endregion


    }


    internal class TCPModbusVarMap : ClassMap<TCPModbusVarEntry>
    {
        public TCPModbusVarMap()
        {
            AutoMap(System.Globalization.CultureInfo.CurrentCulture);

            Adjust_columns();
        }

        public void Adjust_columns()
        {
            Map(m => m.Scale_factor).Ignore();
            Map(m => m.Read_range_grid).Ignore();
            Map(m => m.Scaled_range_grid).Ignore();
            Map(m => m.Value).Ignore();
        }
    }
}
