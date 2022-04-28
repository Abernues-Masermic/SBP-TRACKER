using System;
using System.Threading;


namespace SBP_TRACKER
{
    internal class Manage_thread
    {

        private ThreadStart? m_thread_start;
        private Thread? m_thread;


        public TCPModbusSlaveEntry TCP_modbus_slave_entry { get; set; }

        public Manage_tcp ManageTCP { get; set; }

        public bool Check_start_conn { get; set; }

        private bool m_recovery_mode;


        public Manage_thread() { 
            ManageTCP = new Manage_tcp();
        }


        public void Start_tcp_com(bool recovery_mode) {

            m_recovery_mode = recovery_mode;

            m_thread_start = new ThreadStart(Start_tcp_com_thread);
            m_thread = new Thread(m_thread_start);
            m_thread.SetApartmentState(ApartmentState.STA);
            m_thread.Start();
        }

        private void Start_tcp_com_thread() {
            if (ManageTCP.Connect(TCP_modbus_slave_entry, m_recovery_mode))
                Manage_logs.SaveLogValue($"START SLAVE -> {TCP_modbus_slave_entry.Name} / {TCP_modbus_slave_entry.IP_primary} / {TCP_modbus_slave_entry.Port} / {TCP_modbus_slave_entry.UnitId} /{TCP_modbus_slave_entry.Dir_ini} /  {TCP_modbus_slave_entry.Modbus_function} /  {TCP_modbus_slave_entry.Read_reg}");   
        }

        public void Stop_tcp_com_thread()
        {
            ManageTCP.Disconnect();
            Manage_logs.SaveLogValue($"STOP SLAVE -> {TCP_modbus_slave_entry.Name}");
        }


        public Tuple<READ_STATE, int[]> Read_holding_registers_int32()
        {
            return ManageTCP.Read_holding_registers_int32(TCP_modbus_slave_entry.Dir_ini, TCP_modbus_slave_entry.Read_reg);
        }

        public Tuple<READ_STATE, int[]> Read_input_registers_int32()
        {
            return ManageTCP.Read_input_registers_int32(TCP_modbus_slave_entry.Dir_ini, TCP_modbus_slave_entry.Read_reg);
        }


        public bool Write_multiple_registers(int start_address, int[] values)
        {
            return ManageTCP.Write_multiple_registers(start_address, values);
        }

    }
}
