using System;
using System.Collections.Generic;
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
        }

        #endregion


        #region Loaded

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Check_depur_enable.IsChecked = Globals.GetTheInstance().Depur_enable == BIT_STATE.ON ? true : false;

            DecimalUpDown_wait_general_seq.Value = Globals.GetTheInstance().Record_data_interval;
            DecimalUpDown_modbus_modubs_read.Value = Globals.GetTheInstance().Modbus_start_address;
            DecimalUpDown_modbus_conn_timeout.Value = Globals.GetTheInstance().Modbus_timeout;
            DecimalUpDown_modbus_start_address.Value = Globals.GetTheInstance().Modbus_start_address;
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

            Globals.GetTheInstance().Record_data_interval = (int)DecimalUpDown_wait_general_seq.Value;
            Globals.GetTheInstance().Modbus_start_address = (int)DecimalUpDown_modbus_modubs_read.Value;
            Globals.GetTheInstance().Modbus_timeout = (int)DecimalUpDown_modbus_conn_timeout.Value;
            Globals.GetTheInstance().Modbus_start_address = (int)DecimalUpDown_modbus_start_address.Value;

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
