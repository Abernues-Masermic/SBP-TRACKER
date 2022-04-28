using System;
using System.Collections.Concurrent;
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

        public ConcurrentDictionary<double, double> Dictionary_slope_correction { get; set; }
        public List<double> List_slope_correction_alphaTT { get; set; }

        #region General

        public BIT_STATE Depur_enable { get; set; }

        #endregion

        #region Modbus

        public int Modbus_read_scs_normal_interval { get; set; }
        public int Modbus_read_scs_fast_interval { get; set; }
        public int Modbus_read_tcu_interval { get; set; }
        public int Modbus_write_tcu_watchdog_interval { get; set; }
        public int Modbus_write_tcu_datetime_interval { get; set; }
        public int Modbus_write_samca_interval { get; set; }
        public int Modbus_conn_timeout { get; set; }
        public int Modbus_comm_timeout { get; set; }
        public int Modbus_reconnect_interval { get; set; }
        public int Modbus_dir_scs_command { get; set; }

        public int Modbus_dir_tcu_datetime { get; set; }

        #endregion

        #region Record

        public int Record_scs_normal_interval { get; set; }
        public int Record_scs_fast_interval { get; set; }

        public int Record_tcu_interval { get; set; }

        public int Record_samca_interval { get; set; }

        public DECIMAL_SEP Decimal_sep { get; set; }
        public FIELD_SEP Field_sep { get; set; }
        public string SField_sep { get; set; }

        public NumberFormatInfo nfi { get; set; }
        public string Date_format { get; set; }
        public string Format_provider { get; set; }

        #endregion

        #region Wind config

        public double SBPT_trigger_3sec { get; set; }
        public double SBPT_trigger_10min { get; set; }
        public int SBPT_wind_delay_time_3sec { get; set; }
        public double SBPT_low_hist_10min { get; set; }

        public double SAMCA_trigger_3sec { get; set; }
        public double SAMCA_trigger_10min { get; set; }
        public int SAMCA_wind_delay_time_3sec { get; set; }
        public double SAMCA_low_hist_10min { get; set; }

        #endregion


        #region INC config

        public int SBPT_inc_avg_interval { get; set; }
        public double Max_diff_tcu_inc_emergency_stow { get; set; }
        public double Max_diff_tcu_inc_alarm { get; set; }

        #endregion



        #region DYN config

        public int SBPT_dyn_avg_interval { get; set; }

        public int SBPT_dyn_max_mov_emerg_stow{ get; set; }

        public int SBPT_dyn_max_mov_alarm { get; set; }

        public int SBPT_dyn_max_static_alarm { get; set; }

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
