using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SBP_TRACKER
{
    public class LinkToSendTCUClass
    {
        public LINK_TO_SEND_TCU Link_to_send_tcu { get; set; }

        public dynamic Value { get; set; }
        public string Unit { get; set; }
        public Label Label_tcu_mode_value { get; set; }
        public Label Label_graphic_mode_value { get; set; }
    }
}
