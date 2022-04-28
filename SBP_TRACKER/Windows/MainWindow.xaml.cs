using NumericUpDownLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace SBP_TRACKER
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Timers

        private System.Timers.Timer m_timer_start_tcp;
        private System.Timers.Timer m_timer_reconnect_slave;

        private System.Timers.Timer m_timer_read_scs_normal_modbus;
        private System.Timers.Timer m_timer_read_scs_fast_modbus;
        private System.Timers.Timer m_timer_read_tcu_modbus;
        private System.Timers.Timer m_timer_write_tcu_watchdog_modbus;
        private System.Timers.Timer m_timer_write_tcu_datetime_modbus;

        private System.Timers.Timer m_timer_write_samca_modbus;

        private System.Timers.Timer m_timer_record_scs_normal;
        private System.Timers.Timer m_timer_record_scs_fast;
        private System.Timers.Timer m_timer_record_tcu;
        private System.Timers.Timer m_timer_record_samca;
        private System.Timers.Timer m_timer_send_mail;

        #endregion

        private int m_scada_watchdog = 0;
        private int m_meteo_watchdog = 0;

        private bool b_manual_start_stop;
        private bool b_individual_start_stop;

        private bool b_first_read_scs_finish;
        private bool b_first_read_tcu_finish;
        private bool b_first_read_samca_finish;

        private bool b_read_scs_modbus;
        private bool b_read_write_tcu_modbus;
        private bool b_read_write_samca_modbus;

        private bool b_slaves_ok = true;

        private bool flag_read_normal = true;
        private bool flag_read_fast = true;
        private bool flag_read_tcu = true;
        private bool flag_write_tcu_watchdog = true;
        private bool flag_write_tcu_datetime = true;


        private bool b_record = true;
        private MAIL_STATE m_mail_state;
        private CODIFIED_STATUS_STATE m_codified_status_state;


        private List<Manage_thread> m_list_manage_thread_in_start_process = new();

        private ObservableCollection<TCPModbusSlaveEntry> m_collection_modbus_slave;
        private ObservableCollection<TCPModbusVarEntry> m_collection_modbus_var;
        private TCPModbusSlaveEntry? m_selected_slave_entry = null;
        private TCUCommand? m_selected_command = null;

        private Queue<List<int>> m_queue_commands = new();
        private Queue<string> m_queue_reconnect_slave = new();

        #region WIND AVG

        private List<List<Tuple<DateTime, double>>> m_list_wind_read_values = new();
        private double[] m_array_wind_avg_values = new double[Constants.MAX_WIND_AVG];
        private bool[] m_array_wind_max_break_in_range = new bool[Constants.MAX_WIND_AVG];

        private DateTime[] m_array_date_trigger_start_3sec = new DateTime[Constants.MAX_WIND_AVG / 2];
        private double[] m_array_low_histeresis_10min = new double[Constants.MAX_WIND_AVG / 2];

        #endregion

        #region INC AVG

        private List<List<Tuple<DateTime, double>>> m_list_inc_slope_read_values = new();
        private double[] m_array_inc_slope_avg_values = new double[Constants.MAX_INC_AVG];
        private bool[] m_array_inc_slope_emerg_stow_in_range= new bool[Constants.MAX_INC_AVG -1];
        private bool[] m_array_inc_slope_alarm_in_range = new bool[Constants.MAX_INC_AVG - 1];

        #endregion

        #region DYN AVG

        private List<List<Tuple<DateTime, double>>> m_list_dyn_read_values = new();
        private double[] m_array_dyn_avg_values = new double[Constants.MAX_DYN_AVG];
        private bool[] m_array_dyn_excesive_force_emerg_stow_in_range = new bool[Constants.MAX_DYN_AVG];
        private bool[] m_array_dyn_excesive_force_alarm_in_range = new bool[Constants.MAX_DYN_AVG];

        #endregion


        #region Array controles

        private List<KeyValuePair<string, System.Windows.Shapes.Ellipse>> m_array_ellipse_slave = new();

        //TCU
        private List<KeyValuePair<LINK_TO_SEND_TCU, Label>> m_keyValuePair_link_to_send_tcu_value = new();
        private List<KeyValuePair<LINK_TO_SEND_TCU, Label>> m_keyValuePair_link_to_send_tcu_unit = new();

        private List<Label> m_array_label_tcu_command_param = new();
        private List<DecimalUpDown> m_array_decimal_tcu_command_param = new();

        //GRAPHIC
        private List<Border> m_array_border_tcu_bit_state = new();

        private List<Label> m_array_label_graphic_title = new();
        private List<Label> m_array_label_graphic_value = new();

        private List<Image> m_array_image_forward_enable = new();
        private List<Image> m_array_image_backward_enable = new();
        private List<Image> m_array_image_forward_disable = new();
        private List<Image> m_array_image_backward_disable = new();
        private List<List<Image>> m_array_image_direction = new();
        private List<Border> m_array_border_field_safety = new();

        private List<KeyValuePair<LINK_TO_SEND_TCU, Label>> m_keyValuePair_link_to_send_tcu_graphic = new();

        private List<Border> m_array_border_wind_avg_value = new();
        private List<Label> m_array_label_wind_avg_value = new();
        private List<Label> m_array_label_wind_avg_max = new();

        private List<Label> m_array_label_inc_slope_avg_value = new();
        private List<Label> m_array_label_dyn_avg_value = new();

        private List<KeyValuePair<LINK_TO_GRAPHIC, Label>> m_keyValuePair_codified_status = new();

        #endregion




        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            Title = "SBP TRACKER / ver " + Constants.version;
            Label_ver.Content = "SBP TRACKER / ver " + Constants.version;

            #region Array controles

            m_array_label_graphic_title = new List<Label>
            {
                Label_southcorner_inc5_slope_long_title,
                Label_southcorner_inc5_slope_lat_title,
                Label_southcorner_inc5_acceleration_x_title,
                Label_southcorner_inc5_acceleration_y_title,
                Label_southcorner_inc5_acceleration_z_title,
                Label_southcorner_inc5_velocity_x_title,
                Label_southcorner_inc5_velocity_y_title,
                Label_southcorner_inc5_velocity_z_title,

                Label_southcorner_inc4_slope_long_title,
                Label_southcorner_inc4_slope_lat_title,
                Label_southcorner_inc4_acceleration_x_title,
                Label_southcorner_inc4_acceleration_y_title,
                Label_southcorner_inc4_acceleration_z_title,
                
                //Velocity is collapsed in graphic
                Label_southcorner_inc4_velocity_x_title,
                Label_southcorner_inc4_velocity_y_title,
                Label_southcorner_inc4_velocity_z_title,

                Label_southlock_inc3_slope_long_title,
                Label_southlock_inc3_slope_lat_title,
                Label_southlock_inc3_acceleration_x_title,
                Label_southlock_inc3_acceleration_y_title,
                Label_southlock_inc3_acceleration_z_title,
                Label_southlock_dyn1_title,

                Label_northlock_inc1_slope_long_title,
                Label_northlock_inc1_slope_lat_title,
                Label_northlock_inc1_acceleration_x_title,
                Label_northlock_inc1_acceleration_y_title,
                Label_northlock_inc1_acceleration_z_title,

                Label_maindrive_tempprobe1_title,
                Label_maindrive_tempprobe2_title,
                Label_maindrive_tempprobe3_title,
                Label_maindrive_tempprobe4_title,

                Label_maindrive_inc2_slope_long_title,
                Label_maindrive_inc2_slope_lat_title,
                Label_maindrive_inc2_acceleration_x_title,
                Label_maindrive_inc2_acceleration_y_title,
                Label_maindrive_inc2_acceleration_z_title,

                Label_maindrive_tracker_irr1_title,
                Label_maindrive_dyn3_title,
                Label_maindrive_dyn2_title,

                Label_weather_mast_wd1_title,
                Label_weather_mast_anenometer1_title,
                label_weather_mast_anenometer2_title,
                Label_weather_mast_fixed_irr2_title,

                Label_samca_irradiance_ghi_title,
                Label_samca_irradiance_poa_title,
                Label_samca_irradiance_clean_title,
                Label_samca_irradiance_soiled_title,

                Label_samca_wind_dir_title,
                Label_samca_wind_speed_main_10m_title,
                Label_samca_wind_speed_main_5m_title,
                Label_samca_wind_speed_sec_10m_title,
                Label_samca_wind_speed_sec_5m_title,

                Label_samca_thermometer_panel1_title,
                Label_samca_thermometer_panel2_title,
                Label_samca_humidity_title,
                Label_samca_rain_fall_title,

                Label_samca_inverter_current_tracker_title,
                Label_samca_inverter_voltage_tracker_title,
                Label_samca_inverter_current_fixed_title,
                Label_samca_inverter_voltage_fixed_title
            };

            m_array_label_graphic_title.ForEach(label_graphic_title =>
            {
                label_graphic_title.Background = Brushes.DarkGray;
                label_graphic_title.MouseDoubleClick += new MouseButtonEventHandler(LabelGraphicTitle_MouseDoubleClick_EventHandler);
            });
            m_array_label_graphic_value = new List<Label>
            {
                Label_southcorner_inc5_slope_long_value,
                Label_southcorner_inc5_slope_lat_value,
                Label_southcorner_inc5_acceleration_x_value,
                Label_southcorner_inc5_acceleration_y_value,
                Label_southcorner_inc5_acceleration_z_value,
                Label_southcorner_inc5_velocity_x_value,
                Label_southcorner_inc5_velocity_y_value,
                Label_southcorner_inc5_velocity_z_value,

                Label_southcorner_inc4_slope_long_value,
                Label_southcorner_inc4_slope_lat_value,
                Label_southcorner_inc4_acceleration_x_value,
                Label_southcorner_inc4_acceleration_y_value,
                Label_southcorner_inc4_acceleration_z_value,
                Label_southcorner_inc4_velocity_x_value,
                Label_southcorner_inc4_velocity_y_value,
                Label_southcorner_inc4_velocity_z_value,

                Label_southlock_inc3_slope_long_value,
                Label_southlock_inc3_slope_lat_value,
                Label_southlock_inc3_acceleration_x_value,
                Label_southlock_inc3_acceleration_y_value,
                Label_southlock_inc3_acceleration_z_value,
                Label_southlock_dyn1_value,

                Label_northlock_inc1_slope_long_value,
                Label_northlock_inc1_slope_lat_value,
                Label_northlock_inc1_acceleration_x_value,
                Label_northlock_inc1_acceleration_y_value,
                Label_northlock_inc1_acceleration_z_value,

                Label_maindrive_tempprobe1_value,
                Label_maindrive_tempprobe2_value,
                Label_maindrive_tempprobe3_value,
                Label_maindrive_tempprobe4_value,

                Label_maindrive_inc2_slope_long_value,
                Label_maindrive_inc2_slope_lat_value,
                Label_maindrive_inc2_acceleration_x_value,
                Label_maindrive_inc2_acceleration_y_value,
                Label_maindrive_inc2_acceleration_z_value,

                Label_maindrive_tracker_irr1_value,
                Label_maindrive_dyn3_value,
                Label_maindrive_dyn2_value,

                Label_weather_mast_wd1_value,
                Label_weather_mast_anenometer1_value,
                Label_weather_mast_anenometer2_value,
                Label_weather_mast_fixed_irr2_value,

                Label_samca_irradiance_ghi_value,
                Label_samca_irradiance_poa_value,
                Label_samca_irradiance_clean_value,
                Label_samca_irradiance_soiled_value,

                Label_samca_wind_dir_value,
                Label_samca_wind_speed_main_10m_value,
                Label_samca_wind_speed_main_5m_value,
                Label_samca_wind_speed_sec_10m_value,
                Label_samca_wind_speed_sec_5m_value,

                Label_samca_thermometer_panel1_value,
                Label_samca_thermometer_panel2_value,
                Label_samca_humidity_value,
                Label_samca_rain_fall_value,

                Label_samca_inverter_current_tracker_value,
                Label_samca_inverter_voltage_tracker_value,
                Label_samca_inverter_current_fixed_value,
                Label_samca_inverter_voltage_fixed_value
            };

            #region LINK TO SEND TCU

            m_keyValuePair_link_to_send_tcu_value.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.SCADA_WD, Label_scada_counter_to_tcu_value));
            m_keyValuePair_link_to_send_tcu_value.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.WIN_SPEED, Label_wind_speed_to_tcu_value));
            m_keyValuePair_link_to_send_tcu_value.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.WIN_DIR, Label_wind_dir_to_tcu_value));
            m_keyValuePair_link_to_send_tcu_value.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.AMBIENT_TEMP, Label_ambient_temp_to_tcu_value));
            m_keyValuePair_link_to_send_tcu_value.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.AMBIENT_PRESSURE, Label_ambient_pressure_to_tcu_value));
            m_keyValuePair_link_to_send_tcu_value.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.DIRECT_IRRAD, Label_ambient_direct_irr_to_tcu_value));
            m_keyValuePair_link_to_send_tcu_value.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.DIFUSSE_IRRAD, Label_ambient_difusse_irr_to_tcu_value));
            m_keyValuePair_link_to_send_tcu_value.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.FIELD_SAFETY, Label_field_safety_supervisor_tcu_value));
            m_keyValuePair_link_to_send_tcu_value.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.METEO_WD, Label_meteo_counter_to_tcu_value));

            m_keyValuePair_link_to_send_tcu_unit.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.SCADA_WD, Label_scada_counter_to_tcu_unit));
            m_keyValuePair_link_to_send_tcu_unit.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.WIN_SPEED, Label_wind_speed_to_tcu_unit));
            m_keyValuePair_link_to_send_tcu_unit.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.WIN_DIR, Label_wind_dir_to_tcu_unit));
            m_keyValuePair_link_to_send_tcu_unit.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.AMBIENT_TEMP, Label_ambient_temp_to_tcu_unit));
            m_keyValuePair_link_to_send_tcu_unit.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.AMBIENT_PRESSURE, Label_ambient_pressure_to_tcu_unit));
            m_keyValuePair_link_to_send_tcu_unit.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.DIRECT_IRRAD, Label_ambient_direct_irr_to_tcu_unit));
            m_keyValuePair_link_to_send_tcu_unit.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.DIFUSSE_IRRAD, Label_ambient_difusse_irr_to_tcu_unit));
            m_keyValuePair_link_to_send_tcu_unit.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.FIELD_SAFETY, Label_field_safety_supervisor_tcu_unit));
            m_keyValuePair_link_to_send_tcu_unit.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.METEO_WD, Label_meteo_counter_to_tcu_unit));

            m_keyValuePair_link_to_send_tcu_graphic.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.SCADA_WD, Label_scada_counter_to_tcu_graphic));
            m_keyValuePair_link_to_send_tcu_graphic.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.WIN_SPEED, Label_wind_speed_to_tcu_graphic));
            m_keyValuePair_link_to_send_tcu_graphic.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.WIN_DIR, Label_wind_dir_to_tcu_graphic));
            m_keyValuePair_link_to_send_tcu_graphic.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.AMBIENT_TEMP, Label_ambient_temp_to_tcu_graphic));
            m_keyValuePair_link_to_send_tcu_graphic.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.AMBIENT_PRESSURE, Label_ambient_pressure_to_tcu_graphic));
            m_keyValuePair_link_to_send_tcu_graphic.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.DIRECT_IRRAD, Label_ambient_direct_irr_to_tcu_graphic));
            m_keyValuePair_link_to_send_tcu_graphic.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.DIFUSSE_IRRAD, Label_ambient_difusse_irr_to_tcu_graphic));
            m_keyValuePair_link_to_send_tcu_graphic.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.FIELD_SAFETY, Label_field_safety_supervisor_tcu_graphic));
            m_keyValuePair_link_to_send_tcu_graphic.Add(new KeyValuePair<LINK_TO_SEND_TCU, Label>(LINK_TO_SEND_TCU.METEO_WD, Label_meteo_counter_to_tcu_graphic));

            #endregion


            m_array_border_tcu_bit_state = new List<Border> {
                Border_main_drive,
                Border_lock1,
                Border_lock2
            };

            m_array_image_backward_disable = new List<Image> {
                Image_main_drive_backward_disable,
                Image_lock1_backward_disable,
                Image_lock2_backward_disable,
            };
            m_array_image_forward_disable = new List<Image> {
                Image_main_drive_forward_disable,
                Image_lock1_forward_disable,
                Image_lock2_forward_disable,
            };
            m_array_image_backward_enable = new List<Image> {
                Image_main_drive_backward_enable,
                Image_lock1_backward_enable,
                Image_lock2_backward_enable,
            };
            m_array_image_forward_disable = new List<Image> {
                Image_main_drive_forward_enable,
                Image_lock1_forward_enable,
                Image_lock2_forward_enable,
            };
            m_array_image_direction = new List<List<Image>>{
                m_array_image_backward_disable,
                m_array_image_forward_disable,
                m_array_image_backward_enable,
                m_array_image_forward_disable
            };

            m_array_border_field_safety = new List<Border> {
                Border_autotrack_scada,
                Border_wind_ok,
                Border_autotrack_samca,
                Border_slaves_ok,
                Border_emergency_stow,
                Border_inc_in_range_emergency_stow,
                Border_inc_in_range_alarm,
                Border_dyn_in_range_emergency_stow,
                Border_dyn_in_range_alarm,
            };

            m_array_border_wind_avg_value = new List<Border> {
                Border_avg_5m_sbpt_3seg_value,
                Border_avg_5m_samca_3seg_value,
                Border_avg_5m_sbpt_10min_value,
                Border_avg_5m_samca_10min_value,
            };
            m_array_label_wind_avg_value = new List<Label> {
                Label_avg_5m_sbpt_3seg_value,
                Label_avg_5m_samca_3seg_value,
                Label_avg_5m_sbpt_10min_value,
                Label_avg_5m_samca_10min_value,
            };
            m_array_label_wind_avg_max = new List<Label> {
                Label_avg_5m_sbpt_3seg_max,
                Label_avg_5m_samca_3seg_max,
                Label_avg_5m_sbpt_10min_max,
                Label_avg_5m_samca_10min_max,
            };
            m_array_label_inc_slope_avg_value = new List<Label>
            {
                Label_northlock_inc1_slope_long_value,
                Label_maindrive_inc2_slope_long_value,
                Label_southlock_inc3_slope_long_value,
                Label_southcorner_inc4_slope_long_value,
                Label_southcorner_inc5_slope_long_value,
                Label_TCU_position_el_value
            };
            m_array_label_dyn_avg_value = new List<Label>
            {
                Label_southlock_dyn1_value,
                Label_maindrive_dyn2_value,
                Label_maindrive_dyn3_value,
            };

            #region CODIFIED STATUS POS GRAPHIC

            m_keyValuePair_codified_status.Add(new KeyValuePair<LINK_TO_GRAPHIC, Label>(LINK_TO_GRAPHIC.ESTADO_CODIF_TCU, Label_TCU_tracker_status_value));
            m_keyValuePair_codified_status.Add(new KeyValuePair<LINK_TO_GRAPHIC, Label>(LINK_TO_GRAPHIC.TRACKER_POS_EL, Label_TCU_position_el_value));
            m_keyValuePair_codified_status.Add(new KeyValuePair<LINK_TO_GRAPHIC, Label>(LINK_TO_GRAPHIC.TRACKER_SET_POINT_EL, Label_TCU_position_setpoint_value));
            m_keyValuePair_codified_status.Add(new KeyValuePair<LINK_TO_GRAPHIC, Label>(LINK_TO_GRAPHIC.TRACKER_ERROR_EL, Label_TCU_error_el_value));
            m_keyValuePair_codified_status.Add(new KeyValuePair<LINK_TO_GRAPHIC, Label>(LINK_TO_GRAPHIC.TRACKER_CONSIGNA_DESFASE_EL, Label_TCU_consigna_desfase_el_value));
            m_keyValuePair_codified_status.Add(new KeyValuePair<LINK_TO_GRAPHIC, Label>(LINK_TO_GRAPHIC.TRACKER_SPEED, Label_TCU_traker_speed_value));
            m_keyValuePair_codified_status.Add(new KeyValuePair<LINK_TO_GRAPHIC, Label>(LINK_TO_GRAPHIC.MAIN_DRIVE_POWER, Label_TCU_main_drive_power_value));
            m_keyValuePair_codified_status.Add(new KeyValuePair<LINK_TO_GRAPHIC, Label>(LINK_TO_GRAPHIC.LOCK1_DRIVE_POWER, Label_TCU_lock1_drive_power_value));
            m_keyValuePair_codified_status.Add(new KeyValuePair<LINK_TO_GRAPHIC, Label>(LINK_TO_GRAPHIC.LOCK2_DRIVE_POWER, Label_TCU_lock2_drive_power_value));
            m_keyValuePair_codified_status.Add(new KeyValuePair<LINK_TO_GRAPHIC, Label>(LINK_TO_GRAPHIC.FECHA_RTC, Label_TCU_datetime_value));
            m_keyValuePair_codified_status.Add(new KeyValuePair<LINK_TO_GRAPHIC, Label>(LINK_TO_GRAPHIC.MILISECONDS, Label_TCU_milisec_value));

            #endregion

            #endregion

            Globals.GetTheInstance().Manage_delegate = new Manage_delegate();
            Globals.GetTheInstance().Manage_delegate.TCP_handler_event += new Manage_delegate.TCP_handler(TCP_events_to_main);
        }

        #endregion



        #region Loaded

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool create_dir_ok = Manage_file.Create_directories();
            bool create_file_ok = Manage_file.Create_files();
            if (!create_dir_ok || !create_file_ok)
            {
                MessageBox.Show("Error generating files. Check admin credentials", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                Application.Current.Shutdown();
            }

            bool load_mail_ok = Manage_file.Load_email_to();

            bool load_app_ok = Manage_file.Load_app_setting();
            Globals.GetTheInstance().nfi = new NumberFormatInfo
            {
                NumberDecimalSeparator = Globals.GetTheInstance().Decimal_sep == DECIMAL_SEP.PUNTO ? "." : ","
            };

            StartAvgValues();


            bool load_modbus_ok = Load_modbus_slave_var();
            bool load_tcu_codified_status = Manage_file.Load_tcu_codified_status();
            bool load_commands = Manage_file.Load_modbus_commands();

            bool load_slope_correction = Manage_file.Load_main_drive_slope_correction();

            if (!load_app_ok || !load_modbus_ok || !load_tcu_codified_status)
            {
                MessageBox.Show("Error loading config. files. Check the system", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                Application.Current.Shutdown();
            }


            if (load_tcu_codified_status)
            {
                Listview_tcu_codified_status.ItemsSource = Globals.GetTheInstance().List_tcu_codified_status.OrderBy(tcu_encode => tcu_encode.DirModbus);
                Listview_tcu_codified_status.Items.Refresh();
            }

            if (load_commands)
            {
                Listview_tcu_commands.ItemsSource = Globals.GetTheInstance().List_tcu_command.OrderBy(tcu_command => tcu_command.Index);
                Listview_tcu_commands.Items.Refresh();
            }

            this.Height = Globals.GetTheInstance().Depur_enable == BIT_STATE.ON ? Constants.depur_enable_height : Constants.depur_disable_height;
            Button_record.Visibility = Globals.GetTheInstance().Depur_enable == BIT_STATE.OFF ? Visibility.Collapsed : Visibility.Visible;

            #region Timer

            m_timer_start_tcp = new System.Timers.Timer();
            m_timer_start_tcp.Elapsed += Timer_start_tcp_Tick;
            m_timer_start_tcp.Interval = 1000;
            m_timer_start_tcp.Stop();

            m_timer_reconnect_slave = new System.Timers.Timer();
            m_timer_reconnect_slave.Elapsed += Timer_reconnect_slave_Tick;
            m_timer_reconnect_slave.Interval = Globals.GetTheInstance().Modbus_reconnect_interval;
            m_timer_reconnect_slave.Stop();

            b_read_scs_modbus = false;

            m_timer_read_scs_normal_modbus = new System.Timers.Timer();
            m_timer_read_scs_normal_modbus.Elapsed += Timer_read_scs_normal_modbus_Tick;
            m_timer_read_scs_normal_modbus.Interval = Globals.GetTheInstance().Modbus_read_scs_normal_interval;
            m_timer_read_scs_normal_modbus.Start();

            m_timer_read_scs_fast_modbus = new System.Timers.Timer();
            m_timer_read_scs_fast_modbus.Elapsed += Timer_read_scs_fast_modbus_Tick;
            m_timer_read_scs_fast_modbus.Interval = Globals.GetTheInstance().Modbus_read_scs_fast_interval;
            m_timer_read_scs_fast_modbus.Start();

            b_read_write_tcu_modbus = false;

            m_timer_read_tcu_modbus = new System.Timers.Timer();
            m_timer_read_tcu_modbus.Elapsed += Timer_read_tcu_modbus_Tick;
            m_timer_read_tcu_modbus.Interval = Globals.GetTheInstance().Modbus_read_tcu_interval;
            m_timer_read_tcu_modbus.Start();

            m_timer_write_tcu_watchdog_modbus = new System.Timers.Timer();
            m_timer_write_tcu_watchdog_modbus.Elapsed += Timer_write_tcu_watchdog_modbus_Tick;
            m_timer_write_tcu_watchdog_modbus.Interval = Globals.GetTheInstance().Modbus_write_tcu_watchdog_interval;
            m_timer_write_tcu_watchdog_modbus.Start();

            m_timer_write_tcu_datetime_modbus = new System.Timers.Timer();
            m_timer_write_tcu_datetime_modbus.Elapsed += Timer_write_tcu_datetime_modbus_Tick;
            m_timer_write_tcu_datetime_modbus.Interval = Globals.GetTheInstance().Modbus_write_tcu_datetime_interval;
            m_timer_write_tcu_datetime_modbus.Start();

            b_read_write_samca_modbus = false;

            m_timer_write_samca_modbus = new System.Timers.Timer();
            m_timer_write_samca_modbus.Elapsed += Timer_write_samca_modbus_Tick;
            m_timer_write_samca_modbus.Interval = Globals.GetTheInstance().Modbus_write_samca_interval;
            m_timer_write_samca_modbus.Start();

            m_timer_record_scs_normal = new System.Timers.Timer();
            m_timer_record_scs_normal.Elapsed += Timer_record_scs_normal_Tick;
            m_timer_record_scs_normal.Interval = Globals.GetTheInstance().Record_scs_normal_interval;
            m_timer_record_scs_normal.Start();

            m_timer_record_scs_fast = new System.Timers.Timer();
            m_timer_record_scs_fast.Elapsed += Timer_record_scs_fast_Tick;
            m_timer_record_scs_fast.Interval = Globals.GetTheInstance().Record_scs_fast_interval;
            m_timer_record_scs_fast.Start();

            m_timer_record_tcu = new System.Timers.Timer();
            m_timer_record_tcu.Elapsed += Timer_record_tcu_Tick;
            m_timer_record_tcu.Interval = Globals.GetTheInstance().Record_tcu_interval;
            m_timer_record_tcu.Start();

            m_timer_record_samca = new System.Timers.Timer();
            m_timer_record_samca.Elapsed += Timer_record_samca_Tick;
            m_timer_record_samca.Interval = Globals.GetTheInstance().Record_samca_interval;
            m_timer_record_samca.Start();


            m_timer_send_mail = new System.Timers.Timer();
            m_timer_send_mail.Elapsed += Timer_send_mail_Tick;
            m_timer_send_mail.Interval = 10000000;
            m_timer_send_mail.Stop();

            #endregion


            if (Globals.GetTheInstance().Mail_instant != "__:__")
                Send_mail_instant_start();
        }

        #endregion


        #region START AVG VALUES

        private void StartAvgValues()
        {
            //WIND
            m_array_border_wind_avg_value.ForEach(wind_avg_border => wind_avg_border.BorderBrush = Brushes.Black);

            m_array_label_wind_avg_value.ForEach(wind_avg_label =>
            {
                wind_avg_label.Content = string.Empty;
                wind_avg_label.Foreground = Brushes.Black;
            });

            Label_avg_5m_sbpt_3seg_max.Content = Globals.GetTheInstance().SBPT_trigger_3sec;
            Label_avg_5m_sbpt_10min_max.Content = Globals.GetTheInstance().SBPT_trigger_10min;
            Label_avg_5m_sbpt_low_hist_10min_value.Content = Globals.GetTheInstance().SBPT_low_hist_10min;

            Label_avg_5m_samca_3seg_max.Content = Globals.GetTheInstance().SAMCA_trigger_3sec;
            Label_avg_5m_samca_10min_max.Content = Globals.GetTheInstance().SAMCA_trigger_10min;
            Label_avg_5m_samca_low_hist_10min_value.Content = Globals.GetTheInstance().SAMCA_low_hist_10min;

            m_list_wind_read_values = new();
            for (int index = 0; index < Constants.MAX_WIND_AVG; index++)
                m_list_wind_read_values.Add(new());

            m_array_wind_avg_values = Enumerable.Repeat(0.0, Constants.MAX_WIND_AVG).ToArray();
            m_array_wind_max_break_in_range = Enumerable.Repeat(true, Constants.MAX_WIND_AVG).ToArray();


            m_array_date_trigger_start_3sec[(int)WIND_DATE_TRIGGER_3SEC.SBPT_3SEC] = DateTime.Now;
            m_array_date_trigger_start_3sec[(int)WIND_DATE_TRIGGER_3SEC.SAMCA_3SEC] = DateTime.Now;

            m_array_low_histeresis_10min[(int)WIND_LOW_HIST_10MIN.SBPT_10MIN] = Globals.GetTheInstance().SBPT_low_hist_10min;
            m_array_low_histeresis_10min[(int)WIND_LOW_HIST_10MIN.SAMCA_10MIN] = Globals.GetTheInstance().SAMCA_low_hist_10min;


            //INCLINOMETER
            m_array_inc_slope_avg_values = Enumerable.Repeat(0.0, Constants.MAX_INC_AVG).ToArray();
            m_array_inc_slope_emerg_stow_in_range = Enumerable.Repeat(true, Constants.MAX_INC_AVG -1).ToArray();
            m_array_inc_slope_alarm_in_range = Enumerable.Repeat(true, Constants.MAX_INC_AVG - 1).ToArray();

            m_list_inc_slope_read_values = new();
            for (int index = 0; index < Constants.MAX_INC_AVG; index++)
                m_list_inc_slope_read_values.Add(new());

            //DYNANOMETER
            m_list_dyn_read_values = new();
            m_array_dyn_excesive_force_emerg_stow_in_range = Enumerable.Repeat(true, Constants.MAX_DYN_AVG).ToArray();
            m_array_dyn_excesive_force_alarm_in_range = Enumerable.Repeat(true, Constants.MAX_DYN_AVG).ToArray();
            for (int index = 0; index < Constants.MAX_DYN_AVG; index++)
                m_list_dyn_read_values.Add(new());
        }

        #endregion

        #region Enable depur mode

        private int num_click_title = 0;
        private void Label_ver_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (num_click_title++ == 10)
            {
                num_click_title = 0;

                Globals.GetTheInstance().Depur_enable = Globals.GetTheInstance().Depur_enable == BIT_STATE.OFF ? BIT_STATE.ON : BIT_STATE.OFF;

                if (Manage_file.Save_app_setting())
                {
                    string s_info = "DEPUR MODE ";
                    s_info += Globals.GetTheInstance().Depur_enable == BIT_STATE.ON ? "ENABLE" : "DISABLE";
                    MessageBox.Show(s_info, "INFO", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);

                    this.Height = Globals.GetTheInstance().Depur_enable == BIT_STATE.ON ? Constants.depur_enable_height : Constants.depur_disable_height;
                    Button_record.Visibility = Globals.GetTheInstance().Depur_enable == BIT_STATE.OFF ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                    MessageBox.Show("ERROR ENABLING DEPU MODE", "INFO", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
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


        #region TCP events to main

        public void TCP_events_to_main(object sender, TCP_handler_args args)
        {
            switch (args.TCP_action)
            {
                case TCP_ACTION.CONNECT:
                    {
                        Manage_logs.SaveLogValue("Connected modbus slave -> " + args.Slave_name);

                        TCPModbusSlaveEntry slave_entry = Globals.GetTheInstance().List_modbus_slave_entry.FirstOrDefault(entry => entry.Name == args.Slave_name);
                        if (slave_entry != null)
                        {
                            slave_entry.Connected = true;
                            Dispatcher.Invoke(() => {
                                KeyValuePair<string, System.Windows.Shapes.Ellipse> ellipse_slave = m_array_ellipse_slave.Find(key => key.Key == args.Slave_name);
                                ellipse_slave.Value.Fill = slave_entry.Field_safety_enable ?  Brushes.Green : Brushes.CadetBlue;
                            });

                            if (slave_entry.Slave_type == SLAVE_TYPE.TCU)
                            {
                                b_read_write_tcu_modbus = true;
                                m_codified_status_state = CODIFIED_STATUS_STATE.NONE;
                            }
                            
                            if (slave_entry.Slave_type == SLAVE_TYPE.SAMCA)
                                b_read_write_samca_modbus = true;
                            

                            Check_ini_read_slaves(args.Slave_name);
                        }

                        break;
                    }

                case TCP_ACTION.DISCONNECT:
                    {
                        Manage_logs.SaveLogValue("DSICONNECT MODBUS SLAVE -> " + args.Slave_name);

                        TCPModbusSlaveEntry slave_entry = Globals.GetTheInstance().List_modbus_slave_entry.FirstOrDefault(slave_entry => slave_entry.Name == args.Slave_name);
                        if (slave_entry != null)
                        {
                            try
                            {
                                slave_entry.Connected = false;

                                if (slave_entry.Slave_type == SLAVE_TYPE.TCU)
                                {
                                    b_read_write_tcu_modbus = false;

                                    Globals.GetTheInstance().List_tcu_codified_status.ForEach(modbus_var => modbus_var.Value = string.Empty);
                                    Dispatcher.Invoke(() => Listview_tcu_codified_status.Items.Refresh());
                                }

                                if (slave_entry.Slave_type == SLAVE_TYPE.SAMCA)
                                    b_read_write_samca_modbus = false;
                                


                                else
                                {
                                    //Modbus var list
                                    m_collection_modbus_var.ToList()
                                        .Where(modbus_var => modbus_var.Slave == slave_entry.Name).ToList()
                                        .ForEach(modbus_var => modbus_var.Value = string.Empty);

                                    slave_entry.List_modbus_var.ForEach(modbus_var =>
                                    {
                                        //Schema
                                        Dispatcher.Invoke(() =>
                                        {
                                            m_array_label_graphic_title
                                                .Select((item, index) => new ControlLinq { Value = item.Content.ToString(), Position = index }).ToList()
                                                .ForEach(control =>
                                                {
                                                    if (control.Value.Equals(modbus_var.Name))
                                                        Dispatcher.Invoke(() => m_array_label_graphic_value[control.Position].Content = string.Empty);
                                                });
                                        });


                                        if ((LINK_TO_SEND_TCU)modbus_var.Link_to_send_tcu != LINK_TO_SEND_TCU.NONE)
                                        {
                                            int index = m_keyValuePair_link_to_send_tcu_value
                                            .Select((value, index) => new { Value = value, Index = index })
                                            .First(key_value_pair => key_value_pair.Value.Key == (LINK_TO_SEND_TCU)modbus_var.Link_to_send_tcu).Index;

                                            m_keyValuePair_link_to_send_tcu_value[index].Value.Content = string.Empty;
                                            m_keyValuePair_link_to_send_tcu_unit[index].Value.Content = string.Empty;
                                            m_keyValuePair_link_to_send_tcu_graphic[index].Value.Content = string.Empty;
                                        }

                                    });
                                }


                                if (Globals.GetTheInstance().List_modbus_slave_entry.All(entry => !entry.Connected) && b_manual_start_stop)
                                {
                                    Manage_logs.SaveLogValue("SE HA DETENIDO PROCESO DE LECTURA ALL SLAVES");

                                    b_read_scs_modbus = false;
                                    Dispatcher.Invoke(() => Checkbox_start.IsChecked = false);
                                    Dispatcher.Invoke(() => ((Storyboard)Resources["BlinkStoryboard"]).Remove());

                                    m_collection_modbus_var.ToList().ForEach(modbus_var => modbus_var.Value = string.Empty);
                                }

                                Dispatcher.Invoke(() => m_array_ellipse_slave.Find(key => key.Key == args.Slave_name).Value.Fill = Brushes.Red);
                                Dispatcher.Invoke(() => Listview_read_modbus.Items.Refresh());
                                Dispatcher.Invoke(() => m_array_label_graphic_value.ForEach(Label_graphic => Label_graphic.Content = string.Empty));
                            }
                            catch (Exception ex)
                            {
                                Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(TCP_events_to_main)} -> DISCONNECT -> {ex.Message}");
                            }
                        }

                        Dispatcher.Invoke(() => Listview_slave_modbus.Items.Refresh());

                        break;
                    }

                case TCP_ACTION.RECOVERY:
                    {
                        TCPModbusSlaveEntry slave_entry = Globals.GetTheInstance().List_modbus_slave_entry.FirstOrDefault(entry => entry.Name == args.Slave_name);
                        if (slave_entry != null)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                KeyValuePair<string, System.Windows.Shapes.Ellipse> ellipse_slave = m_array_ellipse_slave.Find(key => key.Key == args.Slave_name);
                                ellipse_slave.Value.Fill = slave_entry.Field_safety_enable ? Brushes.Green : Brushes.CadetBlue;
                            });
                        }

                        break;
                    }

                case TCP_ACTION.ERROR_CONNECT:
                    {
                        Manage_logs.SaveLogValue("ERROR CONNECTION MODBUS SLAVE -> " + args.Slave_name);

                        TCPModbusSlaveEntry slave_entry = Globals.GetTheInstance().List_modbus_slave_entry.FirstOrDefault(entry => entry.Name == args.Slave_name);
                        if (slave_entry != null)
                            Check_ini_read_slaves(slave_entry.Name);

                        break;
                    }

                case TCP_ACTION.ERROR_READ:
                    {
                        Manage_logs.SaveErrorValue($"ERROR READ MODBUS SLAVE -> { args.Slave_name}");
                        Dispatcher.Invoke(() => m_array_ellipse_slave.Find(x => x.Key == args.Slave_name).Value.Fill = Brushes.Yellow);

                        break;
                    }

                case TCP_ACTION.ERROR_WRITE:
                    {
                        Manage_logs.SaveErrorValue($"ERROR WRITE MODBUS SLAVE -> { args.Slave_name}");
                        Dispatcher.Invoke(() => m_array_ellipse_slave.Find(x => x.Key == args.Slave_name).Value.Fill = Brushes.Yellow);

                        break;
                    }

                case TCP_ACTION.RECONNECT:
                    {
                        Manage_logs.SaveLogValue($"STATE RECONNECT MODBUS SLAVE -> { args.Slave_name}");

                        Dispatcher.Invoke(() => m_array_ellipse_slave.Find(x => x.Key == args.Slave_name).Value.Fill = Brushes.Red);

                        Globals.GetTheInstance().List_manage_thread.Find(x => x.TCP_modbus_slave_entry.Name == args.Slave_name).Stop_tcp_com_thread();

                        Manage_logs.SaveLogValue($"ENQUEUE RECONNECT SLAVE -> {args.Slave_name}");

                        if (m_queue_reconnect_slave.Count == 0)
                            m_timer_reconnect_slave.Start();

                        m_queue_reconnect_slave.Enqueue(args.Slave_name);

                        break;
                    }
            }
        }

        #endregion




        #region Data show mode

        private void Radio_table_mode_Checked(object sender, RoutedEventArgs e)
        {
            if (Grid_table_mode != null)
            {
                Grid_table_mode.Visibility = Visibility.Visible;
                Dockpanel_graphic_mode.Visibility = Visibility.Collapsed;
                Grid_tcu_control_mode.Visibility = Visibility.Collapsed;
                Button_save_graphic_config.Visibility = Visibility.Collapsed;
            }
        }

        private void Radio_graphic_mode_Checked(object sender, RoutedEventArgs e)
        {
            if (Grid_table_mode != null)
            {
                Grid_table_mode.Visibility = Visibility.Collapsed;
                Dockpanel_graphic_mode.Visibility = Visibility.Visible;
                Grid_tcu_control_mode.Visibility = Visibility.Collapsed;
                Button_save_graphic_config.Visibility = Visibility.Visible;
            }
        }

        private void Radio_tcu_mode_Checked(object sender, RoutedEventArgs e)
        {
            if (Grid_table_mode != null)
            {
                Grid_table_mode.Visibility = Visibility.Collapsed;
                Dockpanel_graphic_mode.Visibility = Visibility.Collapsed;
                Grid_tcu_control_mode.Visibility = Visibility.Visible;
                Button_save_graphic_config.Visibility = Visibility.Collapsed;
            }
        }

        #endregion


        #region Graphic mode

        #region Save graphic map

        private void Button_save_graphic_config_Click(object sender, RoutedEventArgs e)
        {
            bool save_ok = Manage_file.Save_var_map_entries();
            if (!save_ok)
                MessageBox.Show("Error saving graphic config.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);

            else
                MessageBox.Show("Graphic config saved", "INFO", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
        }

        #endregion

        #region Label graphic double click

        private void LabelGraphicTitle_MouseDoubleClick_EventHandler(object sender, MouseEventArgs e)
        {
            Label label = sender as Label;
            SelectVarMapWindow selected_var_map_window = new();
            selected_var_map_window.Selected_index = m_array_label_graphic_title.IndexOf(label);

            //Save assigned var
            TCPModbusVarEntry current_modbus_var = null;
            Globals.GetTheInstance().List_modbus_slave_entry.ForEach(entry =>
            {
                entry.List_modbus_var.ForEach(modbus_var =>
                {
                    if (modbus_var.Graphic_pos == selected_var_map_window.Selected_index)
                    {
                        current_modbus_var = modbus_var;
                        selected_var_map_window.Selected_var = current_modbus_var.Name;
                    }
                });
            });


            if (selected_var_map_window.ShowDialog() == true)
            {
                if (string.IsNullOrEmpty(selected_var_map_window.Selected_var))
                {
                    label.Background = Brushes.DarkGray;

                    if (current_modbus_var != null)
                        current_modbus_var.Graphic_pos = Constants.index_no_selected;
                }

                else
                {
                    Globals.GetTheInstance().List_modbus_slave_entry.ForEach(entry =>
                    {
                        entry.List_modbus_var.ForEach(modbus_var =>
                        {
                            if (modbus_var.Name == selected_var_map_window.Selected_var)
                                modbus_var.Graphic_pos = selected_var_map_window.Selected_index;
                        });
                    });

                    label.Background = Brushes.Black;
                }
            }
        }

        #endregion

        #region TCU graphic buttons

        private void Button_stow_Click(object sender, RoutedEventArgs e)
        {
            List<int> list_command_fields = new();
            list_command_fields.Add((int)GRAPHIC_SCADA_COMMANDS.STOW);
            Manage_logs.SaveLogValue($"ENQUEUE GRAPHIC COMMAND (STOW)");
            m_queue_commands.Enqueue(list_command_fields);
        }

        private void Button_offline_Click(object sender, RoutedEventArgs e)
        {
            List<int> list_command_fields = new();
            list_command_fields.Add((int)GRAPHIC_SCADA_COMMANDS.OFFLINE);
            Manage_logs.SaveLogValue($"ENQUEUE GRAPHIC COMMAND (OFFLINE)");
            m_queue_commands.Enqueue(list_command_fields);
        }

        private void Button_online_Click(object sender, RoutedEventArgs e)
        {
            List<int> list_command_fields = new();
            list_command_fields.Add((int)GRAPHIC_SCADA_COMMANDS.ONLINE);
            Manage_logs.SaveLogValue($"ENQUEUE GRAPHIC COMMNAD (ONLINE)");
            m_queue_commands.Enqueue(list_command_fields);
        }

        private void Button_tracking_Click(object sender, RoutedEventArgs e)
        {
            List<int> list_command_fields = new();
            list_command_fields.Add((int)GRAPHIC_SCADA_COMMANDS.TRACKING);
            Manage_logs.SaveLogValue($"ENQUEUE GRAPHIC COMMNAD (TRACKING)");
            m_queue_commands.Enqueue(list_command_fields);
        }

        #endregion


        #region SAMCA COMMANDS

        private void Button_samca_stow_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_samca_track_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #endregion


        #region Table mode

        #region Slave modbus selection changed

        private void Listview_slave_modbus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (Listview_slave_modbus.SelectedItems.Count > 0)
            {
                if (Listview_slave_modbus.SelectedItem is TCPModbusSlaveEntry)
                {
                    TCPModbusSlaveEntry entry = Listview_slave_modbus.SelectedItem as TCPModbusSlaveEntry;
                    Select_slave_var(entry);
                }
            }
        }

        #endregion

        #region Check all slaves

        private void Checkbox_all_slave_Checked(object sender, RoutedEventArgs e)
        {
            if (Globals.GetTheInstance().List_modbus_slave_entry != null)
                Select_slave_var(null);
        }


        private void Select_slave_var(TCPModbusSlaveEntry entry)
        {
            m_selected_slave_entry = entry;

            m_collection_modbus_var.Clear();

            List<TCPModbusSlaveEntry> list_entry = Globals.GetTheInstance().List_modbus_slave_entry;
            if (entry != null)
            {
                Manage_logs.SaveLogValue("Selected modbus slave to filter var -> " + entry.Name);

                list_entry = list_entry.Where(modbus_slave_entry => modbus_slave_entry.Name == entry.Name).ToList();
                Checkbox_all_slave.IsChecked = false;
            }


            list_entry.ForEach(modbus_slave_entry =>
            {
                modbus_slave_entry.List_modbus_var.ForEach(modbus_slave_var => m_collection_modbus_var.Add(modbus_slave_var));
            });


            m_collection_modbus_var = new ObservableCollection<TCPModbusVarEntry>(m_collection_modbus_var.OrderBy(modbus_var => modbus_var.Slave).ThenBy(modbus_var => modbus_var.DirModbus));
            Listview_read_modbus.ItemsSource = m_collection_modbus_var;
            Listview_read_modbus.Items.Refresh();
        }

        #endregion

        #endregion


        #region TCU control mode

        #region Codif status actions

        private void Listview_codif_state_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as ListView).SelectedItem;
            if (item != null)
            {
                TCUCodifiedStatusEntry selected_tcu_encode = item as TCUCodifiedStatusEntry;
                TCPModbusSlaveEntry? modbus_slave_entry = m_collection_modbus_slave.FirstOrDefault(modbus_slave => modbus_slave.Slave_type == SLAVE_TYPE.TCU);

                SettingVarCodifiedStatusWindow codified_window = new();
                codified_window.TCU_codified_status_entry = selected_tcu_encode;
                codified_window.Slave_entry = modbus_slave_entry;
                if (codified_window.ShowDialog() == true)
                {
                    int index = Globals.GetTheInstance().List_tcu_codified_status.FindIndex(tcu_encode => tcu_encode.Name == selected_tcu_encode.Name);
                    Globals.GetTheInstance().List_tcu_codified_status[index] = codified_window.TCU_codified_status_entry;

                    bool save_ok = Manage_file.Save_tcu_decodified_entries();
                    if (!save_ok)
                        MessageBox.Show("Error saving TCU codified entry", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);

                    else
                    {
                        Listview_tcu_codified_status.ItemsSource = Globals.GetTheInstance().List_tcu_codified_status.OrderBy(tcu_encode => tcu_encode.DirModbus);
                        Listview_tcu_codified_status.Items.Refresh();
                    }
                }
            }
        }

        private void Button_new_codif_state_Click(object sender, RoutedEventArgs e)
        {
            SettingVarCodifiedStatusWindow codified_window = new();

            codified_window.TCU_codified_status_entry = new()
            {
                SCS_record = false,
                Status_mask_enable = false,
                List_status_mask = new List<string>(),
                DirModbus = Constants.index_no_selected,
                Link_to_graphic = (int)LINK_TO_GRAPHIC.NONE
            };

            if (codified_window.ShowDialog() == true)
            {
                Globals.GetTheInstance().List_tcu_codified_status.Add(codified_window.TCU_codified_status_entry);
                bool save_ok = Manage_file.Save_tcu_decodified_entries();
                if (!save_ok)
                {
                    MessageBox.Show("Error saving new TCU encode entry", "INFO", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                else
                {
                    Listview_tcu_codified_status.ItemsSource = Globals.GetTheInstance().List_tcu_codified_status.OrderBy(tcu_encode => tcu_encode.DirModbus);
                    Listview_tcu_codified_status.Items.Refresh();
                }
            }
        }

        private void Button_remove_codif_state_Click(object sender, RoutedEventArgs e)
        {
            var item = Listview_tcu_codified_status.SelectedItem;

            if (item != null)
            {
                TCUCodifiedStatusEntry encode_state_entry = item as TCUCodifiedStatusEntry;

                Globals.GetTheInstance().List_tcu_codified_status.Remove(encode_state_entry);
                bool save_ok = Manage_file.Save_tcu_decodified_entries();
                if (!save_ok)
                {
                    MessageBox.Show("Error deleting selected encode entry", "INFO", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                else
                {
                    Listview_tcu_codified_status.ItemsSource = Globals.GetTheInstance().List_tcu_codified_status.OrderBy(tcu_encode => tcu_encode.DirModbus);
                    Listview_tcu_codified_status.Items.Refresh();
                }
            }
        }

        #endregion

        #region Codified status mask

        private void Gridview_mask_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            TCUCodifiedStatusEntry codified_entry = (TCUCodifiedStatusEntry)btn.DataContext;

            BitStatusWindow bit_status = new()
            {
                Owner = this
            };

            BlurEffect blurEffect = new()
            {
                Radius = 3
            };

            Effect = blurEffect;

            bit_status.TCU_codified_status_entry = codified_entry;
            bit_status.ShowDialog();
            Effect = null;
        }

        #endregion

        #region Commands

        #region List manual commands selection changed

        private void Listview_tcu_commands_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (sender as ListView).SelectedItem;
            if (item != null)
            {
                WrapPanelTCUParameters.Children.Clear();
                m_array_label_tcu_command_param.Clear();
                m_array_decimal_tcu_command_param.Clear();

                m_selected_command = item as TCUCommand;

                int index = 0;

                if (m_selected_command.Num_params != 0)
                {
                    do
                    {
                        WrapPanel wrap_param = new()
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(0, 2, 0, 2),
                            Height = 28,
                        };

                        Label label_tcu_command = new()
                        {
                            Style = Application.Current.Resources["Label_setting"] as Style,
                            Width = 160,
                            Content = m_selected_command.Name_params[index].ToString()
                        };
                        wrap_param.Children.Add(label_tcu_command);

                        m_array_label_tcu_command_param.Add(label_tcu_command);


                        Tuple<decimal, decimal> tuple_min_max = Functions.TypeCode_min_max(m_selected_command.Type_params[index]);

                        decimal stepsize = m_selected_command.Type_params[index] == TypeCode.Single ? (decimal)0.1 : (decimal)1;

                        DecimalUpDown decimal_tcu_command = new()
                        {
                            Style = Application.Current.Resources["DecimalUpDownStyle"] as Style,
                            DisplayLength = 8,
                            MinValue = tuple_min_max.Item1,
                            MaxValue = tuple_min_max.Item2,
                            Value = 0,
                            IsReadOnly = false,
                            StepSize = stepsize
                        };
                        wrap_param.Children.Add(decimal_tcu_command);

                        m_array_decimal_tcu_command_param.Add(decimal_tcu_command);


                        WrapPanelTCUParameters.Children.Add(wrap_param);
                    }
                    while (++index < m_selected_command.Num_params);
                }
            }
        }

        #endregion

        #region Send command

        private void Button_send_command_Click(object sender, RoutedEventArgs e)
        {
            if (m_selected_command != null)
            {
                List<int> list_command_fields = new();
                list_command_fields.Add(m_selected_command.Index);

                int index = 0;
                if (m_selected_command.Num_params != 0)
                {
                    do
                    {
                        switch (m_selected_command.Type_params[index])
                        {
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                                {
                                    list_command_fields.Add((int)m_array_decimal_tcu_command_param[index].Value);
                                    break;
                                }

                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                                {
                                    int param_value = (int)m_array_decimal_tcu_command_param[index].Value;
                                    byte[] byte_send_parameter = BitConverter.GetBytes(param_value);
                                    ushort first_send_parameter = BitConverter.ToUInt16(byte_send_parameter, 0);
                                    ushort second_send_parameter = BitConverter.ToUInt16(byte_send_parameter, 2);
                                    list_command_fields.Add((int)first_send_parameter);
                                    list_command_fields.Add((int)second_send_parameter);

                                    break;
                                }
                        }
                    }
                    while (++index < m_selected_command.Num_params);
                }

                string s_data = string.Empty;
                list_command_fields.ForEach(value => s_data += value.ToString() + " ");
                Manage_logs.SaveLogValue($"ENQUEUE COMMAND ->  { s_data }");

                m_queue_commands.Enqueue(list_command_fields);
            }
        }

        #endregion

        #endregion

        #endregion



        #region Load Modbus slave / var

        private bool Load_modbus_slave_var()
        {
            bool load_modbus_ok = true;

            bool read_slave_ok = Manage_file.Load_modbus_slave_entries();
            string s_log = read_slave_ok ? "Read modbus slave entries OK" : "Error reading modbus slave entries";
            Manage_logs.SaveLogValue(s_log);

            bool read_var_map_ok = Manage_file.Load_var_map_entries();
            s_log = read_slave_ok ? "Read var map entries OK" : "Error reading var map entries";
            Manage_logs.SaveLogValue(s_log);

            load_modbus_ok = read_slave_ok && read_var_map_ok;

            try
            {
                if (load_modbus_ok)
                {
                    #region Slave - Var map

                    string s_slave_var_log = "SLAVE - VAR CONFIG " + "\r\n" + "-----------------------" + "\r\n";

                    m_collection_modbus_slave = new ObservableCollection<TCPModbusSlaveEntry>();
                    m_collection_modbus_var = new ObservableCollection<TCPModbusVarEntry>();

                    m_keyValuePair_link_to_send_tcu_value.ForEach(x => x.Value.Content = string.Empty);
                    m_keyValuePair_link_to_send_tcu_unit.ForEach(x => x.Value.Content = string.Empty);
                    m_keyValuePair_link_to_send_tcu_graphic.ForEach(x => x.Value.Content = string.Empty);

                    Globals.GetTheInstance().List_modbus_slave_entry.ForEach(modbus_slave_entry =>
                    {
                        m_collection_modbus_slave.Add(modbus_slave_entry);
                        s_slave_var_log += modbus_slave_entry.Name + "\r\n" + "-----------------------" + "\r\n";

                        modbus_slave_entry.List_modbus_var.ForEach(modbus_slave_var =>
                        {
                            m_collection_modbus_var.Add(modbus_slave_var);

                            s_slave_var_log += modbus_slave_var.Name + "\r\n";

                            if (modbus_slave_var.Graphic_pos != Constants.index_no_selected)
                                m_array_label_graphic_title[modbus_slave_var.Graphic_pos].Background = Brushes.Black;


                            if ((LINK_TO_SEND_TCU)modbus_slave_var.Link_to_send_tcu != LINK_TO_SEND_TCU.NONE)
                                m_keyValuePair_link_to_send_tcu_unit.First(x => x.Key == (LINK_TO_SEND_TCU)modbus_slave_var.Link_to_send_tcu).Value.Content = modbus_slave_var.Unit;

                        });

                        s_slave_var_log += "------------------------------" + "\r\n";
                    });

                    Manage_logs.SaveLogValue(s_slave_var_log);


                    #region Refresh lists

                    m_collection_modbus_slave = new ObservableCollection<TCPModbusSlaveEntry>(m_collection_modbus_slave.OrderBy(modbus_slave => modbus_slave.Name));
                    Listview_slave_modbus.ItemsSource = m_collection_modbus_slave;
                    Listview_slave_modbus.Items.Refresh();

                    m_collection_modbus_var = new ObservableCollection<TCPModbusVarEntry>(m_collection_modbus_var.OrderBy(modbus_var => modbus_var.Slave).ThenBy(modbus_var => modbus_var.DirModbus));
                    Listview_read_modbus.ItemsSource = m_collection_modbus_var;
                    Listview_read_modbus.Items.Refresh();

                    #endregion

                    #endregion

                    #region Slave_controls into Graphic mode

                    Wrap_slave_state.Children.Clear();
                    m_array_ellipse_slave.Clear();
                    m_collection_modbus_slave.ToList().ForEach(slave =>
                    {
                        System.Windows.Shapes.Ellipse ellipse_slave = new();
                        ellipse_slave.Margin = new Thickness(5, 0, 5, 0);
                        ellipse_slave.Width = 15;
                        ellipse_slave.Height = 15;
                        ellipse_slave.Stroke = Brushes.Black;
                        ellipse_slave.StrokeThickness = 1;
                        ellipse_slave.VerticalAlignment = VerticalAlignment.Center;
                        ellipse_slave.Fill = Brushes.Red;
                        ellipse_slave.Cursor = Cursors.Hand;
                        ellipse_slave.MouseEnter += new MouseEventHandler(EllipseOnMouseEnter);
                        ellipse_slave.MouseLeave += new MouseEventHandler(EllipseOnMouseLeave);
                        ellipse_slave.MouseDown += new MouseButtonEventHandler(EllipseOnMouseDown);

                        Wrap_slave_state.Children.Add(ellipse_slave);
                        m_array_ellipse_slave.Add(new KeyValuePair<string, System.Windows.Shapes.Ellipse>(slave.Name, ellipse_slave));
                    });


                    #endregion

                    #region Start manage thread

                    Globals.GetTheInstance().List_manage_thread = new List<Manage_thread>();
                    Globals.GetTheInstance().List_modbus_slave_entry.ForEach(slave_entry =>
                    {
                        Manage_thread manage_thread = new();
                        manage_thread.TCP_modbus_slave_entry = slave_entry;
                        Globals.GetTheInstance().List_manage_thread.Add(manage_thread);
                    });

                    #endregion
                }
            }
            catch (Exception ex)
            {
                load_modbus_ok = false;
                Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Load_modbus_slave_var)} -> {ex.Message}");
            }

            return load_modbus_ok;
        }

        #endregion


        #region Ellipse associated with Slaves events

        private void EllipseOnMouseEnter(object sender, MouseEventArgs e)
        {
            string slave_name = m_array_ellipse_slave.First(x => x.Value == (System.Windows.Shapes.Ellipse)sender).Key;
            TextblockPopUp.Text = slave_name;
            PopUpSlaves.IsOpen = true;
        }

        private void EllipseOnMouseLeave(object sender, MouseEventArgs e)
        {
            PopUpSlaves.IsOpen = false;
        }

        private void EllipseOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            string slave_name = m_array_ellipse_slave.First(x => x.Value == (System.Windows.Shapes.Ellipse)sender).Key;
            System.Windows.Shapes.Ellipse slave_ellipse = (System.Windows.Shapes.Ellipse)sender;

            TCPModbusSlaveEntry slave_entry = Globals.GetTheInstance().List_modbus_slave_entry.First(modbus_slave => modbus_slave.Name == slave_name);

            if (!slave_entry.Connected)
            {
                if (slave_entry.Field_safety_enable)
                {
                    MessageBoxResult result = MessageBox.Show("¿Deshabilitar Slave para BIT FIELD SAFETY?", "INFO", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == MessageBoxResult.Yes)
                    {
                        slave_entry.Field_safety_enable = false;
                        slave_ellipse.Fill = Brushes.Blue;
                    }
                }
                else {
                    MessageBoxResult result = MessageBox.Show("¿Habilitar Slave para BIT FIELD SAFETY?", "INFO", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == MessageBoxResult.Yes)
                    {
                        slave_entry.Field_safety_enable = true;
                        slave_ellipse.Fill = Brushes.Red;
                    }
                }
            }
        }
        #endregion


        #region Start / Stop TCP modbus

        #region Checkbox start - stop

        private void Checkbox_start_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                b_manual_start_stop = true;
                b_individual_start_stop = false;

                if (!b_individual_start_stop)
                {
                    m_list_manage_thread_in_start_process.Clear();
                    Globals.GetTheInstance().List_manage_thread.ForEach(manage_thread => m_list_manage_thread_in_start_process.Add(manage_thread));

                    Border_wait.Visibility = Visibility.Visible;
                    m_timer_start_tcp.Start();
                }
            }
            catch (Exception ex)
            {
                Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Checkbox_start_Checked)} -> {ex.Message}");
            }
        }

        private void Checkbox_start_Unchecked(object sender, RoutedEventArgs e)
        {
            b_manual_start_stop = true;
            b_individual_start_stop = false;

            m_queue_reconnect_slave.Clear();

            if (!b_individual_start_stop)
                Globals.GetTheInstance().List_manage_thread.ForEach(manage => manage.Stop_tcp_com_thread());

        }

        #endregion

        #region Start stop in gridview
        private void Gridview_state_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                TCPModbusSlaveEntry slaveEntry = (TCPModbusSlaveEntry)btn.DataContext;

                b_manual_start_stop = true;
                b_individual_start_stop = true;

                Manage_thread manageThread = Globals.GetTheInstance().List_manage_thread.First(manage_thread => manage_thread.TCP_modbus_slave_entry.Name == slaveEntry.Name);
                if (!manageThread.ManageTCP.Is_connected())
                {
                    m_list_manage_thread_in_start_process.Clear();
                    m_list_manage_thread_in_start_process.Add(manageThread);
                    Border_wait.Visibility = Visibility.Visible;
                    m_timer_start_tcp.Start();
                }
                else
                    manageThread.Stop_tcp_com_thread();
            }
            catch (Exception ex)
            {
                Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Gridview_state_Click)} -> {ex.Message}");
            }
        }

        #endregion

        #region Timer start

        private void Timer_start_tcp_Tick(object sender, EventArgs e)
        {
            b_first_read_scs_finish = false;
            b_first_read_tcu_finish = false;
            b_first_read_samca_finish = false;

            m_timer_start_tcp.Stop();

            m_list_manage_thread_in_start_process.ForEach(manage => manage.Start_tcp_com(false));
        }

        #endregion

        #region Check ini read slaves

        private void Check_ini_read_slaves(string slave_name)
        {
            try
            {
                m_list_manage_thread_in_start_process.RemoveAll(Manage_thread => Manage_thread.TCP_modbus_slave_entry.Name == slave_name);
                if (m_list_manage_thread_in_start_process.Count == 0)
                {
                    b_read_scs_modbus = flag_read_normal = flag_read_fast = flag_read_tcu = flag_write_tcu_watchdog = flag_write_tcu_datetime = b_slaves_ok = Globals.GetTheInstance().List_modbus_slave_entry.Exists(slave_entry => slave_entry.Connected);

                    string s_log = b_read_scs_modbus ? "INICIO PROCESO DE LECTURA SLAVES" : "ERROR EN EL INICIO PROCESO DE LECTURA SLAVES. NINGUN SLAVE CONECTADO";
                    Manage_logs.SaveLogValue(s_log);

                    if (b_read_scs_modbus)
                    {
                        Dispatcher.Invoke(() => StartAvgValues());
                        b_manual_start_stop = false;
                        Dispatcher.Invoke(() => ((Storyboard)Resources["BlinkStoryboard"]).Begin());
                    }

                    Dispatcher.Invoke(() => Checkbox_start.IsChecked = b_read_scs_modbus);
                    Dispatcher.Invoke(() => Border_wait.Visibility = Visibility.Collapsed);
                }

                Dispatcher.Invoke(() => Listview_slave_modbus.Items.Refresh());
            }
            catch (Exception ex)
            {
                Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Check_ini_read_slaves)} -> {ex.Message}");
            }
        }

        #endregion

        #endregion



        #region Timer read SCS modbus

        private void Timer_read_scs_normal_modbus_Tick(object sender, EventArgs e)
        {
            if (b_read_scs_modbus && flag_read_normal)
            {
                flag_read_normal = false;

                Read_scs_modbus(false);
                b_first_read_scs_finish = true;
                b_first_read_samca_finish = true;

                flag_read_normal = true;
            }
        }

        private void Timer_read_scs_fast_modbus_Tick(object sender, EventArgs e)
        {
            if (b_read_scs_modbus && flag_read_fast)
            {
                flag_read_fast = false;

                Read_scs_modbus(true);

                flag_read_fast = true;
            }
        }


        private void Read_scs_modbus(bool fast_mode)
        {
            try
            {
                //Analizar si está filtrado un slave
                List<Manage_thread> list_read_thread = Globals.GetTheInstance().List_manage_thread;
                if (m_selected_slave_entry != null)
                    list_read_thread = list_read_thread.Where(manage_thread => manage_thread.TCP_modbus_slave_entry.Name == m_selected_slave_entry.Name).ToList();


                list_read_thread.ForEach(manage_thread =>
                {
                    if (manage_thread.ManageTCP.Is_connected())
                    {
                        if (manage_thread.TCP_modbus_slave_entry.Fast_mode == fast_mode)
                        {
                            Tuple<READ_STATE, int[]> tuple_read = null;
                            if (manage_thread.TCP_modbus_slave_entry.Modbus_function == MODBUS_FUNCION.READ_HOLDING_REG)
                                tuple_read = manage_thread.Read_holding_registers_int32();
                            else
                                tuple_read = manage_thread.Read_input_registers_int32();

                            if (tuple_read.Item1 == READ_STATE.OK)
                            {
                                if (tuple_read.Item2.Length != 0)
                                {
                                    List<TCPModbusVarEntry> list_var_entry = m_collection_modbus_var.Where(modbus_var => modbus_var.Slave == manage_thread.TCP_modbus_slave_entry.Name).ToList();

                                    list_var_entry.ForEach(var_entry =>
                                    {
                                        if (var_entry.DirModbus < manage_thread.TCP_modbus_slave_entry.Read_reg)
                                        {
                                            var_entry.Value = Functions.Read_from_array_convert_scale(tuple_read.Item2, var_entry.DirModbus, var_entry.Name, var_entry.TypeVar, var_entry.Read_range_min, var_entry.Scaled_range_min, var_entry.Scale_factor);
                                            var_entry.Value += var_entry.Offset;


                                            //CORRECTION LOAD PIN
                                            if (var_entry.Correction_load_pin)
                                            {
                                                double key_value_second = Globals.GetTheInstance().List_slope_correction_alphaTT.FirstOrDefault(x => x >= double.Parse(var_entry.Value, Globals.GetTheInstance().nfi));
                                                int index_second = Globals.GetTheInstance().List_slope_correction_alphaTT.FindIndex(x => x == key_value_second);
                                                double key_value_first = index_second == 0 ? key_value_second : Globals.GetTheInstance().List_slope_correction_alphaTT[index_second - 1];

                                                double factor =
                                                Globals.GetTheInstance().Dictionary_slope_correction[key_value_first] +
                                                ((Globals.GetTheInstance().Dictionary_slope_correction[key_value_second] - Globals.GetTheInstance().Dictionary_slope_correction[key_value_first]) /
                                                (key_value_second - key_value_first)) *
                                                (double.Parse(var_entry.Value, Globals.GetTheInstance().nfi) - key_value_first);

                                                var_entry.Value = Math.Round(double.Parse(var_entry.Value, Globals.GetTheInstance().nfi) / factor, 6).ToString("0.00", Globals.GetTheInstance().nfi);
                                            }


                                            #region AVG LINKADO -> Mas adelante Vamos a realizar este proceso en un thread aparte

                                            //WIND AVG VALUES
                                            if ((LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.WIND_AVG_SBPT || (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.WIND_AVG_SAMCA)
                                            {
                                                Tuple<DateTime, double> read_value = new Tuple<DateTime, double>(DateTime.Now, double.Parse(var_entry.Value, Globals.GetTheInstance().nfi));

                                                switch ((LINK_TO_AVG)var_entry.Link_to_avg)
                                                {
                                                    case LINK_TO_AVG.WIND_AVG_SBPT:
                                                        {
                                                            m_list_wind_read_values[(int)WIND_AVG_POSITION.SBPT_3SEC].Add(read_value);
                                                            m_list_wind_read_values[(int)WIND_AVG_POSITION.SBPT_10MIN].Add(read_value);

                                                            m_list_wind_read_values[(int)WIND_AVG_POSITION.SBPT_3SEC] = m_list_wind_read_values[(int)WIND_AVG_POSITION.SBPT_3SEC].Where(p => DateTime.Now.Subtract(p.Item1) < TimeSpan.FromSeconds((int)WIND_AVG_INTERVAL.SHORT_SEC)).ToList();
                                                            m_list_wind_read_values[(int)WIND_AVG_POSITION.SBPT_10MIN] = m_list_wind_read_values[(int)WIND_AVG_POSITION.SBPT_10MIN].Where(p => DateTime.Now.Subtract(p.Item1) < TimeSpan.FromMinutes((int)WIND_AVG_INTERVAL.LONG_MIN)).ToList();

                                                            m_array_wind_avg_values[(int)WIND_AVG_POSITION.SBPT_3SEC] = m_list_wind_read_values[(int)WIND_AVG_POSITION.SBPT_3SEC].Select(p => p.Item2).Average();
                                                            m_array_wind_avg_values[(int)WIND_AVG_POSITION.SBPT_10MIN] = m_list_wind_read_values[(int)WIND_AVG_POSITION.SBPT_10MIN].Select(p => p.Item2).Average();

                                                            Dispatcher.Invoke(() => m_array_label_wind_avg_value[(int)WIND_AVG_POSITION.SBPT_3SEC].Content = Math.Round(m_array_wind_avg_values[(int)WIND_AVG_POSITION.SBPT_3SEC], 2).ToString(Globals.GetTheInstance().nfi));
                                                            Dispatcher.Invoke(() => m_array_label_wind_avg_value[(int)WIND_AVG_POSITION.SBPT_10MIN].Content = Math.Round(m_array_wind_avg_values[(int)WIND_AVG_POSITION.SBPT_10MIN], 2).ToString(Globals.GetTheInstance().nfi));

                                                            break;
                                                        }

                                                    case LINK_TO_AVG.WIND_AVG_SAMCA:
                                                        {
                                                            m_list_wind_read_values[(int)WIND_AVG_POSITION.SAMCA_3SEC].Add(read_value);
                                                            m_list_wind_read_values[(int)WIND_AVG_POSITION.SAMCA_10MIN].Add(read_value);

                                                            m_list_wind_read_values[(int)WIND_AVG_POSITION.SAMCA_3SEC] = m_list_wind_read_values[(int)WIND_AVG_POSITION.SAMCA_3SEC].Where(p => DateTime.Now.Subtract(p.Item1) < TimeSpan.FromSeconds((int)WIND_AVG_INTERVAL.SHORT_SEC)).ToList();
                                                            m_list_wind_read_values[(int)WIND_AVG_POSITION.SAMCA_10MIN] = m_list_wind_read_values[(int)WIND_AVG_POSITION.SAMCA_10MIN].Where(p => DateTime.Now.Subtract(p.Item1) < TimeSpan.FromMinutes((int)WIND_AVG_INTERVAL.LONG_MIN)).ToList();

                                                            m_array_wind_avg_values[(int)WIND_AVG_POSITION.SAMCA_3SEC] = m_list_wind_read_values[(int)WIND_AVG_POSITION.SAMCA_3SEC].Select(p => p.Item2).Average();
                                                            m_array_wind_avg_values[(int)WIND_AVG_POSITION.SAMCA_10MIN] = m_list_wind_read_values[(int)WIND_AVG_POSITION.SAMCA_10MIN].Select(p => p.Item2).Average();

                                                            Dispatcher.Invoke(() => m_array_label_wind_avg_value[(int)WIND_AVG_POSITION.SAMCA_3SEC].Content = Math.Round(m_array_wind_avg_values[(int)WIND_AVG_POSITION.SAMCA_3SEC], 2).ToString(Globals.GetTheInstance().nfi));
                                                            Dispatcher.Invoke(() => m_array_label_wind_avg_value[(int)WIND_AVG_POSITION.SAMCA_10MIN].Content = Math.Round(m_array_wind_avg_values[(int)WIND_AVG_POSITION.SAMCA_10MIN], 2).ToString(Globals.GetTheInstance().nfi));

                                                            break;
                                                        }
                                                }
                                            }

                                            //CHECK WIND MAX VALUE
                                            Dispatcher.Invoke(() =>
                                            {
                                                if (manage_thread.ManageTCP.Is_connected())
                                                {
                                                    m_array_label_wind_avg_value
                                                    .Select((value, index) => new { Container = value, Position = index }).ToList()
                                                    .ForEach(label =>
                                                    {
                                                        if (m_array_label_wind_avg_value[label.Position].Content != null)
                                                        {
                                                        //No se ha superado el valor máximo -> Analizar sic se supera
                                                        if (m_array_wind_max_break_in_range[label.Position])
                                                            {
                                                                bool check_wind = double.Parse(m_array_label_wind_avg_max[label.Position].Content.ToString(), NumberStyles.Any, Globals.GetTheInstance().nfi) != 0; //Valor MAX deshabilitado
                                                            if (check_wind)
                                                                {
                                                                    if (
                                                                    double.TryParse(m_array_label_wind_avg_value[label.Position].Content.ToString(), NumberStyles.Any, Globals.GetTheInstance().nfi, out double result_var) &&
                                                                    double.TryParse(m_array_label_wind_avg_max[label.Position].Content.ToString(), NumberStyles.Any, Globals.GetTheInstance().nfi, out double result_max))
                                                                    {
                                                                        m_array_wind_max_break_in_range[label.Position] = result_var <= result_max;

                                                                        if (!m_array_wind_max_break_in_range[label.Position])
                                                                        {
                                                                            Manage_logs.SaveLogValue($"SE HA SUPERADO LA WIND MAX VEL PERMITIDA PARA LA MEDIA ->  {(WIND_AVG_POSITION)label.Position} / " +
                                                                                $"VELOCIDAD MEDIA REG -> { m_array_wind_avg_values[label.Position] } / " +
                                                                                $"VELOCIDAD MAX PERMITIDA -> {m_array_label_wind_avg_max[label.Position].Content.ToString()}");

                                                                            m_array_border_wind_avg_value[label.Position].BorderBrush = Brushes.Red;
                                                                            m_array_label_wind_avg_value[label.Position].Foreground = Brushes.Red;


                                                                            if (label.Position == (int)WIND_AVG_POSITION.SBPT_3SEC)
                                                                                m_array_date_trigger_start_3sec[(int)WIND_DATE_TRIGGER_3SEC.SBPT_3SEC] = DateTime.Now;

                                                                            if (label.Position == (int)WIND_AVG_POSITION.SAMCA_3SEC)
                                                                                m_array_date_trigger_start_3sec[(int)WIND_DATE_TRIGGER_3SEC.SAMCA_3SEC] = DateTime.Now;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    });
                                                }
                                            });


                                            //CHECK TRIGGER DELAY or LOW HISTERESIS
                                            Dispatcher.Invoke(() =>
                                            {
                                                if (manage_thread.ManageTCP.Is_connected())
                                                {
                                                    m_array_wind_max_break_in_range.Select((value, index) => new { Value = value, Position = index }).ToList()
                                                    .ForEach(wind_break_in_range =>
                                                    {
                                                    //Se habia superado valor máximo -> analizar vuelta a la normalidad
                                                    if (!wind_break_in_range.Value)
                                                        {
                                                            bool wind_ok = false;

                                                            string s_add_info = string.Empty;
                                                            switch ((WIND_AVG_POSITION)wind_break_in_range.Position)
                                                            {
                                                                case WIND_AVG_POSITION.SBPT_3SEC:
                                                                    {
                                                                        if (m_array_wind_avg_values[wind_break_in_range.Position] < Globals.GetTheInstance().SBPT_trigger_3sec)
                                                                        {
                                                                            wind_ok = DateTime.Now.Subtract(m_array_date_trigger_start_3sec[(int)WIND_DATE_TRIGGER_3SEC.SBPT_3SEC]) > TimeSpan.FromMinutes(Globals.GetTheInstance().SBPT_wind_delay_time_3sec);

                                                                            s_add_info += $"TIEMPO DELAY -> {Globals.GetTheInstance().SBPT_wind_delay_time_3sec} MIN";
                                                                        }
                                                                        else
                                                                            m_array_date_trigger_start_3sec[(int)WIND_DATE_TRIGGER_3SEC.SBPT_3SEC] = DateTime.Now;

                                                                        break;
                                                                    }

                                                                case WIND_AVG_POSITION.SAMCA_3SEC:
                                                                    {
                                                                        if (m_array_wind_avg_values[wind_break_in_range.Position] < Globals.GetTheInstance().SAMCA_trigger_3sec)
                                                                        {
                                                                            wind_ok = DateTime.Now.Subtract(m_array_date_trigger_start_3sec[(int)WIND_DATE_TRIGGER_3SEC.SAMCA_3SEC]) > TimeSpan.FromMinutes(Globals.GetTheInstance().SAMCA_wind_delay_time_3sec);
                                                                            s_add_info += $"TIEMPO DELAY -> {Globals.GetTheInstance().SAMCA_wind_delay_time_3sec} MIN";
                                                                        }
                                                                        else
                                                                            m_array_date_trigger_start_3sec[(int)WIND_DATE_TRIGGER_3SEC.SAMCA_3SEC] = DateTime.Now;

                                                                        break;
                                                                    }

                                                                case WIND_AVG_POSITION.SBPT_10MIN:
                                                                    {
                                                                        wind_ok = m_array_wind_avg_values[wind_break_in_range.Position] < m_array_low_histeresis_10min[(int)WIND_LOW_HIST_10MIN.SBPT_10MIN];
                                                                        s_add_info += $"VELOCIDAD HISTERESIS -> {m_array_low_histeresis_10min[(int)WIND_LOW_HIST_10MIN.SBPT_10MIN]}";

                                                                        break;
                                                                    }

                                                                case WIND_AVG_POSITION.SAMCA_10MIN:
                                                                    {
                                                                        wind_ok = m_array_wind_avg_values[wind_break_in_range.Position] < m_array_low_histeresis_10min[(int)WIND_LOW_HIST_10MIN.SAMCA_10MIN];
                                                                        s_add_info += $"VELOCIDAD HISTERESIS -> {m_array_low_histeresis_10min[(int)WIND_LOW_HIST_10MIN.SAMCA_10MIN]}";

                                                                        break;
                                                                    }
                                                            }

                                                            if (wind_ok)
                                                            {
                                                                Manage_logs.SaveLogValue($"SE HA BAJADO DE LA VELOCIDAD HISTERESIS PARA LA MEDIA ->  {(WIND_AVG_POSITION)wind_break_in_range.Position} / " +
                                                                     $"VELOCIDAD MEDIA REG -> {m_array_wind_avg_values[wind_break_in_range.Position]} / " +
                                                                     s_add_info);

                                                                m_array_border_wind_avg_value[wind_break_in_range.Position].BorderBrush = Brushes.Black;
                                                                m_array_label_wind_avg_value[wind_break_in_range.Position].Foreground = Brushes.Black;
                                                                m_array_wind_max_break_in_range[wind_break_in_range.Position] = true;
                                                            }
                                                        }
                                                    });
                                                }
                                            });


                                            //INCLINOMETER AVG VALUES
                                            bool inc_slope_avg_value = false;
                                            if (inc_slope_avg_value =
                                                ((LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC1_SLOPE_AVG_SBPT ||
                                                (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC2_SLOPE_AVG_SBPT ||
                                                (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC3_SLOPE_AVG_SBPT ||
                                                (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC4_SLOPE_AVG_SBPT ||
                                                (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC5_SLOPE_AVG_SBPT))
                                            {
                                                if (!fast_mode)
                                                {
                                                    Tuple<DateTime, double> read_value = new(DateTime.Now, double.Parse(var_entry.Value, Globals.GetTheInstance().nfi));

                                                    INC_LABEL_AVG_POSITION inc_label_pos =
                                                    (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC1_SLOPE_AVG_SBPT ? INC_LABEL_AVG_POSITION.INC1 :
                                                    (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC2_SLOPE_AVG_SBPT ? INC_LABEL_AVG_POSITION.INC2 :
                                                    (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC3_SLOPE_AVG_SBPT ? INC_LABEL_AVG_POSITION.INC3 :
                                                    (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC4_SLOPE_AVG_SBPT ? INC_LABEL_AVG_POSITION.INC4 :
                                                    INC_LABEL_AVG_POSITION.INC5;

                                                    m_list_inc_slope_read_values[(int)inc_label_pos].Add(read_value);
                                                    m_list_inc_slope_read_values[(int)inc_label_pos] = m_list_inc_slope_read_values[(int)inc_label_pos].Where(p => DateTime.Now.Subtract(p.Item1) < TimeSpan.FromSeconds(Globals.GetTheInstance().SBPT_inc_avg_interval)).ToList();
                                                    m_array_inc_slope_avg_values[(int)inc_label_pos] = m_list_inc_slope_read_values[(int)inc_label_pos].Select(p => p.Item2).Average();

                                                    Dispatcher.Invoke(() => m_array_label_inc_slope_avg_value[(int)inc_label_pos].Content = $"{m_array_inc_slope_avg_values[(int)inc_label_pos]} {var_entry.Unit}");

                                                    //Compare with TCU INC VALUE and change inrange arrays
                                                    if (m_list_inc_slope_read_values[(int)INC_LABEL_AVG_POSITION.TCU].Count >= 0 && !m_list_inc_slope_read_values.Exists(x => x.Count == 0))
                                                    {
                                                        m_array_inc_slope_emerg_stow_in_range[(int)inc_label_pos] = (Globals.GetTheInstance().Max_diff_tcu_inc_emergency_stow == 0) ||  (Math.Abs(m_array_inc_slope_avg_values[(int)inc_label_pos] - (m_array_inc_slope_avg_values[(int)INC_LABEL_AVG_POSITION.TCU] / 1000)) < Globals.GetTheInstance().Max_diff_tcu_inc_emergency_stow);
                                                        m_array_inc_slope_alarm_in_range[(int)inc_label_pos] = (Globals.GetTheInstance().Max_diff_tcu_inc_alarm == 0) || (Math.Abs(m_array_inc_slope_avg_values[(int)inc_label_pos] - (m_array_inc_slope_avg_values[(int)INC_LABEL_AVG_POSITION.TCU] / 1000)) < Globals.GetTheInstance().Max_diff_tcu_inc_alarm);
                                                    }
                                                }
                                            }

                                            //DYNANOMETER AVG VALUES
                                            bool dyn_avg_value = false;
                                            if (dyn_avg_value =
                                              ((LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.DYN1_AVG_SBPT ||
                                              (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.DYN2_AVG_SBPT ||
                                              (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.DYN3_AVG_SBPT))
                                            {
                                                //Requerimiento del cliente
                                                double d_value = double.Parse(var_entry.Value, Globals.GetTheInstance().nfi);
                                                if ((LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.DYN3_AVG_SBPT)
                                                    d_value *= -1;
                                                
                                                if (!fast_mode)
                                                {
                                                    Tuple<DateTime, double> read_value = new(DateTime.Now, d_value);

                                                    DYN_LABEL_AVG_POSITION dyn_label_pos =
                                                       (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.DYN1_AVG_SBPT ? DYN_LABEL_AVG_POSITION.DYN1 :
                                                       (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.DYN2_AVG_SBPT ? DYN_LABEL_AVG_POSITION.DYN2 :
                                                       DYN_LABEL_AVG_POSITION.DYN3;

                                                    m_list_dyn_read_values[(int)dyn_label_pos].Add(read_value);
                                                    m_list_dyn_read_values[(int)dyn_label_pos] = m_list_dyn_read_values[(int)dyn_label_pos].Where(p => DateTime.Now.Subtract(p.Item1) < TimeSpan.FromSeconds(Globals.GetTheInstance().SBPT_dyn_avg_interval)).ToList();
                                                    m_array_dyn_avg_values[(int)dyn_label_pos] = m_list_dyn_read_values[(int)dyn_label_pos].Select(p => p.Item2).Average();

                                                    Dispatcher.Invoke(() => m_array_label_dyn_avg_value[(int)dyn_label_pos].Content = $"{m_array_dyn_avg_values[(int)dyn_label_pos]} {var_entry.Unit }");

                                                    //Compare with limits and change inrange arrays
                                                    if (!m_list_dyn_read_values.Exists(x => x.Count == 0))
                                                    {
                                                        bool TCU_MOVING = true; //Todavia no se puede leer del TCU

                                                        if (TCU_MOVING && m_codified_status_state != CODIFIED_STATUS_STATE.NONE)
                                                            m_array_dyn_excesive_force_emerg_stow_in_range[(int)dyn_label_pos] = (Globals.GetTheInstance().SBPT_dyn_max_mov_emerg_stow == 0) || (m_array_dyn_avg_values[(int)dyn_label_pos] < Globals.GetTheInstance().SBPT_dyn_max_mov_emerg_stow);

                                                        if (TCU_MOVING && m_codified_status_state == CODIFIED_STATUS_STATE.STOW || m_codified_status_state == CODIFIED_STATUS_STATE.EMERG_CMD_POS_OS_STOW || m_codified_status_state == CODIFIED_STATUS_STATE.EMERG_AUTO_POS_OS_STOW)
                                                        {
                                                            bool excesive_force_alarm_moving_in_range = (Globals.GetTheInstance().SBPT_dyn_max_mov_alarm == 0) || (m_array_dyn_avg_values[(int)dyn_label_pos] < Globals.GetTheInstance().SBPT_dyn_max_mov_alarm);
                                                            bool excesive_force_alarm_static_in_range = (Globals.GetTheInstance().SBPT_dyn_max_static_alarm == 0) || (m_array_dyn_avg_values[(int)dyn_label_pos] < Globals.GetTheInstance().SBPT_dyn_max_static_alarm);

                                                            m_array_dyn_excesive_force_alarm_in_range[(int)dyn_label_pos] = excesive_force_alarm_moving_in_range && excesive_force_alarm_static_in_range;
                                                        }
                                                    }
                                                }
                                            }

                                            #endregion


                                            //LINK TO SEND TO TCU
                                            if ((LINK_TO_SEND_TCU)var_entry.Link_to_send_tcu != LINK_TO_SEND_TCU.NONE)
                                            {
                                                Dispatcher.Invoke(() =>
                                                {
                                                    if (manage_thread.ManageTCP.Is_connected())
                                                    {
                                                        int list_index = m_keyValuePair_link_to_send_tcu_value
                                                        .Select((value, index) => new { Value = value, Index = index })
                                                        .First(key_value_pair => key_value_pair.Value.Key == (LINK_TO_SEND_TCU)var_entry.Link_to_send_tcu).Index;

                                                        m_keyValuePair_link_to_send_tcu_value[list_index].Value.Content = Math.Round(float.Parse(var_entry.Value, Globals.GetTheInstance().nfi), 2);
                                                        m_keyValuePair_link_to_send_tcu_unit[list_index].Value.Content = var_entry.Unit;
                                                        m_keyValuePair_link_to_send_tcu_graphic[list_index].Value.Content = $"{Math.Round(float.Parse(var_entry.Value, Globals.GetTheInstance().nfi), 2)} {var_entry.Unit}";
                                                    }
                                                });
                                            }


                                            //SHOW VALUES IN GRAPHIC MODE
                                            if (!inc_slope_avg_value && !dyn_avg_value)
                                            {
                                                Dispatcher.Invoke(() =>
                                                {
                                                    if (manage_thread.ManageTCP.Is_connected())
                                                    {
                                                        ControlLinq? control_linq = m_array_label_graphic_title
                                                        .Select((item, index) => new ControlLinq { Value = item.Content.ToString(), Position = index })
                                                        .FirstOrDefault(control => control.Position.Equals(var_entry.Graphic_pos));
                                                        if (control_linq != null)
                                                            m_array_label_graphic_value[control_linq.Position].Content = var_entry.Value + " " + var_entry.Unit;
                                                    }
                                                });
                                            }
                                        }
                                    });
                                }
                            }

                            else if (tuple_read.Item1 == READ_STATE.ERROR && Globals.GetTheInstance().Depur_enable == BIT_STATE.ON)
                                Manage_logs.SaveErrorValue($"Error read image -> {manage_thread.TCP_modbus_slave_entry.Name} / {manage_thread.TCP_modbus_slave_entry.IP_primary} / {manage_thread.TCP_modbus_slave_entry.Port} / {manage_thread.TCP_modbus_slave_entry.Dir_ini} / {manage_thread.TCP_modbus_slave_entry.UnitId}");

                        }
                    }
                });

                Dispatcher.Invoke(() => Listview_read_modbus.Items.Refresh());
            }
            catch (Exception ex)
            {
                Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Read_scs_modbus)} -> { ex.Message}");
            }
        }

        #endregion


        #region Timer read TCU modbus

        private void Timer_read_tcu_modbus_Tick(object sender, EventArgs e)
        {
            if (b_read_write_tcu_modbus && flag_read_tcu)
            {
                flag_read_tcu = false;

                Read_tcu_modbus();
                b_first_read_tcu_finish = true;
                flag_read_tcu = true;
            }
        }

        private void Read_tcu_modbus()
        {
            try
            {
                TCPModbusSlaveEntry? slave_entry = m_collection_modbus_slave.FirstOrDefault(modbus_slave => modbus_slave.Slave_type == SLAVE_TYPE.TCU);
                if (slave_entry != null)
                {
                    Manage_thread manage_thread = Globals.GetTheInstance().List_manage_thread.First(manage_thread => manage_thread.TCP_modbus_slave_entry.Name == slave_entry.Name);
                    if (manage_thread.ManageTCP.Is_connected())
                    {
                        Tuple<READ_STATE, int[]> tuple_read = null;
                        if (slave_entry.Modbus_function == MODBUS_FUNCION.READ_HOLDING_REG)
                            tuple_read = manage_thread.Read_holding_registers_int32();
                        else
                            tuple_read = manage_thread.Read_input_registers_int32();

                        if (tuple_read.Item1 == READ_STATE.OK)
                        {
                            if (tuple_read.Item2.Length != 0)
                            {
                                Globals.GetTheInstance().List_tcu_codified_status.ForEach(codified_status =>
                                {
                                    if (codified_status.DirModbus < manage_thread.TCP_modbus_slave_entry.Read_reg)
                                    {
                                        codified_status.Value = Functions.Read_from_array(tuple_read.Item2, codified_status.DirModbus, codified_status.TypeVar);

                                        if (!string.IsNullOrEmpty(codified_status.Value))
                                        {
                                            if ((LINK_TO_GRAPHIC) codified_status.Link_to_graphic != LINK_TO_GRAPHIC.NONE )
                                            {
                                                Dispatcher.Invoke(() =>
                                                {
                                                    try
                                                    {
                                                        if (manage_thread.ManageTCP.Is_connected())
                                                        {
                                                            //A nivel de BIT
                                                            if ((LINK_TO_GRAPHIC)codified_status.Link_to_graphic == LINK_TO_GRAPHIC.CODIF_BITS_TO_TCU1)
                                                            {
                                                                var drive_bits = Enum.GetValues(typeof(CODIFIED_STATUS_DRIVE_BIT));
                                                                foreach (dynamic drive_bit in drive_bits)
                                                                {
                                                                    switch (drive_bit)
                                                                    {
                                                                        case CODIFIED_STATUS_DRIVE_BIT.MAIN_DRIVE:
                                                                            {
                                                                                Border_main_drive.Background = Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)drive_bit) ? new SolidColorBrush(Color.FromArgb(0xFF, 0x09, 0x09, 0xB5)) : new SolidColorBrush(Color.FromArgb(0x00, 0x09, 0x09, 0xB5));
                                                                                break;
                                                                            }

                                                                        case CODIFIED_STATUS_DRIVE_BIT.MAIN_DRIVE_BACKWARD:
                                                                            {
                                                                                Image_main_drive_backward_enable.Visibility = Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;
                                                                                Image_main_drive_backward_disable.Visibility = Functions.IsBitSetTo0(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;

                                                                                break;
                                                                            }

                                                                        case CODIFIED_STATUS_DRIVE_BIT.MAIN_DRIVE_FORWARD:
                                                                            {
                                                                                Image_main_drive_forward_enable.Visibility = Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;
                                                                                Image_main_drive_forward_disable.Visibility = Functions.IsBitSetTo0(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;
                                                                                break;
                                                                            }

                                                                        case CODIFIED_STATUS_DRIVE_BIT.LOCK1_DRIVE:
                                                                            {
                                                                                Border_lock1.Background = Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)drive_bit) ? new SolidColorBrush(Color.FromArgb(0xFF, 0x09, 0x09, 0xB5)) : new SolidColorBrush(Color.FromArgb(0x00, 0x09, 0x09, 0xB5));
                                                                                break;
                                                                            }

                                                                        case CODIFIED_STATUS_DRIVE_BIT.LOCK1_DRIVE_BACKWARD:
                                                                            {
                                                                                Image_lock1_backward_enable.Visibility = Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;
                                                                                Image_lock1_backward_disable.Visibility = Functions.IsBitSetTo0(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;
                                                                                break;
                                                                            }

                                                                        case CODIFIED_STATUS_DRIVE_BIT.LOCK1_DRIVE_FORWARD:
                                                                            {
                                                                                Image_lock1_forward_enable.Visibility = Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;
                                                                                Image_lock1_forward_disable.Visibility = Functions.IsBitSetTo0(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;
                                                                                break;
                                                                            }

                                                                        case CODIFIED_STATUS_DRIVE_BIT.LOCK2_DRIVE:
                                                                            {
                                                                                Border_lock2.Background = Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)drive_bit) ? new SolidColorBrush(Color.FromArgb(0xFF, 0x09, 0x09, 0xB5)) : new SolidColorBrush(Color.FromArgb(0x00, 0x09, 0x09, 0xB5));
                                                                                break;
                                                                            }

                                                                        case CODIFIED_STATUS_DRIVE_BIT.LOCK2_DRIVE_BACKWARD:
                                                                            {
                                                                                Image_lock2_backward_enable.Visibility = Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;
                                                                                Image_lock2_backward_disable.Visibility = Functions.IsBitSetTo0(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;
                                                                                break;
                                                                            }

                                                                        case CODIFIED_STATUS_DRIVE_BIT.LOCK2_DRIVE_FORWARD:
                                                                            {
                                                                                Image_lock2_forward_enable.Visibility = Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;
                                                                                Image_lock2_forward_disable.Visibility = Functions.IsBitSetTo0(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;
                                                                                break;
                                                                            }
                                                                    }
                                                                }
                                                            }


                                                            //A nivel de valor
                                                            else
                                                            {
                                                                Label current_label = m_keyValuePair_codified_status.First(key_value => key_value.Key ==(LINK_TO_GRAPHIC)codified_status.Link_to_graphic).Value;

                                                                //Estado codificado
                                                                if ((LINK_TO_GRAPHIC)codified_status.Link_to_graphic == LINK_TO_GRAPHIC.ESTADO_CODIF_TCU)
                                                                {
                                                                    if (Enum.IsDefined(typeof(CODIFIED_STATUS_STATE), int.Parse(codified_status.Value)))
                                                                    {
                                                                        CODIFIED_STATUS_STATE m_codified_status_state_last = m_codified_status_state;

                                                                        m_codified_status_state = (CODIFIED_STATUS_STATE)int.Parse(codified_status.Value);

                                                                        current_label.Content =
                                                                        m_codified_status_state == CODIFIED_STATUS_STATE.POWER_ON_DIAGNOSIS ? "POWER ON DIAGNOSIS" :
                                                                        m_codified_status_state == CODIFIED_STATUS_STATE.OFFLINE ? "OFFLINE" :
                                                                        m_codified_status_state == CODIFIED_STATUS_STATE.OFFLINE_WITH_ERROR ? "OFFLINE WITH ERROR" :
                                                                        m_codified_status_state == CODIFIED_STATUS_STATE.ONLINE_WAITING_CMD ? "ONLINE WAITING" :
                                                                        m_codified_status_state == CODIFIED_STATUS_STATE.POSITION_EL_DEG ? "POSITION ELEVATION" :
                                                                        m_codified_status_state == CODIFIED_STATUS_STATE.POSITION_MAIN_DRIVE_MM ? "POSITION MAIN DRIVE" :
                                                                        m_codified_status_state == CODIFIED_STATUS_STATE.STOW ? "STOW" :
                                                                        m_codified_status_state == CODIFIED_STATUS_STATE.TRACKING_SUN_XYZ ? "TRACKING SUN XYZ" :
                                                                        m_codified_status_state == CODIFIED_STATUS_STATE.TRACKING_DEF_XYZ ? "TRACKING DEF XYZ" :
                                                                        m_codified_status_state == CODIFIED_STATUS_STATE.MAINT_CHK_LOCK_1 ? "MAINTENANCE CHK LOCK 1" :
                                                                        m_codified_status_state == CODIFIED_STATUS_STATE.MAINT_CHK_LOCK_2 ? "MAINTENANCE CHK LOCK 1" :
                                                                        m_codified_status_state == CODIFIED_STATUS_STATE.MAIN_CHK_1_LH ? "MAINTENANCE CHK 1 LH" :
                                                                        m_codified_status_state == CODIFIED_STATUS_STATE.DRIVE_SPEED ? "DRIVE SPEED" :
                                                                        m_codified_status_state == CODIFIED_STATUS_STATE.EMERG_CMD_POS_OS_STOW ? "EMERG CMD POS OS STOW" :
                                                                        m_codified_status_state == CODIFIED_STATUS_STATE.EMERG_AUTO_POS_OS_STOW ? "EMERG AUTO POS OS STOW" :
                                                                        string.Empty;

                                                                        if (m_codified_status_state == CODIFIED_STATUS_STATE.OFFLINE_WITH_ERROR && m_codified_status_state_last != CODIFIED_STATUS_STATE.NONE && m_codified_status_state != m_codified_status_state_last)
                                                                        {
                                                                            Functions.Send_alarm_mail("SBP TRACKER ALARM", $"TRACKER LAST STATUS: {m_codified_status_state_last} - CURRENT STATUS : { m_codified_status_state }" );
                                                                        }
                                                                    }


                                                                    else
                                                                        current_label.Content = string.Empty;
                                                                }

                                                                else
                                                                {
                                                                    //TCU Internal inclinometer
                                                                    if ((LINK_TO_GRAPHIC)codified_status.Link_to_graphic == LINK_TO_GRAPHIC.TRACKER_POS_EL)
                                                                    {
                                                                        Tuple<DateTime, double> read_value = new(DateTime.Now, double.Parse(codified_status.Value, Globals.GetTheInstance().nfi));

                                                                        m_list_inc_slope_read_values[(int)INC_LABEL_AVG_POSITION.TCU].Add(read_value);
                                                                        m_list_inc_slope_read_values[(int)INC_LABEL_AVG_POSITION.TCU] = m_list_inc_slope_read_values[(int)INC_LABEL_AVG_POSITION.TCU].Where(p => DateTime.Now.Subtract(p.Item1) < TimeSpan.FromSeconds(Globals.GetTheInstance().SBPT_inc_avg_interval)).ToList();
                                                                        m_array_inc_slope_avg_values[(int)INC_LABEL_AVG_POSITION.TCU] = m_list_inc_slope_read_values[(int)INC_LABEL_AVG_POSITION.TCU].Select(p => p.Item2).Average();

                                                                        current_label.Content = $"{m_array_inc_slope_avg_values[(int)INC_LABEL_AVG_POSITION.TCU]} {codified_status.Unit}";
                                                                    }

                                                                    //Other var
                                                                    else
                                                                    {
                                                                        //Modificar valor por fecha UTC
                                                                        if ((LINK_TO_GRAPHIC)codified_status.Link_to_graphic == LINK_TO_GRAPHIC.FECHA_RTC)
                                                                        {
                                                                            TimeSpan time = TimeSpan.FromSeconds(int.Parse(codified_status.Value));
                                                                            DateTime dateTime = DateTime.UnixEpoch.Add(time);
                                                                            codified_status.Value = dateTime.ToString(Globals.GetTheInstance().Date_format, new CultureInfo(Globals.GetTheInstance().Format_provider));
                                                                        }

                                                                        current_label.Content = $"{codified_status.Value} {codified_status.Unit}";
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    catch (Exception ex) {
                                                        Manage_logs.SaveErrorValue($"{GetType().Name} ->  {nameof(Read_tcu_modbus)} -> CODIFIED_STATUS_POS_GRAPHIC ->  {ex.Message}");
                                                    }
                                                });
                                            }
                                        }
                                    }
                                });
                            }
                        }

                        else if (tuple_read.Item1 == READ_STATE.ERROR && Globals.GetTheInstance().Depur_enable == BIT_STATE.ON)
                            Manage_logs.SaveErrorValue($"Error read image -> {manage_thread.TCP_modbus_slave_entry.Name} / {manage_thread.TCP_modbus_slave_entry.IP_primary} / {manage_thread.TCP_modbus_slave_entry.Port} / {manage_thread.TCP_modbus_slave_entry.Dir_ini} / {manage_thread.TCP_modbus_slave_entry.UnitId}");
                        
                    }

                    Dispatcher.Invoke(() => Listview_tcu_codified_status.Items.Refresh());

                }
            }
            catch (Exception ex)
            {
                Manage_logs.SaveErrorValue($"{GetType().Name} ->  {nameof(Read_tcu_modbus)} -> {ex.Message}");
            }
        }

        #endregion


        #region Timer write TCU watchdog modbus

        private void Timer_write_tcu_watchdog_modbus_Tick(object sender, EventArgs e)
        {
            try
            {
                if (b_read_scs_modbus)
                {
                    //Manage Watchdog - field safety
                    Dispatcher.Invoke(() =>
                    {
                        //SCADA - METEO WATCHDOG
                        m_keyValuePair_link_to_send_tcu_value.First(x => x.Key == LINK_TO_SEND_TCU.SCADA_WD).Value.Content = m_scada_watchdog;
                        m_keyValuePair_link_to_send_tcu_graphic.First(x => x.Key == LINK_TO_SEND_TCU.SCADA_WD).Value.Content = m_scada_watchdog;
                        m_keyValuePair_link_to_send_tcu_value.First(x => x.Key == LINK_TO_SEND_TCU.METEO_WD).Value.Content = m_meteo_watchdog;
                        m_keyValuePair_link_to_send_tcu_graphic.First(x => x.Key == LINK_TO_SEND_TCU.METEO_WD).Value.Content = m_meteo_watchdog;

                        int field_safety_value = 0x00;

                        //AUTOTRACK SCADA
                        field_safety_value =
                            SwitchSlider_autotracking.ToggledState == false ?
                            Functions.SetBitTo0(field_safety_value, (int)FIELD_SAFETY_VALUE_BIT.AUTOTRACK_SCADA) :
                            Functions.SetBitTo1(field_safety_value, (int)FIELD_SAFETY_VALUE_BIT.AUTOTRACK_SCADA);

                        m_array_border_field_safety[(int)FIELD_SAFETY_CHECK.AUTOTRACK_SCADA].Background = SwitchSlider_autotracking.ToggledState == true ? Brushes.DarkBlue : Brushes.White;

                        //WIND CONDITIONS
                        field_safety_value =
                            m_array_wind_max_break_in_range.ToList().Exists(wind_break_in_range => wind_break_in_range == false) ?
                            Functions.SetBitTo1(field_safety_value, (int)FIELD_SAFETY_VALUE_BIT.WIND_CONDITIONS_OK) :
                            Functions.SetBitTo0(field_safety_value, (int)FIELD_SAFETY_VALUE_BIT.WIND_CONDITIONS_OK);

                        m_array_border_field_safety[(int)FIELD_SAFETY_CHECK.WIND_CONDITIONS_OK].Background = m_array_wind_max_break_in_range.ToList().Exists(wind_break_in_range => wind_break_in_range == false) ? Brushes.White : Brushes.DarkBlue;


                        //SLAVE COMMUNICATION
                        field_safety_value =
                            Globals.GetTheInstance().List_modbus_slave_entry.Exists(slave_entry => !slave_entry.Connected && slave_entry.Field_safety_enable) ?
                            Functions.SetBitTo0(field_safety_value, (int)FIELD_SAFETY_VALUE_BIT.SLAVE_COMM_OK) :
                            Functions.SetBitTo1(field_safety_value, (int)FIELD_SAFETY_VALUE_BIT.SLAVE_COMM_OK);

                        m_array_border_field_safety[(int)FIELD_SAFETY_CHECK.SBPT_SLAVES_COMM_OK].Background = Globals.GetTheInstance().List_modbus_slave_entry.Exists(slave_entry => !slave_entry.Connected && slave_entry.Field_safety_enable) ? Brushes.White : Brushes.DarkBlue;

                        bool last_slaves_ok = b_slaves_ok;
                        b_slaves_ok = Globals.GetTheInstance().List_modbus_slave_entry.Exists(slave_entry => !slave_entry.Connected && slave_entry.Field_safety_enable) ? false : true;

                        if (!b_slaves_ok && b_slaves_ok != last_slaves_ok)
                        {
                            string s_slaves_error = string.Empty;
                            Globals.GetTheInstance().List_modbus_slave_entry
                            .Where(slave_entry => !slave_entry.Connected && slave_entry.Field_safety_enable).ToList()
                            .ForEach(slave_entry => s_slaves_error += $"{slave_entry.Name} /");

                            Functions.Send_alarm_mail("SBP TRACKER ALARM", "COMMUNICATION WITH SLAVES HAS BEEN LOST -> " + s_slaves_error);
                        }
                        


                        //EMERGENCY STOW PUSH BUTTON
                        field_safety_value =
                            Toogle_emergency_stop.IsChecked == false ?
                            Functions.SetBitTo0(field_safety_value, (int)FIELD_SAFETY_VALUE_BIT.EMERGENCY_STOW_BUTTON) :
                            Functions.SetBitTo1(field_safety_value, (int)FIELD_SAFETY_VALUE_BIT.EMERGENCY_STOW_BUTTON);

                        m_array_border_field_safety[(int)FIELD_SAFETY_CHECK.SBPT_EMERGENCY_STOW].Background = Toogle_emergency_stop.IsChecked == true ? Brushes.DarkBlue : Brushes.White;

                        //SBPT INC IN RANGES
                        field_safety_value =
                            m_array_inc_slope_emerg_stow_in_range.All(x => x == true) ?
                            Functions.SetBitTo1(field_safety_value, (int)FIELD_SAFETY_VALUE_BIT.INC_IN_RANGES_FOR_EMERGENCY_STOW) :
                            Functions.SetBitTo0(field_safety_value, (int)FIELD_SAFETY_VALUE_BIT.INC_IN_RANGES_FOR_EMERGENCY_STOW);

                        m_array_border_field_safety[(int)FIELD_SAFETY_CHECK.SBPT_INC_IN_RANGES_FOR_EMERG_STOW].Background = m_array_inc_slope_emerg_stow_in_range.All(x => x == true) ? Brushes.DarkBlue : Brushes.White;

                        field_safety_value =
                            m_array_inc_slope_alarm_in_range.All(x => x == true) ?
                            Functions.SetBitTo1(field_safety_value, (int)FIELD_SAFETY_VALUE_BIT.INC_IN_RANGES_FOR_ALARM) :
                            Functions.SetBitTo0(field_safety_value, (int)FIELD_SAFETY_VALUE_BIT.INC_IN_RANGES_FOR_ALARM);

                        m_array_border_field_safety[(int)FIELD_SAFETY_CHECK.SBPT_INC_IN_RANGES_FOR_ALARM].Background = m_array_inc_slope_alarm_in_range.All(x => x == true) ? Brushes.DarkBlue : Brushes.White;



                        //SBPT DYN IN RANGES
                        field_safety_value =
                            m_array_dyn_excesive_force_emerg_stow_in_range.All(x => x == true) ?
                            Functions.SetBitTo1(field_safety_value, (int)FIELD_SAFETY_VALUE_BIT.DYN_IN_RANGES_FOR_EMERGENCY_STOW) :
                            Functions.SetBitTo0(field_safety_value, (int)FIELD_SAFETY_VALUE_BIT.DYN_IN_RANGES_FOR_EMERGENCY_STOW);

                        m_array_border_field_safety[(int)FIELD_SAFETY_CHECK.SBPT_DYN_IN_RANGES_FOR_EMERG_STOW].Background = m_array_dyn_excesive_force_emerg_stow_in_range.All(x => x == true) ? Brushes.DarkBlue : Brushes.White;

                        field_safety_value =
                            m_array_dyn_excesive_force_alarm_in_range.All(x => x == true) ?
                            Functions.SetBitTo1(field_safety_value, (int)FIELD_SAFETY_VALUE_BIT.DYN_IN_RANGES_FOR_ALARM) :
                            Functions.SetBitTo0(field_safety_value, (int)FIELD_SAFETY_VALUE_BIT.DYN_IN_RANGES_FOR_ALARM);

                        m_array_border_field_safety[(int)FIELD_SAFETY_CHECK.SBPT_DYN_IN_RANGES_FOR_ALARM].Background = m_array_dyn_excesive_force_alarm_in_range.All(x => x == true) ? Brushes.DarkBlue : Brushes.White;

                        m_keyValuePair_link_to_send_tcu_value.First(x => x.Key == LINK_TO_SEND_TCU.FIELD_SAFETY).Value.Content = string.Format("0x{0:X4}", field_safety_value);
                        m_keyValuePair_link_to_send_tcu_graphic.First(x => x.Key == LINK_TO_SEND_TCU.FIELD_SAFETY).Value.Content = string.Format("0x{0:X4}", field_safety_value);
                    });
                }

                if (b_read_write_tcu_modbus && flag_write_tcu_watchdog)
                {
                    flag_write_tcu_watchdog = false;
                    Write_tcu_watchdog_modbus();
                    flag_write_tcu_watchdog = true;
                }
            }
            catch (Exception ex)
            {
                Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Timer_write_tcu_watchdog_modbus_Tick)} -> {ex.Message}");
            }
        }

        private void Write_tcu_watchdog_modbus()
        {
            TCPModbusSlaveEntry? slave_entry = Globals.GetTheInstance().List_modbus_slave_entry.FirstOrDefault(entry => entry.Slave_type == SLAVE_TYPE.TCU);
            if (slave_entry != null)
            {
                try
                {
                    List<int> list_write_values = new();

                    string s_log = "SENDING ";
                    if (m_queue_commands.Count != 0)
                    {
                        s_log += "QUEUE ";
                        list_write_values = m_queue_commands.Dequeue();

                        string s_command = "SEND COMMAND TO TCU : ";
                        list_write_values.ForEach(value => s_command += value.ToString() + " ");
                        Manage_logs.SaveCommandValue(s_command);
                    }

                    else
                    {
                        s_log += "WATCHDOG ";

                        list_write_values.Add(Constants.CMD_SCS_METEO_STATION_VALUES);

                        Dispatcher.Invoke(() =>
                        {
                            m_keyValuePair_link_to_send_tcu_value.Select((item, index) => new { Item = item, Position = index }).ToList()
                               .ForEach(x =>
                               {
                                   if (!string.IsNullOrEmpty(x.Item.Value.Content.ToString()))
                                   {
                                       int send_to_tcu_value = 
                                       x.Item.Key == LINK_TO_SEND_TCU.SCADA_WD || x.Item.Key == LINK_TO_SEND_TCU.METEO_WD ? int.Parse(x.Item.Value.Content.ToString()) :
                                       x.Item.Key == LINK_TO_SEND_TCU.FIELD_SAFETY ? Convert.ToInt32(x.Item.Value.Content.ToString(), 16) :
                                       (int)(float.Parse(x.Item.Value.Content.ToString()) * 100); //Multiplicarlo por 100 para los decimales
                                       
                                       list_write_values.Add(send_to_tcu_value);
                                   }
                                   else
                                       list_write_values.Add(0);
                               });
                        });
                    }

                    if (Globals.GetTheInstance().Depur_enable == BIT_STATE.ON)
                    {
                        s_log += " COMMAND -> ";
                        list_write_values.ForEach(value => s_log += value.ToString() + " ");
                        Manage_logs.SaveLogValue(s_log);
                    }


                    Manage_thread manage_thread = Globals.GetTheInstance().List_manage_thread.First(manage_thread => manage_thread.TCP_modbus_slave_entry.Name == slave_entry.Name);
                    manage_thread.Write_multiple_registers(Globals.GetTheInstance().Modbus_dir_scs_command, list_write_values.ToArray());

                    m_scada_watchdog = m_scada_watchdog == Constants.MAX_SCADA_WD ? 1 : m_scada_watchdog + 1;
                    m_meteo_watchdog = m_meteo_watchdog == UInt16.MaxValue ? 1 : m_meteo_watchdog + 1;

                }
                catch (Exception ex)
                {
                    Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Write_tcu_watchdog_modbus)} -> {ex.Message}");
                }
            }
        }

        #endregion

        #region Timer write TCU datetime modbus

        private void Timer_write_tcu_datetime_modbus_Tick(object sender, EventArgs e)
        {
            try
            {
                TCPModbusSlaveEntry? slave_entry = Globals.GetTheInstance().List_modbus_slave_entry.FirstOrDefault(entry => entry.Slave_type == SLAVE_TYPE.TCU);
                if (slave_entry != null)
                {
                    //Mostrar valor de las variables de lectura
                    if (b_read_write_tcu_modbus && flag_write_tcu_datetime)
                    {
                        flag_write_tcu_datetime = false;

                        int year = DateTime.Now.Year - 2000;
                        int month = DateTime.Now.Month;
                        int day = DateTime.Now.Day;
                        int hour = DateTime.Now.Hour;
                        int minute = DateTime.Now.Minute;
                        int second = DateTime.Now.Second;
                        int millisecond = DateTime.Now.Millisecond;

                        List<int> list_values = new() { year, month, day, hour, minute, second, millisecond };

                        Manage_thread manage_thread = Globals.GetTheInstance().List_manage_thread.First(manage_thread => manage_thread.TCP_modbus_slave_entry.Name == slave_entry.Name);
                        manage_thread.Write_multiple_registers(Globals.GetTheInstance().Modbus_dir_tcu_datetime, list_values.ToArray());

                        flag_write_tcu_datetime = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Timer_write_tcu_datetime_modbus_Tick)} -> {ex.Message}");
            }
        }

        #endregion


        #region Timer write SAMCA

        private void Timer_write_samca_modbus_Tick(object sender, EventArgs e)
        {
            if (b_read_write_samca_modbus)
            {
            }
        }
        #endregion




        #region Timer reconnect slave

        private void Timer_reconnect_slave_Tick(object sender, EventArgs e)
        {
            m_timer_reconnect_slave.Stop();
            if (m_queue_reconnect_slave.Count != 0)
            {
                if (!b_read_scs_modbus && !b_read_write_tcu_modbus && !b_read_write_samca_modbus)
                {
                    m_queue_reconnect_slave.Clear();
                }
                else
                {
                    string slave = m_queue_reconnect_slave.Dequeue();
                    Manage_logs.SaveLogValue($"RESTART SLAVE COMMUNICATIONS -> {slave}");

                    Globals.GetTheInstance().List_manage_thread.Find(x => x.TCP_modbus_slave_entry.Name == slave).Start_tcp_com(true);

                    if (m_queue_reconnect_slave.Count != 0)
                        m_timer_reconnect_slave.Start();
                }
            }
        }

        #endregion


        #region Record

        #region Button record data

        private void Button_record_Click(object sender, RoutedEventArgs e)
        {
            Image_record_on.Visibility = b_record ? Visibility.Collapsed : Visibility.Visible;
            Image_record_off.Visibility = b_record ? Visibility.Visible : Visibility.Collapsed;
            b_record = !b_record;
        }

        #endregion

        #region Timer record data

        private void Timer_record_scs_normal_Tick(object sender, EventArgs e)
        {
            Record_scs(Constants.Record_scs1, false);
        }

        private void Timer_record_scs_fast_Tick(object sender, EventArgs e)
        {
            Record_scs(Constants.Record_scs2, true);
        }

        private void Record_scs(string s_file, bool fast_mode)
        {
            if (b_read_scs_modbus && b_record && b_first_read_scs_finish)
            {
                try
                {
                    string s_dir = AppDomain.CurrentDomain.BaseDirectory + Constants.Record_dir + @"\" + String.Format("{0:0000}", DateTime.Now.Year) + String.Format("{0:00}", DateTime.Now.Month);
                    if (!Directory.Exists(s_dir))
                        Directory.CreateDirectory(s_dir);


                    s_file = s_dir + @"\" + s_file + DateTime.Now.Year.ToString("0000") + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + ".csv";
                    if (!File.Exists(s_file))
                    {
                        using FileStream fs = File.Create(s_file);
                        fs.Close();

                        //Date head
                        string s_head = $"UTCDATE{Globals.GetTheInstance().SField_sep}";
                        s_head += $"DDATE{Globals.GetTheInstance().SField_sep}";

                        //Modbus var head
                        m_collection_modbus_var.ToList().ForEach(modbus_var =>
                        {
                            bool save = modbus_var.SCS_record && (!fast_mode || modbus_var.Fast_mode_record);
                            if (save)
                                s_head += $"{modbus_var.Name}[{modbus_var.Unit}]{Globals.GetTheInstance().SField_sep}";
                        });

                        //TCU codified status var head
                        if (!fast_mode)
                        {
                            Globals.GetTheInstance().List_tcu_codified_status
                                .Where(codified_status => codified_status.SCS_record).ToList()
                                .ForEach(codified_status => s_head += $"{codified_status.Name}[{codified_status.Unit}]{ Globals.GetTheInstance().SField_sep}");
                        }

                        s_head = s_head.Remove(s_head.Length - 1);

                        using StreamWriter stream_writer_head = new(s_file);
                        stream_writer_head.WriteLine(s_head);
                    }

                    double d_value = DateTime.UtcNow.ToOADate();

                    //Date values
                    string s_line = $"{DateTime.UtcNow.ToString(Globals.GetTheInstance().Date_format, new CultureInfo(Globals.GetTheInstance().Format_provider))}{Globals.GetTheInstance().SField_sep}";
                    s_line += $"{DateTime.UtcNow.ToOADate().ToString(Globals.GetTheInstance().nfi)}{Globals.GetTheInstance().SField_sep}";

                    //Modbus var values
                    m_collection_modbus_var.ToList().ForEach(modbus_var =>
                    {
                        bool save = modbus_var.SCS_record && (!fast_mode || modbus_var.Fast_mode_record);
                        if (save)
                        {
                            //Check if slave is connected
                            TCPModbusSlaveEntry slave_entry = Globals.GetTheInstance().List_modbus_slave_entry.First(slave => slave.Name.Equals(modbus_var.Slave));
                            s_line += slave_entry.Connected ? modbus_var.Value : Constants.Error_code;
                            s_line += Globals.GetTheInstance().SField_sep;
                        }
                    });

                    //TCU codified status var values
                    if (!fast_mode)
                    {
                        Globals.GetTheInstance().List_tcu_codified_status
                            .Where(codified_status => codified_status.SCS_record).ToList()
                            .ForEach(codified_status => s_line += $"{codified_status.Value}{ Globals.GetTheInstance().SField_sep}");
                    }

                    s_line = s_line.Remove(s_line.Length - 1);

                    using StreamWriter stream_writer_scs = new(s_file, true);
                    stream_writer_scs.WriteLine(s_line);
                }
                catch (Exception ex)
                {
                    Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Record_scs)} -> {ex.Message}");
                }
            }
        }


        private void Timer_record_tcu_Tick(object sender, EventArgs e)
        {
            Record_tcu(Constants.Record_tcu);
        }

        private void Record_tcu(string s_file)
        {
            if (b_read_write_tcu_modbus && b_record && b_first_read_tcu_finish)
            {
                try
                {
                    string s_dir = AppDomain.CurrentDomain.BaseDirectory + Constants.Record_dir + @"\" + String.Format("{0:0000}", DateTime.Now.Year) + String.Format("{0:00}", DateTime.Now.Month);
                    if (!Directory.Exists(s_dir))
                        Directory.CreateDirectory(s_dir);


                    s_file = s_dir + @"\" + s_file + DateTime.Now.Year.ToString("0000") + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + ".csv";
                    if (!File.Exists(s_file))
                    {
                        using FileStream fs = File.Create(s_file);
                        fs.Close();

                        string s_head = $"UTCDATE{Globals.GetTheInstance().SField_sep}";
                        s_head += $"DDATE{Globals.GetTheInstance().SField_sep}";

                        Globals.GetTheInstance().List_tcu_codified_status.ToList().ForEach(encode_state => s_head += $"{encode_state.Name}{ Globals.GetTheInstance().SField_sep}");

                        s_head = s_head.Remove(s_head.Length - 1);

                        using StreamWriter stream_writer_head = new(s_file);
                        stream_writer_head.WriteLine(s_head);
                    }

                    double d_value = DateTime.UtcNow.ToOADate();

                    //Fecha formato UTC
                    string s_line = $"{DateTime.UtcNow.ToString(Globals.GetTheInstance().Date_format, new CultureInfo(Globals.GetTheInstance().Format_provider))}{Globals.GetTheInstance().SField_sep}";
                    s_line += $"{DateTime.UtcNow.ToOADate().ToString(Globals.GetTheInstance().nfi)}{Globals.GetTheInstance().SField_sep}";

                    Globals.GetTheInstance().List_tcu_codified_status.ForEach(encode_state => s_line += $"{encode_state.Value}{ Globals.GetTheInstance().SField_sep}");

                    s_line = s_line.Remove(s_line.Length - 1);

                    using StreamWriter stream_writer_tcu = new(s_file, true);
                    stream_writer_tcu.WriteLine(s_line);
                }
                catch (Exception ex)
                {
                    Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Record_tcu)} -> {ex.Message}");
                }
            }
        }


        private void Timer_record_samca_Tick(object sender, EventArgs e)
        {
            Record_samca(Constants.Record_samca);
        }

        private void Record_samca(string s_file)
        {
            if (b_read_write_samca_modbus && b_record && b_first_read_samca_finish)
            {
            }
        }

        #endregion

        #endregion


        #region Mail

        #region Redefine send mail instant

        private void Send_mail_instant_start()
        {
            int interval_ms = Functions.Redefine_send_mail_instant();

            m_mail_state = MAIL_STATE.COMPRESS;
            m_timer_send_mail.Interval = interval_ms;
            m_timer_send_mail.Start();
        }

        #endregion

        #region Timer send mail

        private void Timer_send_mail_Tick(object sender, EventArgs e)
        {
            if (Globals.GetTheInstance().Mail_on == BIT_STATE.ON)
            {
                switch (m_mail_state)
                {
                    case MAIL_STATE.COMPRESS:
                        {
                            try
                            {
                                m_timer_send_mail.Stop();

                                string s_dir = AppDomain.CurrentDomain.BaseDirectory + Constants.Record_dir + @"\" + String.Format("{0:0000}", DateTime.Now.Year) + String.Format("{0:00}", DateTime.Now.Month);

                                string s_log = "COMPRESS FILES OK -> ";

                                List<string> list_record_file = new() { Constants.Record_scs1, Constants.Record_scs2 };
                                list_record_file.ForEach(record_file =>
                                {
                                    string s_file = s_dir + @"\" + record_file + DateTime.Now.Year.ToString("0000") + DateTime.Now.Month.ToString("00") + DateTime.Now.AddDays(-1).Day.ToString("00") + ".csv";

                                    string s_zip_file = Path.GetFileNameWithoutExtension(s_file) + ".zip";
                                    string path_zip_file = Constants.Compress_dir + @"\" + s_zip_file;

                                    if (File.Exists(path_zip_file))
                                        File.Delete(path_zip_file);

                                    using ZipArchive zip = ZipFile.Open(path_zip_file, ZipArchiveMode.Create);
                                    zip.CreateEntryFromFile(s_file, Path.GetFileName(s_file));

                                    s_log += Path.GetFileName(s_file) + ";";
                                });

                                m_mail_state = MAIL_STATE.SEND;
                                m_timer_send_mail.Interval = 2000;

                                Manage_logs.SaveLogValue(s_log);
                            }
                            catch (Exception ex)
                            {
                                Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Timer_send_mail_Tick)} -> {ex.Message}");
                                m_mail_state = MAIL_STATE.END;
                            }
                            m_timer_send_mail.Start();

                            break;
                        }

                    case MAIL_STATE.SEND:
                        {
                            m_timer_send_mail.Stop();

                            Tuple<bool, List<string>> tuple_send_mail = Functions.Send_record_mail();
                            List<string> list_report_files = tuple_send_mail.Item2;
                            if (tuple_send_mail.Item1)
                            {
                                string s_log = "SEND RECORD MAIL OK. Files -> ";
                                list_report_files.ForEach(file_report => s_log += file_report + ";");
                                Manage_logs.SaveLogValue(s_log);
                            }

                            m_timer_send_mail.Start();
                            m_mail_state = MAIL_STATE.END;

                            break;
                        }

                    case MAIL_STATE.END:
                        {
                            m_timer_send_mail.Stop();
                            Send_mail_instant_start();

                            break;
                        }
                }
            }
        }

        #endregion

        #endregion


        #region Setting

        #region Setting APP
        private void Button_setting_app_Click(object sender, RoutedEventArgs e)
        {
            if (b_read_scs_modbus)
                Checkbox_start.IsChecked = false;

            SettingAppWindow settingAppWindow = new()
            {
                Left = this.Left,
                Top = this.Top,
            };
            settingAppWindow.ShowDialog();

            #region Redefine timers

            m_timer_record_scs_normal.Stop();
            m_timer_record_scs_fast.Stop();
            m_timer_record_tcu.Stop();
            m_timer_record_samca.Stop();

            m_timer_reconnect_slave.Stop();
            m_timer_read_scs_normal_modbus.Stop();
            m_timer_read_scs_fast_modbus.Stop();
            m_timer_read_tcu_modbus.Stop();
            m_timer_write_tcu_watchdog_modbus.Stop();
            m_timer_write_tcu_datetime_modbus.Stop();
            m_timer_write_samca_modbus.Stop();

            m_timer_record_scs_normal.Interval = Globals.GetTheInstance().Record_scs_normal_interval;
            m_timer_record_scs_fast.Interval = Globals.GetTheInstance().Record_scs_fast_interval;
            m_timer_record_tcu.Interval = Globals.GetTheInstance().Record_tcu_interval;
            m_timer_record_samca.Interval = Globals.GetTheInstance().Record_samca_interval;

            m_timer_reconnect_slave.Interval = Globals.GetTheInstance().Modbus_reconnect_interval;
            m_timer_read_scs_normal_modbus.Interval = Globals.GetTheInstance().Modbus_read_scs_normal_interval;
            m_timer_read_scs_fast_modbus.Interval = Globals.GetTheInstance().Modbus_read_scs_fast_interval;
            m_timer_read_tcu_modbus.Interval = Globals.GetTheInstance().Modbus_read_tcu_interval;
            m_timer_write_tcu_watchdog_modbus.Interval = Globals.GetTheInstance().Modbus_write_tcu_watchdog_interval;
            m_timer_write_tcu_datetime_modbus.Interval = Globals.GetTheInstance().Modbus_write_tcu_datetime_interval;
            m_timer_write_samca_modbus.Interval = Globals.GetTheInstance().Modbus_write_samca_interval;

            m_timer_record_scs_normal.Start();
            m_timer_record_scs_fast.Start();
            m_timer_record_tcu.Start();
            m_timer_record_samca.Start();

            m_timer_read_scs_normal_modbus.Start();
            m_timer_read_scs_fast_modbus.Start();
            m_timer_read_tcu_modbus.Start();
            m_timer_write_tcu_watchdog_modbus.Start();
            m_timer_write_tcu_datetime_modbus.Start();
            m_timer_write_samca_modbus.Start();

            #endregion

            Globals.GetTheInstance().nfi.NumberDecimalSeparator = Globals.GetTheInstance().Decimal_sep == DECIMAL_SEP.PUNTO ? "." : ",";
            this.Height = Globals.GetTheInstance().Depur_enable == BIT_STATE.ON ? Constants.depur_enable_height : Constants.depur_disable_height;

            StartAvgValues();
        }

        #endregion

        #region Setting mail

        private void Button_setting_mail_Click(object sender, RoutedEventArgs e)
        {
            if (b_read_scs_modbus)
                Checkbox_start.IsChecked = false;

            SettingMailWindow settingMailWindow = new()
            {
                Left = this.Left,
                Top = this.Top,
            };
            settingMailWindow.ShowDialog();


            m_timer_send_mail.Stop();
            if (Globals.GetTheInstance().Mail_instant != "__:__")
                Send_mail_instant_start();
        }

        #endregion

        #region Setting modbus

        private void Button_setting_modbus_Click(object sender, RoutedEventArgs e)
        {
            if (b_read_scs_modbus)
                Checkbox_start.IsChecked = false;


            m_timer_reconnect_slave.Stop();
            m_timer_read_scs_normal_modbus.Stop();
            m_timer_read_scs_fast_modbus.Stop();
            m_timer_read_tcu_modbus.Stop();
            m_timer_write_tcu_watchdog_modbus.Stop();
            m_timer_write_tcu_datetime_modbus.Stop();

            m_timer_record_scs_normal.Stop();
            m_timer_record_scs_fast.Stop();
            m_timer_record_tcu.Stop();

            SettingModbusWindow setting_modbus_window = new()
            {
                Left = this.Left,
                Top = this.Top,
            };
            setting_modbus_window.ShowDialog();

            m_timer_read_scs_normal_modbus.Start();
            m_timer_read_scs_fast_modbus.Start();
            m_timer_read_tcu_modbus.Start();
            m_timer_write_tcu_watchdog_modbus.Start();
            m_timer_write_tcu_datetime_modbus.Start();
            m_timer_write_samca_modbus.Start();

            m_timer_record_scs_normal.Start();
            m_timer_record_scs_fast.Start();
            m_timer_record_tcu.Start();

            Load_modbus_slave_var();
        }

        #endregion

        #endregion



        #region Exit

        private void Button_exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion




    }
}
