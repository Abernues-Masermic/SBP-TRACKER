using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EasyModbus;

namespace SBP_TRACKER
{

    internal class Manage_tcp
    {
        private TCPModbusSlaveEntry _modbus_slave_entry;
        private ModbusClient? m_modbus_client;

        private readonly System.Timers.Timer m_timer_recovery_read_write;
        private DateTime m_date_error_start;
        public bool m_enable_read_write_reg;
        private bool b_connected = false;
        private bool m_recovery_mode { get; set; }




        #region Constructor

        public Manage_tcp()
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
                _modbus_slave_entry = modbus_slave_entry;

                m_modbus_client = new ModbusClient()
                {
                    IPAddress = modbus_slave_entry.IP_primary,
                    Port = modbus_slave_entry.Port,
                    UnitIdentifier = modbus_slave_entry.UnitId,
                    ConnectionTimeout = Globals.GetTheInstance().Modbus_conn_timeout
                };

                m_modbus_client.Connect();
                int[] read_data = m_modbus_client.ReadHoldingRegisters(_modbus_slave_entry.Dir_ini, _modbus_slave_entry.Read_reg);

                action = TCP_ACTION.CONNECT;
                m_enable_read_write_reg = true;
                b_connected = true;
                start_ok = true;
            }
            catch (Exception ex)
            {
                Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Connect)} -> {ex.Message} -> {_modbus_slave_entry.Name}");
                action = TCP_ACTION.ERROR_CONNECT;
            }


            if (recovery_mode && action == TCP_ACTION.ERROR_CONNECT)
                Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(_modbus_slave_entry.Name, TCP_ACTION.RECONNECT, new List<int>());

            else
                Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(_modbus_slave_entry.Name, action, new List<int>());

            return start_ok;
        }


        public bool Disconnect()
        {
            bool stop_ok = false;
            try
            {
                b_connected = false;

                if (m_modbus_client != null)
                    m_modbus_client.Disconnect();

                Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(_modbus_slave_entry.Name, TCP_ACTION.DISCONNECT, new List<int>());

                stop_ok = true;
            }
            catch { }

            return stop_ok;
        }


        public bool Is_connected()
        {
            bool is_connected = m_modbus_client != null ? m_modbus_client.Connected : false;

            return is_connected && m_modbus_client.Available(1000);
        }

        #endregion



        #region Timer recovery read write

        private void Timer_recovery_read_write_Tick(object sender, EventArgs e)
        {
            if (b_connected)
            {
                try
                {
                    int[] read_data = m_modbus_client.ReadHoldingRegisters(_modbus_slave_entry.Dir_ini, _modbus_slave_entry.Read_reg);
                    Manage_logs.SaveCommunicationValue($"COMMUNICATIONS RECOVERY OK  -> {_modbus_slave_entry.Name} / {_modbus_slave_entry.UnitId} / {_modbus_slave_entry.IP_primary}");
                    Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(_modbus_slave_entry.Name, TCP_ACTION.RECOVERY, new List<int>());
                    m_enable_read_write_reg = true;
                    m_timer_recovery_read_write.Stop();
                }
                catch
                {
                    if (DateTime.Now.Subtract(m_date_error_start) > TimeSpan.FromMilliseconds(Globals.GetTheInstance().Modbus_comm_timeout))
                    {
                        Manage_logs.SaveCommunicationValue($"TIMER COMMUNICATION WAIT FINISHED. TRY TO RECONNECT -> {_modbus_slave_entry.Name}");
                        Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(_modbus_slave_entry.Name, TCP_ACTION.RECONNECT, new List<int>());
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

        public Tuple<READ_STATE, int[]> Read_holding_registers_int32(int start_address, int num_bytes)
        {
            READ_STATE read_state = READ_STATE.WAIT;
            int[] received_data = Array.Empty<int>();
            if (b_connected && m_enable_read_write_reg)
            {
                TCP_ACTION action = TCP_ACTION.READ;

                string s_error = string.Empty;
                try
                {
                    if (Is_connected() && m_modbus_client != null)
                    {
                        received_data = m_modbus_client.ReadHoldingRegisters(start_address, num_bytes);
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

                    Manage_logs.SaveErrorValue($"{typeof(Manage_tcp).Name}  ->  {nameof(Read_holding_registers_int32)} -> {s_error}");

                    action = TCP_ACTION.ERROR_READ;
                    m_date_error_start = DateTime.Now;
                    m_timer_recovery_read_write.Start();
                }

                Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(_modbus_slave_entry.Name, action, received_data.ToList());
            }

            return Tuple.Create(read_state, received_data);
        }

        #endregion

        #region Read holding registers float

        public Tuple<READ_STATE, float> Read_holding_registers_float(int start_address, int num_bytes)
        {
            READ_STATE read_state = READ_STATE.WAIT;
            int[] i_received_data = Array.Empty<int>();
            float f_received_data = 0;
            if (b_connected && m_enable_read_write_reg)
            {
                TCP_ACTION action = TCP_ACTION.READ;

                try
                {
                    if (Is_connected() && m_modbus_client != null)
                    {
                        i_received_data = m_modbus_client.ReadHoldingRegisters(start_address, num_bytes);
                        f_received_data = ModbusClient.ConvertRegistersToFloat(i_received_data);
                        read_state = READ_STATE.OK;

                        Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(_modbus_slave_entry.Name, TCP_ACTION.READ, i_received_data.ToList());
                    }
                }
                catch (Exception ex)
                {
                    read_state = READ_STATE.ERROR;
                    m_enable_read_write_reg = false;

                    Manage_logs.SaveErrorValue($"{typeof(Manage_tcp).Name}  ->  {nameof(Read_holding_registers_float)} -> {ex.Message}");

                    action = TCP_ACTION.ERROR_READ;
                    m_date_error_start = DateTime.Now;
                    m_timer_recovery_read_write.Start();

                }

                Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(_modbus_slave_entry.Name, action, i_received_data.ToList());
            }

            return Tuple.Create(read_state, f_received_data);
        }

        #endregion

        #region Read input registers i32

        public Tuple<READ_STATE, int[]> Read_input_registers_int32(int start_address, int num_bytes)
        {
            READ_STATE read_state = READ_STATE.WAIT;
            int[] received_data = Array.Empty<int>();

            if (b_connected && m_enable_read_write_reg)
            {
                TCP_ACTION action = TCP_ACTION.READ;

                try
                {
                    if (Is_connected() && m_modbus_client != null)
                    {
                        received_data = m_modbus_client.ReadInputRegisters(start_address, num_bytes);
                        read_state = READ_STATE.OK;
                    }
                }
                catch (Exception ex)
                {
                    read_state=READ_STATE.ERROR;
                    m_enable_read_write_reg = false;

                    Manage_logs.SaveErrorValue($"{typeof(Manage_tcp).Name}  ->  {nameof(Read_input_registers_int32)} -> {ex.Message}");

                    action = TCP_ACTION.ERROR_READ;
                    m_date_error_start = DateTime.Now;
                    m_timer_recovery_read_write.Start();
                }

                Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(_modbus_slave_entry.Name, action, received_data.ToList());
            }
            return Tuple.Create(read_state, received_data);
        }

        #endregion


        #region Write single register

        public bool Write_single_registers(int start_address, int value)
        {
            bool write_ok = false;

            if (b_connected && m_enable_read_write_reg)
            {
                TCP_ACTION action = TCP_ACTION.WRITE;
                try
                {
                    if (Is_connected() && m_modbus_client != null)
                    {
                        m_modbus_client.WriteSingleRegister(start_address, value);
                        write_ok = true;
                    }
                }
                catch (Exception ex)
                {
                    m_enable_read_write_reg = false;

                    Manage_logs.SaveErrorValue($"{typeof(Manage_tcp).Name}  ->  {nameof(Write_single_registers)} -> {ex.Message}");

                    action = TCP_ACTION.ERROR_WRITE;
                    m_date_error_start = DateTime.Now;
                    m_timer_recovery_read_write.Start();
                }

                Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(_modbus_slave_entry.Name, action, new List<int>());
            }

            return write_ok;
        }

        #endregion

        #region Write multiple registers

        public bool Write_multiple_registers(int start_address, int[] values)
        {
            bool write_ok = false;
            if (b_connected && m_enable_read_write_reg)
            {
                TCP_ACTION action = TCP_ACTION.WRITE;
                try
                {
                    if (Is_connected() && m_modbus_client != null)
                    {
                        if (Globals.GetTheInstance().Depur_enable == BIT_STATE.ON)
                        {
                            string s_data = string.Empty;
                            values.ToList().ForEach(value => s_data += value.ToString() + " ");
                            Manage_logs.SaveDepurValue($"WRITE MULTIPLE REGISTERS : {start_address} - {s_data}");
                        }

                        m_modbus_client.WriteMultipleRegisters(start_address, values);
                        write_ok = true;
                    }
                }
                catch (Exception ex)
                {
                    if (Globals.GetTheInstance().Depur_enable == BIT_STATE.ON)
                        Manage_logs.SaveDepurValue($"WRITE MULTIPLE REGISTERS ERROR /{ex}");

                    m_enable_read_write_reg = false;

                    Manage_logs.SaveErrorValue($"{typeof(Manage_tcp).Name}  ->  {nameof(Write_multiple_registers)} -> {ex.Message}");

                    action = TCP_ACTION.ERROR_WRITE;
                    m_date_error_start = DateTime.Now;
                    m_timer_recovery_read_write.Start();
                }
                Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(_modbus_slave_entry.Name, action, new List<int>());
            }
            else
            {
                if (Globals.GetTheInstance().Depur_enable == BIT_STATE.ON)
                    Manage_logs.SaveDepurValue($"WRITE MULTIPLE REGISTERS ERROR / CONNECTED - {b_connected} / ENABLE READ WRITE - {m_enable_read_write_reg}");
            }

            return write_ok;
        }

        #endregion

        #endregion
    }
}
