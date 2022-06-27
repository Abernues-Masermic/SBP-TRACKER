using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NModbus;
using System.Net.Sockets;

namespace SBP_TRACKER
{

    internal class Manage_modbus
    {
        private TCPModbusSlaveEntry m_modbus_slave_entry;

        private TcpClient m_tcp_client;
        private IModbusMaster? master;

        private readonly System.Timers.Timer m_timer_recovery_read_write;
        private DateTime m_date_error_start;
        public bool m_enable_read_write_reg;
        private bool b_connected = false;
        private bool m_recovery_mode { get; set; }




        #region Constructor

        public Manage_modbus()
        {
            m_timer_recovery_read_write = new System.Timers.Timer();
            m_timer_recovery_read_write.Elapsed += Timer_recovery_read_write_Tick;
            m_timer_recovery_read_write.Interval = Globals.GetTheInstance().Modbus_conn_timeout;
            m_timer_recovery_read_write.Stop();
        }

        #endregion


        #region Connect / disconnect

        public bool Connect(TCPModbusSlaveEntry modbus_slave_entry, bool recovery_mode)
        {
            bool start_ok = false;

            TCP_ACTION action = TCP_ACTION.CONNECT;
            try
            {
                m_modbus_slave_entry = modbus_slave_entry;

                m_tcp_client = new TcpClient(modbus_slave_entry.IP_primary, modbus_slave_entry.Port);
                m_tcp_client.ReceiveTimeout = Globals.GetTheInstance().Modbus_conn_timeout;
               
                var factory = new ModbusFactory();
                master = factory.CreateMaster(m_tcp_client);

                ushort[] read_data = master.ReadHoldingRegisters(m_modbus_slave_entry.UnitId, m_modbus_slave_entry.Dir_ini, m_modbus_slave_entry.Read_reg);
                action = TCP_ACTION.CONNECT;
                m_enable_read_write_reg = true;
                b_connected = true;
                start_ok = true;
            }
            catch (Exception ex)
            {
                Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Connect)} -> {ex.Message} -> {m_modbus_slave_entry.Name}");
                action = TCP_ACTION.ERROR_CONNECT;
            }


            if (recovery_mode && action == TCP_ACTION.ERROR_CONNECT)
                Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(m_modbus_slave_entry.Name, TCP_ACTION.RECONNECT, new List<ushort>());

            else
                Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(m_modbus_slave_entry.Name, action, new List<ushort>());

            return start_ok;
        }


        public bool Disconnect()
        {
            bool stop_ok = false;
            try
            {
                b_connected = false;

                if (master != null)
                {
                    m_tcp_client.Close();
                    m_tcp_client.Dispose();
                    master.Dispose();
                }

                Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(m_modbus_slave_entry.Name, TCP_ACTION.DISCONNECT, new List<ushort>());

                stop_ok = true;
            }
            catch { }

            return stop_ok;
        }


        public bool Is_connected()
        {
            bool is_connected = master != null && m_tcp_client.Connected;
            return is_connected ;
        }

        #endregion



        #region Timer recovery read write

        private void Timer_recovery_read_write_Tick(object sender, EventArgs e)
        {
            if (b_connected)
            {
                try
                {
                    ushort[] read_data = master.ReadHoldingRegisters(m_modbus_slave_entry.UnitId, m_modbus_slave_entry.Dir_ini, m_modbus_slave_entry.Read_reg);

                    Manage_logs.SaveCommunicationValue($"COMMUNICATIONS RECOVERY OK  -> {m_modbus_slave_entry.Name} / {m_modbus_slave_entry.UnitId} / {m_modbus_slave_entry.IP_primary}");
                    Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(m_modbus_slave_entry.Name, TCP_ACTION.RECOVERY, new List<ushort>());
                    m_enable_read_write_reg = true;
                    m_timer_recovery_read_write.Stop();
                }
                catch
                {
                    if (DateTime.Now.Subtract(m_date_error_start) > TimeSpan.FromMilliseconds(Globals.GetTheInstance().Modbus_comm_timeout))
                    {
                        Manage_logs.SaveCommunicationValue($"TIMER COMMUNICATION WAIT FINISHED. TRY TO RECONNECT -> {m_modbus_slave_entry.Name}");
                        Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(m_modbus_slave_entry.Name, TCP_ACTION.RECONNECT, new List<ushort>());
                        m_timer_recovery_read_write.Stop();
                    }
                }
            }
            else
                m_timer_recovery_read_write.Stop();
        }

        #endregion




        #region Modbus general functions

        #region Read holding registers i32

        public Tuple<READ_STATE, ushort[]> Read_holding_registers(ushort start_address, ushort num_bytes)
        {
            READ_STATE read_state = READ_STATE.WAIT;
            ushort[] received_data = Array.Empty<ushort>();
            if (b_connected && m_enable_read_write_reg)
            {
                TCP_ACTION action = TCP_ACTION.READ;

                string s_error = string.Empty;
                try
                {
                    if (Is_connected() )
                    {
                        received_data = master.ReadHoldingRegisters(m_modbus_slave_entry.UnitId, start_address, num_bytes);
                        read_state = READ_STATE.OK;
                    }

                }
                catch (IOException ex)
                {
                    s_error = ex.Message;
                    read_state = READ_STATE.ERROR;
                }
                catch (Exception ex)
                {
                    s_error = ex.Message;
                    read_state = READ_STATE.ERROR;
                }

                if (read_state != READ_STATE.OK)
                {
                    m_enable_read_write_reg = false;

                    Manage_logs.SaveErrorValue($"{typeof(Manage_modbus).Name}  ->  {nameof(Read_holding_registers)} -> {s_error}");

                    action = TCP_ACTION.ERROR_READ;
                    m_date_error_start = DateTime.Now;
                    m_timer_recovery_read_write.Start();
                }

                Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(m_modbus_slave_entry.Name, action, received_data.ToList());
            }

            return Tuple.Create(read_state, received_data);
        }

        #endregion

        #region Read input registers i32

        public Tuple<READ_STATE, ushort[]> Read_input_registers_int32(ushort start_address, ushort num_bytes)
        {
            READ_STATE read_state = READ_STATE.WAIT;
            ushort[] received_data = Array.Empty<ushort>();

            if (b_connected && m_enable_read_write_reg)
            {
                TCP_ACTION action = TCP_ACTION.READ;

                try
                {
                    if (Is_connected() )
                    {
                        received_data = master.ReadInputRegisters(m_modbus_slave_entry.UnitId, start_address, num_bytes);
                        read_state = READ_STATE.OK;
                    }
                }
                catch (Exception ex)
                {
                    read_state=READ_STATE.ERROR;
                    m_enable_read_write_reg = false;

                    Manage_logs.SaveErrorValue($"{typeof(Manage_modbus).Name} -> {nameof(Read_input_registers_int32)} -> {ex.Message}");

                    action = TCP_ACTION.ERROR_READ;
                    m_date_error_start = DateTime.Now;
                    m_timer_recovery_read_write.Start();
                }

                Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(m_modbus_slave_entry.Name, action, received_data.ToList());
            }
            return Tuple.Create(read_state, received_data);
        }

        #endregion

        #region Write single register

        public bool Write_single_registers(ushort start_address, ushort value)
        {
            bool write_ok = false;

            if (b_connected && m_enable_read_write_reg)
            {
                TCP_ACTION action = TCP_ACTION.WRITE;
                try
                {
                    if (Is_connected() )
                    {
                        master.WriteSingleRegister(m_modbus_slave_entry.UnitId, start_address, value);
                        write_ok = true;
                    }
                }
                catch (Exception ex)
                {
                    m_enable_read_write_reg = false;

                    Manage_logs.SaveErrorValue($"{typeof(Manage_modbus).Name} -> {nameof(Write_single_registers)} -> {ex.Message}");

                    action = TCP_ACTION.ERROR_WRITE;
                    m_date_error_start = DateTime.Now;
                    m_timer_recovery_read_write.Start();
                }

                Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(m_modbus_slave_entry.Name, action, new List<ushort>());
            }

            return write_ok;
        }

        #endregion

        #region Write multiple registers

        public bool Write_multiple_registers(ushort start_address, ushort[] values)
        {
            bool write_ok = false;
            if (b_connected && m_enable_read_write_reg)
            {
                TCP_ACTION action = TCP_ACTION.WRITE;
                try
                {
                    if (Is_connected())
                    {
                        string s_data = string.Empty;
                        values.ToList().ForEach(value => s_data += value.ToString() + " ");
                        Manage_logs.SaveDepurValue($"WRITE MULTIPLE REGISTERS : {start_address} - {s_data}");

                        master.WriteMultipleRegisters(m_modbus_slave_entry.UnitId, start_address, values);
                        write_ok = true;
                    }
                }
                catch (Exception ex)
                {
                    Manage_logs.SaveDepurValue($"WRITE MULTIPLE REGISTERS ERROR / {ex}");

                    m_enable_read_write_reg = false;

                    Manage_logs.SaveErrorValue($"{typeof(Manage_modbus).Name} -> {nameof(Write_multiple_registers)} -> {ex.Message}");

                    action = TCP_ACTION.ERROR_WRITE;
                    m_date_error_start = DateTime.Now;
                    m_timer_recovery_read_write.Start();
                }
                Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(m_modbus_slave_entry.Name, action, new List<ushort>());
            }

            else
                Manage_logs.SaveDepurValue($"WRITE MULTIPLE REGISTERS ERROR / CONNECTED - {b_connected} / ENABLE READ WRITE - {m_enable_read_write_reg}");
            

            return write_ok;
        }

        #endregion

        #endregion
    }
}
