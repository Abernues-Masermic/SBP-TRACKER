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
            Check_depur_enable.IsChecked = Globals.GetTheInstance().Depur_enable == BIT_STATE.ON ? true : false;

            DecimalUpDown_modbus_read_met.Value = Globals.GetTheInstance().Modbus_read_met_interval;
            DecimalUpDown_modbus_read_tcu.Value = Globals.GetTheInstance().Modbus_read_tcu_interval;
            DecimalUpDown_modbus_write_tcu.Value = Globals.GetTheInstance().Modbus_write_tcu_interval;
            DecimalUpDown_modbus_conn_timeout.Value = Globals.GetTheInstance().Modbus_timeout;
            DecimalUpDown_modbus_dir_scs_command.Value = Globals.GetTheInstance().Modbus_dir_scs_command;

            DecimalUpDown_record_data_met1.Value = Globals.GetTheInstance().Record_data_met1_interval;
            DecimalUpDown_record_data_met2.Value = Globals.GetTheInstance().Record_data_met2_interval;
            DecimalUpDown_record_data_tcu.Value = Globals.GetTheInstance().Record_data_tcu_interval;

            Combobox_decimal_sep.SelectedIndex = (int) Globals.GetTheInstance().Decimal_sep;
            Combobox_field_sep.SelectedIndex = (int) Globals.GetTheInstance().Field_sep;

            Textbox_date_format.Text = Globals.GetTheInstance().Date_format;
            Combobox_provider.Text = Globals.GetTheInstance().Format_provider;
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
            Globals.GetTheInstance().Depur_enable = Check_depur_enable.IsChecked == true ? BIT_STATE.ON : BIT_STATE.OFF;

            Globals.GetTheInstance().Modbus_read_met_interval = (int)DecimalUpDown_modbus_read_met.Value;
            Globals.GetTheInstance().Modbus_read_tcu_interval = (int)DecimalUpDown_modbus_read_tcu.Value;
            Globals.GetTheInstance().Modbus_write_tcu_interval = (int)DecimalUpDown_modbus_write_tcu.Value;
            Globals.GetTheInstance().Modbus_timeout = (int)DecimalUpDown_modbus_conn_timeout.Value;
            Globals.GetTheInstance().Modbus_dir_scs_command = (int)DecimalUpDown_modbus_dir_scs_command.Value;


            Globals.GetTheInstance().Record_data_met1_interval = (int)DecimalUpDown_record_data_met1.Value;
            Globals.GetTheInstance().Record_data_met2_interval = (int)DecimalUpDown_record_data_met2.Value;
            Globals.GetTheInstance().Record_data_tcu_interval = (int)DecimalUpDown_record_data_tcu.Value;

            Globals.GetTheInstance().Decimal_sep = (DECIMAL_SEP) Combobox_decimal_sep.SelectedIndex;
            Globals.GetTheInstance().Field_sep = (FIELD_SEP)Combobox_field_sep.SelectedIndex;
            Globals.GetTheInstance().SField_sep = Globals.GetTheInstance().Field_sep == FIELD_SEP.COMA ? "," : ";";

            Globals.GetTheInstance().Date_format = Textbox_date_format.Text;
            Globals.GetTheInstance().Format_provider = Combobox_provider.Text;

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
