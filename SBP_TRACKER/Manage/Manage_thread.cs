using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SBP_TRACKER
{
    internal class Manage_thread
    {

        private ThreadStart? m_thread_start;
        private Thread? m_thread;


        public Manage_tcp ManageTCP { get; set; }

        public string Slave_name { get; set; }

        public string IP_address { get; set; }
        public int Port { get; set; }

        public byte UnitId { get; set; }


        public bool Check_start_conn { get; set; }



        public Manage_thread(string slave_name, string ip_address, int port, byte unitId) { 
            Slave_name = slave_name;
            IP_address = ip_address;
            Port = port;
            UnitId = unitId;
        }


        public void Start_tcp_com() {
            m_thread_start = new ThreadStart(Start_tcp_com_thread);
            m_thread = new Thread(m_thread_start);
            m_thread.SetApartmentState(ApartmentState.STA);
            m_thread.Start();
        }

        private void Start_tcp_com_thread() {
            ManageTCP = new Manage_tcp();
            ManageTCP.Connect(Slave_name, IP_address, Port, UnitId);
        }

        public void Stop_tcp_com_thread()
        {
            ManageTCP.Disconnect();
        }

 
    }
}
