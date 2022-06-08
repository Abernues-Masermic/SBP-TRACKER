using CsvHelper.Configuration;
using System;
using System.Collections.Generic;

namespace SBP_TRACKER
{
    public class TCUCodifiedStatusEntry
    {
        public int DirModbus { get; set; }

        public string Name { get; set; }

        public TypeCode TypeVar { get; set; }

        public double Factor { get; set; }

        public string Unit { get; set; }

        public bool TCU_record { get; set; }

        public bool SCS_record { get; set; }

        public bool Status_mask_enable { get; set; }

        public int Link_to_graphic { get; set; }

        public string Send_to_samca_pos { get; set; }

        public List<string> List_status_mask { get; set; }

        public string Value { get; set; }
    }


    internal class TCUCodifiedStatusMap : ClassMap<TCUCodifiedStatusEntry>
    {
        public TCUCodifiedStatusMap()
        {
            AutoMap(System.Globalization.CultureInfo.CurrentCulture);

            Adjust_columns();
        }


        public void Adjust_columns()
        {
            Map(m => m.List_status_mask).Ignore();
            Map(m => m.Value).Ignore();
        }
    }
}
