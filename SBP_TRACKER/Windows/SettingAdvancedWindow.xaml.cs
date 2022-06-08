using System.Windows;
using System.Windows.Input;

namespace SBP_TRACKER
{

    public partial class SettingAdvancedWindow : Window
    {
        public string Input_info { get; set; }
        public string Input_value { get; set; }

        #region Constructor

        public SettingAdvancedWindow()
        {
            InitializeComponent();
        }

        #endregion


        #region Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Check_api_enable.IsChecked = Globals.GetTheInstance().Enable_web_api == BIT_STATE.ON;
            DecimalUpDown_tracker_ID.Value = Globals.GetTheInstance().Tracker_ID;
            Textbox_tracker_name.Text = Globals.GetTheInstance().Tracker_name;
            Textbox_api_root.Text = Globals.GetTheInstance().API_root;
            Textbox_data_controller.Text = Globals.GetTheInstance().Data_controller_route;
            Textbox_state_controller.Text = Globals.GetTheInstance().State_controller_route;
            DecimalUpDown_send_state_api.Value = Globals.GetTheInstance().Send_state_interval_web_API;
            DecimalUpDown_send_data_api.Value = Globals.GetTheInstance().Send_data_interval_web_API;
            DecimalUpDown_wait_error_conn_api.Value = Globals.GetTheInstance().Wait_error_conn_interval_web_API;
            DecimalUpDown_http_timeout.Value = Globals.GetTheInstance().HTTP_timeout;
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


        #region Send API

        private async void Button_sendAPI_state_Click(object sender, RoutedEventArgs e)
        {
            await Globals.GetTheInstance().ManageWebAPI.SendModbusAPIState();
        }

        private async void Button_sendAPI_data_Click(object sender, RoutedEventArgs e)
        {
            await Globals.GetTheInstance().ManageWebAPI.SendModbusAPIData();
        }

        #endregion

        #region Save

        private void Button_save_Click(object sender, RoutedEventArgs e)
        {
            Globals.GetTheInstance().Enable_web_api = Check_api_enable.IsChecked == true ? BIT_STATE.ON : BIT_STATE.OFF;
            Globals.GetTheInstance().Tracker_ID = DecimalUpDown_tracker_ID.Value;
            Globals.GetTheInstance().Tracker_name = Textbox_tracker_name.Text;
            Globals.GetTheInstance().API_root = Textbox_api_root.Text;
            Globals.GetTheInstance().Data_controller_route = Textbox_data_controller.Text;
            Globals.GetTheInstance().State_controller_route = Textbox_state_controller.Text;
            Globals.GetTheInstance().Send_state_interval_web_API = DecimalUpDown_send_state_api.Value;
            Globals.GetTheInstance().Send_data_interval_web_API = DecimalUpDown_send_data_api.Value;
            Globals.GetTheInstance().Wait_error_conn_interval_web_API = DecimalUpDown_wait_error_conn_api.Value;
            Globals.GetTheInstance().HTTP_timeout = DecimalUpDown_http_timeout.Value;

            bool save_ok = Manage_file.Save_app_setting();
            if (save_ok)
                MessageBox.Show("Config. saved", "Info", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            else
                MessageBox.Show("Error saving config.", "Info", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
        }

        #endregion

        #region Exit

        private void Button_exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        #endregion


    }
}
