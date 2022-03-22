using System;
using System.Collections.Generic;


namespace SBP_TRACKER
{
    public class TCUCommand
    {
        public int Index { get; set; }

        public string? Name { get; set; }

        public int Num_params { get; set; }

        public List<string>? Name_params { get; set; }

        public List<TypeCode>? Type_params { get; set; }
    }
}
