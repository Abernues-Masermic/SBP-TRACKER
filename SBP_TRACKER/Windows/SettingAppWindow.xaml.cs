using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CsvHelper;
using CsvHelper.Configuration;


namespace SBP_TRACKER
{

    public partial class SettingAppWindow : Window
    {

        #region Constructor

        public SettingAppWindow()
        {
            InitializeComponent();

            Combobox_provider.ItemsSource = CultureInfo.GetCultures(CultureTypes.AllCultures);
        }

        #endregion


        #region Loaded

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //MODBUS
            DecimalUpDown_modbus_read_scs_normal.Value = Globals.GetTheInstance().Modbus_read_scs_normal_interval;
            DecimalUpDown_modbus_read_scs_fast.Value = Globals.GetTheInstance().Modbus_read_scs_fast_interval;
            DecimalUpDown_modbus_read_tcu.Value = Globals.GetTheInstance().Modbus_read_tcu_interval;
            DecimalUpDown_modbus_write_tcu_watchdog.Value = Globals.GetTheInstance().Modbus_write_tcu_watchdog_interval;
            DecimalUpDown_modbus_write_tcu_datetime.Value = Globals.GetTheInstance().Modbus_write_tcu_datetime_interval;
            DecimalUpDown_modbus_write_samca.Value = Globals.GetTheInstance().Modbus_write_samca_interval;
            DecimalUpDown_modbus_conn_timeout.Value = Globals.GetTheInstance().Modbus_conn_timeout;
            DecimalUpDown_modbus_comm_timeout.Value = Globals.GetTheInstance().Modbus_comm_timeout;
            DecimalUpDown_modbus_reconnect.Value = Globals.GetTheInstance().Modbus_reconnect_interval;
            DecimalUpDown_modbus_dir_tcu_command.Value = Globals.GetTheInstance().Modbus_dir_tcu_command;
            DecimalUpDown_modbus_dir_tcu_datetime.Value = Globals.GetTheInstance().Modbus_dir_tcu_datetime;
            DecimalUpDown_modbus_dir_wr_samca.Value = Globals.GetTheInstance().Modbus_dir_write_samca;
            
            DecimalUpDown_refresh_scada.Value = Globals.GetTheInstance().Refresh_scada_interval;

            //RECORD
            DecimalUpDown_record_scs_normal.Value = Globals.GetTheInstance().Record_scs_normal_interval;
            DecimalUpDown_record_scs_fast.Value = Globals.GetTheInstance().Record_scs_fast_interval;
            DecimalUpDown_record_tcu.Value = Globals.GetTheInstance().Record_tcu_interval;
            DecimalUpDown_record_samca.Value = Globals.GetTheInstance().Record_samca_interval;

            Combobox_decimal_sep.SelectedIndex = (int) Globals.GetTheInstance().Decimal_sep;
            Combobox_field_sep.SelectedIndex = (int) Globals.GetTheInstance().Field_sep;

            Textbox_date_format.Text = Globals.GetTheInstance().Date_format;
            Combobox_provider.Text = Globals.GetTheInstance().Format_provider;

            //WIND CONFIG
            DecimalUpDown_sbpt_3sec_trigger.Value = (decimal) Globals.GetTheInstance().SBPT_trigger_3sec;
            DecimalUpDown_sbpt_10min_trigger.Value = (decimal)Globals.GetTheInstance().SBPT_trigger_10min;
            DecimalUpDown_sbpt_wind_delay_3sec.Value = Globals.GetTheInstance().SBPT_wind_delay_time_3sec;
            DecimalUpDown_sbpt_low_hist_10min.Value = (decimal)Globals.GetTheInstance().SBPT_low_hist_10min;

            DecimalUpDown_samca_3sec_trigger.Value = (decimal)Globals.GetTheInstance().SAMCA_trigger_3sec;
            DecimalUpDown_samca_10min_trigger.Value = (decimal)Globals.GetTheInstance().SAMCA_trigger_10min;
            DecimalUpDown_samca_wind_delay_3sec.Value = Globals.GetTheInstance().SAMCA_wind_delay_time_3sec;
            DecimalUpDown_samca_low_hist_10min.Value = (decimal)Globals.GetTheInstance().SAMCA_low_hist_10min;

            //INC CONFIG
            DecimalUpDown_sbpt_inc_avg_interval.Value = Globals.GetTheInstance().SBPT_inc_avg_interval;
            DecimalUpDown_inc_max_diff_emerg_stow.Value = (decimal)Globals.GetTheInstance().Max_diff_tcu_inc_emergency_stow;
            DecimalUpDown_inc_max_diff_alarm.Value = (decimal)Globals.GetTheInstance().Max_diff_tcu_inc_alarm;

            //DYN CONFIG
            DecimalUpDown_sbpt_dyn_avg_interval.Value = Globals.GetTheInstance().SBPT_dyn_avg_interval;
            DecimalUpDown_sbpt_dyn_max_moving_emergency_stow.Value = Globals.GetTheInstance().SBPT_dyn_max_mov_emerg_stow;
            DecimalUpDown_sbpt_dyn_max_moving_alarm.Value = Globals.GetTheInstance().SBPT_dyn_max_mov_alarm;
            DecimalUpDown_sbpt_dyn_max_static_alarm.Value = Globals.GetTheInstance().SBPT_dyn_max_static_alarm;
        }

        #endregion


        #region Mover pantalla

        private void Control_MouseDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        #endregion


        #region Button save

        private void Button_save_Click(object sender, RoutedEventArgs e)
        {
            //MODBUS
            Globals.GetTheInstance().Modbus_read_scs_normal_interval = (int)DecimalUpDown_modbus_read_scs_normal.Value;
            Globals.GetTheInstance().Modbus_read_scs_fast_interval = (int)DecimalUpDown_modbus_read_scs_fast.Value;
            Globals.GetTheInstance().Modbus_read_tcu_interval = (int)DecimalUpDown_modbus_read_tcu.Value;
            Globals.GetTheInstance().Modbus_write_tcu_watchdog_interval = (int)DecimalUpDown_modbus_write_tcu_watchdog.Value;
            Globals.GetTheInstance().Modbus_write_tcu_datetime_interval = (int)DecimalUpDown_modbus_write_tcu_datetime.Value;
            Globals.GetTheInstance().Modbus_write_samca_interval = (int)DecimalUpDown_modbus_write_samca.Value;
            Globals.GetTheInstance().Modbus_conn_timeout = (int)DecimalUpDown_modbus_conn_timeout.Value;
            Globals.GetTheInstance().Modbus_comm_timeout = (int)DecimalUpDown_modbus_comm_timeout.Value;
            Globals.GetTheInstance().Modbus_reconnect_interval = (int)DecimalUpDown_modbus_reconnect.Value;
            Globals.GetTheInstance().Modbus_dir_tcu_command = (int)DecimalUpDown_modbus_dir_tcu_command.Value;
            Globals.GetTheInstance().Modbus_dir_tcu_datetime = (int)DecimalUpDown_modbus_dir_tcu_datetime.Value;
            Globals.GetTheInstance().Modbus_dir_write_samca = (int)DecimalUpDown_modbus_dir_wr_samca.Value;

            Globals.GetTheInstance().Refresh_scada_interval = (int)DecimalUpDown_refresh_scada.Value;

            //RECORD
            Globals.GetTheInstance().Record_scs_normal_interval = (int)DecimalUpDown_record_scs_normal.Value;
            Globals.GetTheInstance().Record_scs_fast_interval = (int)DecimalUpDown_record_scs_fast.Value;
            Globals.GetTheInstance().Record_tcu_interval = (int)DecimalUpDown_record_tcu.Value;
            Globals.GetTheInstance().Record_samca_interval = (int)DecimalUpDown_record_samca.Value;

            Globals.GetTheInstance().Decimal_sep = (DECIMAL_SEP)Combobox_decimal_sep.SelectedIndex;
            Globals.GetTheInstance().Field_sep = (FIELD_SEP)Combobox_field_sep.SelectedIndex;
            Globals.GetTheInstance().SField_sep = Globals.GetTheInstance().Field_sep == FIELD_SEP.COMA ? "," : ";";

            Globals.GetTheInstance().Date_format = Textbox_date_format.Text;
            Globals.GetTheInstance().Format_provider = Combobox_provider.Text;


            //WIND CONFIG
            Globals.GetTheInstance().SBPT_trigger_3sec = (double)DecimalUpDown_sbpt_3sec_trigger.Value;
            Globals.GetTheInstance().SBPT_trigger_10min = (double)DecimalUpDown_sbpt_10min_trigger.Value;
            Globals.GetTheInstance().SBPT_wind_delay_time_3sec = (int)DecimalUpDown_sbpt_wind_delay_3sec.Value;
            Globals.GetTheInstance().SBPT_low_hist_10min = (double)DecimalUpDown_sbpt_low_hist_10min.Value;

            Globals.GetTheInstance().SAMCA_trigger_3sec = (double)DecimalUpDown_samca_3sec_trigger.Value;
            Globals.GetTheInstance().SAMCA_trigger_10min = (double)DecimalUpDown_samca_10min_trigger.Value;
            Globals.GetTheInstance().SAMCA_wind_delay_time_3sec = (int)DecimalUpDown_samca_wind_delay_3sec.Value;
            Globals.GetTheInstance().SAMCA_low_hist_10min = (double)DecimalUpDown_samca_low_hist_10min.Value;

            //INCLINOMETER CONFIG
            Globals.GetTheInstance().SBPT_inc_avg_interval = (int)DecimalUpDown_sbpt_inc_avg_interval.Value;
            Globals.GetTheInstance().Max_diff_tcu_inc_emergency_stow = (double)DecimalUpDown_inc_max_diff_emerg_stow.Value;
            Globals.GetTheInstance().Max_diff_tcu_inc_alarm = (double)DecimalUpDown_inc_max_diff_alarm.Value;

            //DYN CONFIG
            Globals.GetTheInstance().SBPT_dyn_avg_interval = (int)DecimalUpDown_sbpt_dyn_avg_interval.Value;
            Globals.GetTheInstance().SBPT_dyn_max_mov_emerg_stow = (int)DecimalUpDown_sbpt_dyn_max_moving_emergency_stow.Value;
            Globals.GetTheInstance().SBPT_dyn_max_mov_alarm = (int)DecimalUpDown_sbpt_dyn_max_moving_alarm.Value;
            Globals.GetTheInstance().SBPT_dyn_max_static_alarm = (int)DecimalUpDown_sbpt_dyn_max_static_alarm.Value;


            bool save_ok = Manage_file.Save_app_setting();
            if (save_ok)
                MessageBox.Show("Config. saved", "Info", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            else
                MessageBox.Show("Error saving config.", "Info", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
        }

        #endregion


        #region Button exit

        private void Button_exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion


    }
}
