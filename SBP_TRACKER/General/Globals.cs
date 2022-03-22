using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBP_TRACKER
{
    internal class Globals
    {
        private static Globals? instance = null;
        public static Globals GetTheInstance()
        {
            instance ??= new Globals();

            return instance;
        }

        public Manage_delegate Manage_delegate { get; set; }


        public List<Manage_thread> List_manage_thread { get; set; }

        public List<TCPModbusSlaveEntry> List_modbus_slave_entry { get; set; }

        public List<TCUCodifiedStatusEntry> List_tcu_codified_status { get; set; }

        public List<TCUCommand> List_tcu_command { get; set; }


        #region General

        public BIT_STATE Depur_enable { get; set; }

        #endregion

        #region Modbus

        public int Modbus_read_met_interval { get; set; }
        public int Modbus_read_tcu_interval { get; set; }
        public int Modbus_write_tcu_interval { get; set; }
        public int Modbus_timeout { get; set; }

        public int Modbus_dir_scs_command { get; set; }


        #endregion

        #region Record

        public int Record_data_met1_interval { get; set; }
        public int Record_data_met2_interval { get; set; }

        public int Record_data_tcu_interval { get; set; }


        public DECIMAL_SEP Decimal_sep { get; set; }
        public FIELD_SEP Field_sep { get; set; }
        public string SField_sep { get; set; }

        public NumberFormatInfo nfi { get; set; }
        public string Date_format { get; set; }
        public string Format_provider { get; set; }

        #endregion


        #region Mail

        public BIT_STATE Mail_on { get; set; }
        public String Mail_instant { get; set; }
        public String Mail_user { get; set; }
        public String Mail_from { get; set; }
        public String Mail_smtp_client { get; set; }
        public String Mail_pass { get; set; }

        public List<string> List_mail_to { get; set; }

        #endregion

    }
}
