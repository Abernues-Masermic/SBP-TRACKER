﻿using System;
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
        private bool m_read_flag { get; set; }
        private bool m_write_flag { get; set; }


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

            if (!TCP_modbus_slave_entry.Reconnect)
                Manage_logs.SaveCommunicationValue($"START SLAVE -> {TCP_modbus_slave_entry.Name} / {TCP_modbus_slave_entry.IP_primary} / {TCP_modbus_slave_entry.Port} / {TCP_modbus_slave_entry.UnitId} /{TCP_modbus_slave_entry.Dir_ini} /  {TCP_modbus_slave_entry.Modbus_function} /  {TCP_modbus_slave_entry.Read_reg}");

            m_read_flag = false;
            m_write_flag = false;
            ManageTCP.Connect(TCP_modbus_slave_entry, m_recovery_mode);
        }

                

        public void Stop_tcp_com_thread()
        {
            ManageTCP.Disconnect();

            if (!TCP_modbus_slave_entry.Reconnect)
                Manage_logs.SaveCommunicationValue($"STOP SLAVE -> {TCP_modbus_slave_entry.Name}");
        }



        public Tuple<READ_STATE, int[]> Read_holding_registers_int32()
        {
            Tuple<READ_STATE, int[]> result = new(READ_STATE.ERROR, Array.Empty<int>());
            if (!m_read_flag)
            {
                m_read_flag = true;
                result = ManageTCP.Read_holding_registers_int32(TCP_modbus_slave_entry.Dir_ini, TCP_modbus_slave_entry.Read_reg);
                m_read_flag = false;
            }
            else if (Globals.GetTheInstance().Depur_enable == BIT_STATE.ON)
            {
                Manage_logs.SaveDepurValue($"READ HOLDING REGISTER FLAG ERROR : { TCP_modbus_slave_entry.Name }");
            }


            return result;
        }

        public Tuple<READ_STATE, int[]> Read_input_registers_int32()
        {
            Tuple<READ_STATE, int[]> result = new(READ_STATE.ERROR, Array.Empty<int>());
            if (!m_read_flag)
            {
                m_read_flag = true;
                result = ManageTCP.Read_input_registers_int32(TCP_modbus_slave_entry.Dir_ini, TCP_modbus_slave_entry.Read_reg);
                m_read_flag = false;
            }
            else if (Globals.GetTheInstance().Depur_enable == BIT_STATE.ON)
            {
                Manage_logs.SaveDepurValue($"READ INPUT REGISTER FLAG ERROR : { TCP_modbus_slave_entry.Name }");
            }
            
            return result;
        }


        public bool Write_multiple_registers(int start_address, int[] values)
        {
            bool write_ok = false;
            if (!m_write_flag)
            {
                m_write_flag = true;
                write_ok = ManageTCP.Write_multiple_registers(start_address, values);
                m_write_flag = false;
            }
            else if (Globals.GetTheInstance().Depur_enable == BIT_STATE.ON)
            {
                Manage_logs.SaveDepurValue($"WRITE MULTIPLE REGISTER FLAG ERROR : { TCP_modbus_slave_entry.Name }");
            }

            return write_ok;
        }

    }
}
