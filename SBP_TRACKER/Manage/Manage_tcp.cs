using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyModbus;

namespace SBP_TRACKER
{

    internal class Manage_tcp
    {
        private ModbusClient? m_modbus_client;


        public string Slave_name {get; set;}

        public bool Connect(string slave_name,string ip_address, int port, byte slave_address)
        {
            bool start_ok = false;

            TCP_ACTION action = TCP_ACTION.CONNECT;
            try
            {
                Slave_name = slave_name;

                m_modbus_client = new ModbusClient()
                {
                    IPAddress = ip_address,
                    Port = port,
                    UnitIdentifier = slave_address,
                    ConnectionTimeout = Globals.GetTheInstance().Modbus_timeout
                };

                m_modbus_client.Connect();

                m_modbus_client.ReadHoldingRegisters(Globals.GetTheInstance().Modbus_start_address, 0);

                start_ok = true;       
            }
            catch 
            {
                action = TCP_ACTION.ERROR_CONNECT;
            }

            Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(slave_name, action, new List<int>());

            return start_ok;
        }


        public bool Disconnect() { 
            bool stop_ok = false;
            try
            {
                if (m_modbus_client != null)
                    m_modbus_client.Disconnect();

                Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(Slave_name, TCP_ACTION.DISCONNECT, new List<int>());

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




        #region Modbus general functions

        #region Read holding registers i32

        public Tuple<bool, int[]> Read_holding_registers_int32(int start_address, int number_off_registers)
        {
            bool  read_ok = false;
            int[] received_data = Array.Empty<int>();

            TCP_ACTION action = TCP_ACTION.READ;

            try
            {
                if (Is_connected())
                {
                    received_data = m_modbus_client.ReadHoldingRegisters(start_address, Constants.MAX_MODBUS_REG);
                    read_ok = true;
                }
            }
            catch
            {
                action = TCP_ACTION.ERROR_READ;
            }

            Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(Slave_name, action, received_data.ToList());

            return Tuple.Create(read_ok, received_data);
        }

        #endregion

        #region Read holding registers float

        public Tuple<bool, float> Read_holding_registers_float(int start_address, int number_off_registers)
        {
            bool read_ok = false;
            float f_received_data = 0;

            try
            {
                int[] i_received_data = m_modbus_client.ReadHoldingRegisters(start_address, number_off_registers);
                f_received_data = ModbusClient.ConvertRegistersToFloat(i_received_data);
                read_ok = true;

                Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(Slave_name, TCP_ACTION.READ, i_received_data.ToList());
            }
            catch { }

            return Tuple.Create(read_ok, f_received_data);
        }

        #endregion

        #region Write single register

        public bool Write_single_registers(int start_address, int value)
        {
            bool write_ok = false;
            try
            {
                m_modbus_client.WriteSingleRegister(start_address, value);
                write_ok = true;

                Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(Slave_name, TCP_ACTION.WRITE, new List<int>());
            }
            catch { }

            return write_ok;
        }

        #endregion

        #region Write multiple registers

        public bool Write_multiple_registers(int start_address, int[] values)
        {
            bool write_ok = false;
            try
            {
                m_modbus_client.WriteMultipleRegisters(start_address, values);
                write_ok = true;

                Globals.GetTheInstance().Manage_delegate.Manage_tcp_to_main(Slave_name, TCP_ACTION.WRITE, new List<int>());
            }
            catch { }

            return write_ok;
        }

        #endregion

        #endregion
    }
}
