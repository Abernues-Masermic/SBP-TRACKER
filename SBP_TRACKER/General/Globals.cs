using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

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

        public List<TCPModbusSlaveEntry> List_slave_entry { get; set; }

        public List<TCUCodifiedStatusEntry> List_tcu_codified_status { get; set; }

        public List<TCUCommand> List_tcu_command { get; set; }

        public ConcurrentDictionary<double, double> Dictionary_slope_correction { get; set; }
        public List<double> List_slope_correction_alphaTT { get; set; }


        #region General

        public BIT_STATE Depur_enable { get; set; }
        public BIT_STATE TCU_depur { get; set; }
        public int Refresh_scada_interval { get; set; }

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
        public ushort Modbus_dir_tcu_command { get; set; }
        public ushort Modbus_dir_tcu_datetime { get; set; }
        public ushort Modbus_dir_write_samca { get; set; }


        public BIT_STATE Enable_write_tcu { get; set; }
        public BIT_STATE Enable_write_samca { get; set; }

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


        #region Mail / Cloud

        public BIT_STATE Mail_data_on { get; set; }
        public BIT_STATE Mail_alarm_on { get; set; }
        public string Mail_instant { get; set; }
        public string Mail_user { get; set; }
        public string Mail_from { get; set; }
        public string Mail_smtp_client { get; set; }
        public string Mail_pass { get; set; }

        public List<string> List_mail_to { get; set; }


        public BIT_STATE Load_cloud_on { get; set; }
        public int Cloud_check_interval { get; set; }
        public string Cloud_script { get; set; }

        public string Python_path { get; set; }

        #endregion


        #region WEB API

        public Manage_web_api ManageWebAPI { get; set; }

        public BIT_STATE Enable_web_api { get; set; }
        public decimal Tracker_ID { get; set; }
        public string Tracker_name { get; set; }
        public string API_root { get; set; }
        public string Data_controller_route { get; set; }
        public string State_controller_route { get; set; }
        public decimal Send_state_interval_web_API { get; set; }
        public decimal Send_data_interval_web_API { get; set; }
        public decimal Wait_error_conn_interval_web_API { get; set; }

        public decimal HTTP_timeout { get; set; }

        #endregion

    }
}
