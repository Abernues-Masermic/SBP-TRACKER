using System;
using System.Threading;


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

        public int Dir_ini { get; set; }

        public int Read_bytes { get; set; }


        public bool Check_start_conn { get; set; }



        public Manage_thread(string slave_name, string ip_address, int port, byte unitId, int dir_ini, int read_bytes) { 
            Slave_name = slave_name;
            IP_address = ip_address;
            Port = port;
            UnitId = unitId;
            Dir_ini = dir_ini;
            Read_bytes = read_bytes;

            ManageTCP = new Manage_tcp();
        }


        public void Start_tcp_com() {
            m_thread_start = new ThreadStart(Start_tcp_com_thread);
            m_thread = new Thread(m_thread_start);
            m_thread.SetApartmentState(ApartmentState.STA);
            m_thread.Start();
        }

        private void Start_tcp_com_thread() {
            if (ManageTCP.Connect(Slave_name, IP_address, Port, UnitId, Dir_ini))
                Manage_logs.SaveLogValue("START SLAVE -> " + Slave_name + " / " + IP_address + " / " + Port + " / " + UnitId + " / " + Dir_ini + " / " + Read_bytes);   
        }

        public void Stop_tcp_com_thread()
        {
            ManageTCP.Disconnect();
            Manage_logs.SaveLogValue("STOP SLAVE -> " + Slave_name);
        }


        public Tuple<bool, int[]> Read_holding_registers_int32()
        {
            return ManageTCP.Read_holding_registers_int32(Dir_ini, Read_bytes);
        }


        public bool Write_multiple_registers(int start_address, int[] values)
        {
            return ManageTCP.Write_multiple_registers(start_address, values);
        }

    }
}
