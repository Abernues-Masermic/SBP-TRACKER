using CsvHelper.Configuration;
using System;
using System.Collections.Generic;

namespace SBP_TRACKER
{
    public class SlopeCorrection
    {
        public double Alpha_TT { get; set; }
        public double Factor { get; set; }
    }


    internal class SlopeCorrectionMap : ClassMap<SlopeCorrection>
    {
        public SlopeCorrectionMap()
        {
            AutoMap(System.Globalization.CultureInfo.CurrentCulture);
        }
    }
}
