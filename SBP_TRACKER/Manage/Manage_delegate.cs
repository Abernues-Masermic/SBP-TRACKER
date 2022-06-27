using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBP_TRACKER
{
    public class Manage_delegate
    {
        public delegate void TCP_handler(object myObject, TCP_handler_args myArgs);

        public event TCP_handler TCP_handler_event;

        public void Manage_tcp_to_main(string slave, TCP_ACTION tcp_action, List<ushort> list_data)
        {
            TCP_handler_args myArgs = new(slave, tcp_action, list_data);
            TCP_handler_event(this, myArgs);
        }
    }


    public class TCP_handler_args : EventArgs
    {
        public TCP_handler_args(string slave_name, TCP_ACTION tcp_action, List<ushort> list_data)
        {
            Slave_name = slave_name;
            TCP_action = tcp_action;
            List_data = list_data;
        }

        public string Slave_name { get; set; }

        public TCP_ACTION TCP_action { get; set; }

        public List<ushort> List_data { get; set; }
    }


}
