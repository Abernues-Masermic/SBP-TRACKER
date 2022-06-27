using Ionic.Zip;
using NumericUpDownLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
        private System.Timers.Timer m_timer_wait_connect;

        private System.Timers.Timer m_timer_read_scs_normal_modbus;
        private System.Timers.Timer m_timer_read_scs_fast_modbus;
        private System.Timers.Timer m_timer_read_tcu_modbus;

        private System.Timers.Timer m_timer_write_tcu_command_modbus;
        private System.Timers.Timer m_timer_write_tcu_datetime_modbus;
        private System.Timers.Timer m_timer_write_samca_modbus;

        private System.Timers.Timer m_timer_record_scs_normal;
        private System.Timers.Timer m_timer_record_scs_fast;
        private System.Timers.Timer m_timer_record_tcu;
        private System.Timers.Timer m_timer_record_samca;

        private System.Timers.Timer m_timer_refresh_scada;
        private System.Timers.Timer m_timer_send_mail;
        private System.Timers.Timer m_timer_setting;

        #endregion

        private bool b_manual_start_stop;
        private bool b_individual_start_stop;

        private bool b_some_slave_ok = true;
        private bool b_wind_ok = true;
        private bool b_record = true;
        private bool b_flag_read_scs = false;
        private bool b_flag_read_tcu = false;

        private List<bool> m_list_read_write_state = new();


        private MAIL_STATE m_mail_state;
        private SETTING_OPTION m_setting_option;

        private TRACKER_STATE m_tracker_state;
        private int m_tracker_milliseconds;
        private LOCK_UNLOCK_STATE m_l1_state;
        private LOCK_UNLOCK_STATE m_l2_state;

        private List<Manage_thread> m_list_manage_thread_in_start_process = new();

        private ObservableCollection<TCPModbusSlaveEntry> m_collection_slave_entry;
        private ObservableCollection<TCPModbusVarEntry> m_collection_var_entry;
        private List<LinkToSendTCUClass> m_list_link_to_send_tcu = new();
        private TCPModbusSlaveEntry? m_selected_slave_entry = null;
        private TCUCommand? m_selected_command = null;


        private Queue<List<ushort>> m_queue_commands = new();
        private Queue<string> m_queue_reconnect_slave = new();

        public ushort[] m_array_write_samca_values;

        #region WIND AVG

        private List<List<Tuple<DateTime, double>>> m_list_wind_read_values = new();
        private double[] m_array_wind_avg_values = new double[Constants.MAX_WIND_AVG];
        private bool[] m_array_wind_max_break_in_range = new bool[Constants.MAX_WIND_AVG];
        private DateTime[] m_array_date_trigger_start_3sec = new DateTime[Constants.MAX_WIND_AVG / 2];
        private double[] m_array_low_histeresis_10min = new double[Constants.MAX_WIND_AVG / 2];

        private Dictionary<string, double> m_dictionary_wind_speed_slave_value; //Se utiliza el valor MAX entre var SCS + SAMCA

        #endregion

        #region INC AVG

        private List<List<Tuple<DateTime, double>>> m_list_inc_slope_read_values = new();
        private double[] m_array_inc_slope_avg_values = new double[Constants.MAX_INC_AVG];
        private bool[] m_array_inc_slope_emerg_stow_in_range = new bool[Constants.MAX_INC_AVG - 1];
        private bool[] m_array_inc_slope_alarm_in_range = new bool[Constants.MAX_INC_AVG - 1];

        #endregion

        #region DYN AVG

        private List<List<Tuple<DateTime, double>>> m_list_dyn_read_values = new();
        private double[] m_array_dyn_avg_values = new double[Constants.MAX_DYN_AVG];
        private bool[] m_array_dyn_excesive_force_emerg_stow_in_range = new bool[Constants.MAX_DYN_AVG];
        private bool[] m_array_dyn_excesive_force_alarm_in_range = new bool[Constants.MAX_DYN_AVG];
        private double[] m_list_dyn_values = new double[Constants.MAX_DYN_AVG];
        private double m_correction_load_pin_factor;

        #endregion


        #region Array controles

        private List<KeyValuePair<string, System.Windows.Shapes.Ellipse>> m_array_ellipse_slave = new();

        //TCU
        private List<Label> m_array_label_tcu_command_param = new();
        private List<DecimalUpDown> m_array_decimal_tcu_command_param = new();

        //GRAPHIC
        private List<Label> m_array_label_graphic_title = new();
        private List<Label> m_array_label_graphic_value = new();

        private List<Border> m_array_border_field_safety = new();

        private List<Border> m_array_border_wind_avg_value = new();
        private List<Label> m_array_label_wind_avg_value = new();
        private List<Label> m_array_label_wind_avg_max = new();

        private List<Label> m_array_label_inc_slope_avg_value = new();
        private List<Label> m_array_label_dyn_avg_value = new();

        private List<KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>> m_keyValuePair_codified_status_value = new();
        private List<LINK_TO_GRAPHIC_TCU> m_list_extract_retract = new()
        {
            LINK_TO_GRAPHIC_TCU.LOCK1_CURRENT_EXTRACT,
            LINK_TO_GRAPHIC_TCU.LOCK1_CURRENT_RETRACT,
            LINK_TO_GRAPHIC_TCU.LOCK2_CURRENT_EXTRACT,
            LINK_TO_GRAPHIC_TCU.LOCK2_CURRENT_RETRACT,
            LINK_TO_GRAPHIC_TCU.LOCK1_SEC_EXTRACT,
            LINK_TO_GRAPHIC_TCU.LOCK1_SEC_RETRACT,
            LINK_TO_GRAPHIC_TCU.LOCK2_SEC_EXTRACT,
            LINK_TO_GRAPHIC_TCU.LOCK2_SEC_RETRACT,
        };

        private List<Border> m_array_border_tcu_bit_state = new();
        private List<Image> m_array_image_forward_enable = new();
        private List<Image> m_array_image_backward_enable = new();
        private List<Image> m_array_image_forward_disable = new();
        private List<Image> m_array_image_backward_disable = new();

        #endregion




        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            Title = "SBP TRACKER / ver " + Constants.version;
            Label_ver.Content = "SBP TRACKER / ver " + Constants.version;

            Grid_table_mode.Visibility = Visibility.Visible;
            Dockpanel_graphic_mode.Visibility = Visibility.Collapsed;
            Grid_tcu_control_mode.Visibility = Visibility.Collapsed;

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

                Label_tracker_string_01_tension_title,
                Label_tracker_string_01_current_title,
                Label_tracker_string_02_tension_title,
                Label_tracker_string_02_current_title,
                Label_tracker_string_03_tension_title,
                Label_tracker_string_03_current_title,
                Label_fixed_string_04_tension_title,
                Label_fixed_string_04_current_title,
                Label_fixed_string_05_tension_title,
                Label_fixed_string_05_current_title,
                Label_fixed_string_06_tension_title,
                Label_fixed_string_06_current_title,
                Label_anemo_10m_title,
                Label_anemo_5m_title,
                Label_wind_dir_title,
                Label_temp_ambiente_title,
                Label_GHI_irr_title,
                Label_PPOA_irr_title,
                Label_humedad_title,
                Label_cell_01_temp_title,
                Label_cell_01_irr_title,
                Label_cell_02_temp_title,
                Label_cell_02_irr_title,
            };

            m_array_label_graphic_title.ForEach(label_graphic_title =>
            {
                label_graphic_title.Background = Brushes.DarkGray;
                label_graphic_title.MouseDoubleClick += new MouseButtonEventHandler(LabelGraphicTitle_MouseDoubleClick_EventHandler);
                label_graphic_title.MouseEnter += new MouseEventHandler(LabelGraphicTitle_MouseEnter_EventHandler);
                label_graphic_title.MouseLeave += new MouseEventHandler(LabelGraphicTitle_MouseLeave_EventHandler);
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

                Label_tracker_string_01_tension_value,
                Label_tracker_string_01_current_value,
                Label_tracker_string_02_tension_value,
                Label_tracker_string_02_current_value,
                Label_tracker_string_03_tension_value,
                Label_tracker_string_03_current_value,
                Label_fixed_string_04_tension_value,
                Label_fixed_string_04_current_value,
                Label_fixed_string_05_tension_value,
                Label_fixed_string_05_current_value,
                Label_fixed_string_06_tension_value,
                Label_fixed_string_06_current_value,
                Label_anemo_10m_value,
                Label_anemo_5m_value,
                Label_wind_dir_value,
                Label_temp_ambiente_value,
                Label_GHI_irr_value,
                Label_PPOA_irr_value,
                Label_humedad_value,
                Label_cell_01_temp_value,
                Label_cell_01_irr_value,
                Label_cell_02_temp_value,
                Label_cell_02_irr_value,
            };


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
            m_array_image_forward_enable = new List<Image> {
                Image_main_drive_forward_enable,
                Image_lock1_forward_enable,
                Image_lock2_forward_enable,
            };


            m_array_border_field_safety = new List<Border> {
                Border_autotrack_disable,
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
                Label_TCU_pos_el_value
            };
            m_array_label_dyn_avg_value = new List<Label>
            {
                Label_southlock_dyn1_value,
                Label_maindrive_dyn2_value,
                Label_maindrive_dyn3_value,
            };

            #region CODIFIED STATUS POS GRAPHIC

            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.TRACKER_STATE_TCU, Label_TCU_tracker_state_value));
            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.TRACKER_MODE_TCU, Label_TCU_tracker_mode_value));

            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.TRACKER_POS_EL, Label_TCU_pos_el_value));
            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.TRACKER_SET_POINT_EL, Label_TCU_pos_setpoint_value));
            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.TRACKER_ERROR_EL, Label_TCU_error_el_value));
            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.VECTOR_SOLAR_EL, Label_TCU_vector_solar_el_value));

            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.MAIN_POWER, Label_TCU_main_power_value));
            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.LOCK1_POWER, Label_TCU_lock1_power_value));
            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.LOCK2_POWER, Label_TCU_lock2_power_value));

            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.LOCK1_CURRENT_EXTRACT, Label_TCU_lock1_current_value));
            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.LOCK1_CURRENT_RETRACT, Label_TCU_lock1_current_value));
            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.LOCK2_CURRENT_EXTRACT, Label_TCU_lock2_current_value));
            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.LOCK2_CURRENT_RETRACT, Label_TCU_lock2_current_value));

            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.LOCK1_SEC_EXTRACT, Label_TCU_lock1_sec_value));
            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.LOCK1_SEC_RETRACT, Label_TCU_lock1_sec_value));
            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.LOCK2_SEC_EXTRACT, Label_TCU_lock2_sec_value));
            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.LOCK2_SEC_RETRACT, Label_TCU_lock2_sec_value));

            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.WD_SCS, Label_TCU_wd_scs_value));
            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.WD_LMS, Label_TCU_wd_lms_value));
            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.SAFETY_SUPERVISOR, Label_TCU_safety_supervisor_value));
            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.ALARM_WARNING_INDEX, Label_TCU_alarm_warning_index_value));

            m_keyValuePair_codified_status_value.Add(new KeyValuePair<LINK_TO_GRAPHIC_TCU, Label>(LINK_TO_GRAPHIC_TCU.DATE_RTC, Label_TCU_datetime_value));

            #endregion

            #endregion

            #region LINK TO SEND TCU CLASS

            m_list_link_to_send_tcu.Add(new LinkToSendTCUClass { Link_to_send_tcu = LINK_TO_SEND_TCU.TRACKER_WD, Label_tcu_mode_value = Label_scada_counter_to_tcu_value, Label_graphic_mode_value = Label_scada_counter_to_tcu_graphic, Value = 0, Unit = String.Empty });
            m_list_link_to_send_tcu.Add(new LinkToSendTCUClass { Link_to_send_tcu = LINK_TO_SEND_TCU.WIN_SPEED, Label_tcu_mode_value = Label_wind_speed_to_tcu_value, Label_graphic_mode_value = Label_wind_speed_to_tcu_graphic, Value = 0.0, Unit = String.Empty });
            m_list_link_to_send_tcu.Add(new LinkToSendTCUClass { Link_to_send_tcu = LINK_TO_SEND_TCU.WIN_DIR, Label_tcu_mode_value = Label_wind_dir_to_tcu_value, Label_graphic_mode_value = Label_wind_dir_to_tcu_graphic, Value = 0.0, Unit = String.Empty });
            m_list_link_to_send_tcu.Add(new LinkToSendTCUClass { Link_to_send_tcu = LINK_TO_SEND_TCU.AMBIENT_TEMP, Label_tcu_mode_value = Label_ambient_temp_to_tcu_value, Label_graphic_mode_value = Label_ambient_temp_to_tcu_graphic, Value = 0.0, Unit = String.Empty });
            m_list_link_to_send_tcu.Add(new LinkToSendTCUClass { Link_to_send_tcu = LINK_TO_SEND_TCU.AMBIENT_PRESSURE, Label_tcu_mode_value = Label_ambient_pressure_to_tcu_value, Label_graphic_mode_value = Label_ambient_pressure_to_tcu_graphic, Value = 0.0, Unit = String.Empty });
            m_list_link_to_send_tcu.Add(new LinkToSendTCUClass { Link_to_send_tcu = LINK_TO_SEND_TCU.DIRECT_IRRAD, Label_tcu_mode_value = Label_ambient_direct_irr_to_tcu_value, Label_graphic_mode_value = Label_ambient_direct_irr_to_tcu_graphic, Value = 0.0, Unit = String.Empty });
            m_list_link_to_send_tcu.Add(new LinkToSendTCUClass { Link_to_send_tcu = LINK_TO_SEND_TCU.DIFUSSE_IRRAD, Label_tcu_mode_value = Label_ambient_difusse_irr_to_tcu_value, Label_graphic_mode_value = Label_ambient_difusse_irr_to_tcu_graphic, Value = 0.0, Unit = String.Empty });
            m_list_link_to_send_tcu.Add(new LinkToSendTCUClass { Link_to_send_tcu = LINK_TO_SEND_TCU.METEO_WD, Label_tcu_mode_value = Label_meteo_counter_to_tcu_value, Label_graphic_mode_value = Label_meteo_counter_to_tcu_graphic, Value = 0, Unit = String.Empty });
            m_list_link_to_send_tcu.Add(new LinkToSendTCUClass { Link_to_send_tcu = LINK_TO_SEND_TCU.SAFETY_SUPERVISOR, Label_tcu_mode_value = Label_field_safety_supervisor_tcu_value, Label_graphic_mode_value = Label_field_safety_supervisor_tcu_graphic, Value = 0, Unit = String.Empty });

            #endregion

            Globals.GetTheInstance().Manage_delegate = new();
            Globals.GetTheInstance().Manage_delegate.TCP_handler_event += new Manage_delegate.TCP_handler(TCP_events_to_main);

            Globals.GetTheInstance().ManageWebAPI = new();
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

            if (!load_app_ok || !load_modbus_ok || !load_tcu_codified_status || !load_slope_correction)
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
            Button_setting_advanced.Visibility = Globals.GetTheInstance().Depur_enable == BIT_STATE.OFF ? Visibility.Collapsed : Visibility.Visible;

            for (int index = 0; index < Enum.GetNames(typeof(READ_WRITE_STATE)).Length; index++)
                m_list_read_write_state.Add(false);

            m_array_write_samca_values = new ushort[Constants.WR_SAMCA_REG_SIZE];


            #region Timer

            m_timer_start_tcp = new System.Timers.Timer();
            m_timer_start_tcp.Elapsed += Timer_start_tcp_Tick;
            m_timer_start_tcp.Interval = 1000;
            m_timer_start_tcp.Stop();

            m_timer_reconnect_slave = new System.Timers.Timer();
            m_timer_reconnect_slave.Elapsed += Timer_reconnect_slave_Tick;
            m_timer_reconnect_slave.Interval = Globals.GetTheInstance().Modbus_reconnect_interval;
            m_timer_reconnect_slave.Stop();

            m_timer_wait_connect = new System.Timers.Timer();
            m_timer_wait_connect.Elapsed += Timer_wait_connect_Tick;
            m_timer_wait_connect.Interval = 6000;
            m_timer_wait_connect.Stop();

            #region Read

            m_timer_read_scs_normal_modbus = new System.Timers.Timer();
            m_timer_read_scs_normal_modbus.Elapsed += Timer_read_scs_normal_modbus_Tick;
            m_timer_read_scs_normal_modbus.Interval = Globals.GetTheInstance().Modbus_read_scs_normal_interval;
            m_timer_read_scs_normal_modbus.Start();

            m_timer_read_scs_fast_modbus = new System.Timers.Timer();
            m_timer_read_scs_fast_modbus.Elapsed += Timer_read_scs_fast_modbus_Tick;
            m_timer_read_scs_fast_modbus.Interval = Globals.GetTheInstance().Modbus_read_scs_fast_interval;
            m_timer_read_scs_fast_modbus.Start();

            m_timer_read_tcu_modbus = new System.Timers.Timer();
            m_timer_read_tcu_modbus.Elapsed += Timer_read_tcu_modbus_Tick;
            m_timer_read_tcu_modbus.Interval = Globals.GetTheInstance().Modbus_read_tcu_interval;
            m_timer_read_tcu_modbus.Start();

            #endregion

            #region Write

            m_timer_write_tcu_command_modbus = new System.Timers.Timer();
            m_timer_write_tcu_command_modbus.Elapsed += Timer_write_tcu_command_modbus_Tick;
            m_timer_write_tcu_command_modbus.Interval = Globals.GetTheInstance().Modbus_write_tcu_watchdog_interval;
            m_timer_write_tcu_command_modbus.Start();

            m_timer_write_tcu_datetime_modbus = new System.Timers.Timer();
            m_timer_write_tcu_datetime_modbus.Elapsed += Timer_write_tcu_datetime_modbus_Tick;
            m_timer_write_tcu_datetime_modbus.Interval = Globals.GetTheInstance().Modbus_write_tcu_datetime_interval;
            m_timer_write_tcu_datetime_modbus.Start();

            m_timer_write_samca_modbus = new System.Timers.Timer();
            m_timer_write_samca_modbus.Elapsed += Timer_write_samca_modbus_Tick;
            m_timer_write_samca_modbus.Interval = Globals.GetTheInstance().Modbus_write_samca_interval;
            m_timer_write_samca_modbus.Start();

            #endregion

            #region Record

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

            #endregion


            m_timer_setting = new System.Timers.Timer();
            m_timer_setting.Elapsed += Timer_setting_Tick;
            m_timer_setting.Interval = 1000;
            m_timer_setting.Stop();

            m_timer_refresh_scada = new System.Timers.Timer();
            m_timer_refresh_scada.Elapsed += Timer_refresh_scada_Tick;
            m_timer_refresh_scada.Interval = Globals.GetTheInstance().Refresh_scada_interval;
            m_timer_refresh_scada.Start();

            m_timer_send_mail = new System.Timers.Timer();
            m_timer_send_mail.Elapsed += Timer_send_mail_Tick;
            m_timer_send_mail.Interval = 10000000;
            m_timer_send_mail.Stop();

            #endregion


            if (Globals.GetTheInstance().Mail_instant != "__:__")
                Send_mail_instant_start();

            if (Globals.GetTheInstance().Enable_web_api == BIT_STATE.ON)
            {
                Globals.GetTheInstance().ManageWebAPI.m_API_data_started = true;
                Globals.GetTheInstance().ManageWebAPI.Start_timer_modbus_API_state(BIT_STATE.ON, (int)Globals.GetTheInstance().Send_state_interval_web_API);
                Globals.GetTheInstance().ManageWebAPI.Start_timer_modbus_API_data(BIT_STATE.ON, (int)Globals.GetTheInstance().Send_data_interval_web_API);
            }


            Functions.Cloud_upload_start(Globals.GetTheInstance().Cloud_check_interval, 1);
        }

        #endregion


        #region START AVG VALUES

        private void StartAvgValues()
        {
            #region WIND

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

            m_dictionary_wind_speed_slave_value = new Dictionary<string, double>();

            #endregion

            #region INCLINOMETER

            m_array_inc_slope_avg_values = Enumerable.Repeat(0.0, Constants.MAX_INC_AVG).ToArray();
            m_array_inc_slope_emerg_stow_in_range = Enumerable.Repeat(true, Constants.MAX_INC_AVG - 1).ToArray();
            m_array_inc_slope_alarm_in_range = Enumerable.Repeat(true, Constants.MAX_INC_AVG - 1).ToArray();

            m_list_inc_slope_read_values = new();
            for (int index = 0; index < Constants.MAX_INC_AVG; index++)
                m_list_inc_slope_read_values.Add(new());

            #endregion

            #region DYNANOMETER

            m_list_dyn_read_values = new();
            m_array_dyn_excesive_force_emerg_stow_in_range = Enumerable.Repeat(true, Constants.MAX_DYN_AVG).ToArray();
            m_array_dyn_excesive_force_alarm_in_range = Enumerable.Repeat(true, Constants.MAX_DYN_AVG).ToArray();
            for (int index = 0; index < Constants.MAX_DYN_AVG; index++)
                m_list_dyn_read_values.Add(new());

            #endregion
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
                    Button_setting_advanced.Visibility = Globals.GetTheInstance().Depur_enable == BIT_STATE.OFF ? Visibility.Collapsed : Visibility.Visible;
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
                        TCPModbusSlaveEntry? slave_entry = Globals.GetTheInstance().List_slave_entry.FirstOrDefault(slave_entry => slave_entry.Name == args.Slave_name);
                        if (slave_entry != null)
                        {
                            string s_log = slave_entry.Reconnect ? "RECONNECTED" : "CONNECTED";
                            Manage_logs.SaveCommunicationValue($"MODBUS SLAVE {s_log} -> { args.Slave_name}");

                            slave_entry.Connected = true;
                            slave_entry.Reconnect = false;

                            Dispatcher.Invoke(() => m_array_ellipse_slave.Find(key => key.Key == args.Slave_name).Value.Fill = Brushes.Green);

                            if (slave_entry.Slave_type == SLAVE_TYPE.TCU)
                            {
                                m_list_read_write_state[(int)READ_WRITE_STATE.read_write_tcu_modbus] = true;
                                m_tracker_state = TRACKER_STATE.NONE;
                                m_l1_state = LOCK_UNLOCK_STATE.NONE;
                                m_l2_state = LOCK_UNLOCK_STATE.NONE;
                            }

                            else if (slave_entry.Slave_type == SLAVE_TYPE.SAMCA)
                                m_list_read_write_state[(int)READ_WRITE_STATE.read_write_samca_modbus] = true;

                            else
                                m_list_read_write_state[(int)READ_WRITE_STATE.read_scs_modbus] = true;

                            Check_ini_read_slaves(args.Slave_name);
                        }

                        break;
                    }

                case TCP_ACTION.DISCONNECT:
                    {
                        TCPModbusSlaveEntry? slave_entry = Globals.GetTheInstance().List_slave_entry.FirstOrDefault(slave_entry => slave_entry.Name == args.Slave_name);
                        if (slave_entry != null)
                        {
                            try
                            {
                                if (!slave_entry.Reconnect)
                                    Manage_logs.SaveCommunicationValue("MODBUS SLAVE DISCONNECTED -> " + args.Slave_name);

                                slave_entry.Connected = false;

                                if (slave_entry.Slave_type == SLAVE_TYPE.TCU)
                                {
                                    m_list_read_write_state[(int)READ_WRITE_STATE.first_read_tcu_finish] = false;
                                    m_list_read_write_state[(int)READ_WRITE_STATE.read_write_tcu_modbus] = false;

                                    Globals.GetTheInstance().List_tcu_codified_status.ForEach(modbus_var => modbus_var.Value = string.Empty);

                                    Dispatcher.Invoke(() =>
                                    {
                                        Listview_tcu_codified_status.Items.Refresh();

                                        //TCU
                                        m_array_border_field_safety.ForEach(border_field_safety => border_field_safety.Background = Brushes.White);
                                        m_array_border_tcu_bit_state.ForEach(border => border.Background = new SolidColorBrush(Color.FromArgb(0x00, 0x09, 0x09, 0xB5)));
                                        m_array_image_backward_enable.ForEach(image => image.Visibility = Visibility.Collapsed);
                                        m_array_image_forward_enable.ForEach(image => image.Visibility = Visibility.Collapsed);
                                        m_array_image_backward_disable.ForEach(image => image.Visibility = Visibility.Visible);
                                        m_array_image_forward_disable.ForEach(image => image.Visibility = Visibility.Visible);
                                        m_keyValuePair_codified_status_value.ForEach(key_value => key_value.Value.Content = string.Empty);
                                    });
                                }


                                else
                                {
                                    if (slave_entry.Slave_type == SLAVE_TYPE.SAMCA)
                                    {
                                        m_list_read_write_state[(int)READ_WRITE_STATE.first_read_samca_finish] = false;
                                        m_list_read_write_state[(int)READ_WRITE_STATE.read_write_samca_modbus] = false;
                                    }

                                    //Modbus var list
                                    m_collection_var_entry.ToList()
                                        .Where(var_entry => var_entry.Slave == slave_entry.Name).ToList()
                                        .ForEach(var_entry => var_entry.Value = string.Empty);

                                    //Schema
                                    slave_entry.List_var_entry.ForEach(var_entry =>
                                    {
                                        Dispatcher.Invoke(() =>
                                        {
                                            if (var_entry.Graphic_pos != Constants.index_no_selected)
                                                m_array_label_graphic_value[var_entry.Graphic_pos].Content = string.Empty;
                                        });

                                        if ((LINK_TO_SEND_TCU)var_entry.Link_to_send_tcu != LINK_TO_SEND_TCU.NONE)
                                        {
                                            LinkToSendTCUClass? link_to_send_tcu_class = m_list_link_to_send_tcu.FirstOrDefault(link => link.Link_to_send_tcu == (LINK_TO_SEND_TCU)var_entry.Link_to_send_tcu);
                                            if (link_to_send_tcu_class != null)
                                            {
                                                Dispatcher.Invoke(() =>
                                                {
                                                    link_to_send_tcu_class.Label_tcu_mode_value.Content = string.Empty;
                                                    link_to_send_tcu_class.Label_graphic_mode_value.Content = string.Empty;
                                                });
                                            }
                                        }

                                        if ((LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.WIND_AVG_SBPT)
                                        {
                                            Dispatcher.Invoke(() =>
                                            {
                                                m_array_label_wind_avg_value[(int)WIND_AVG_POSITION.SBPT_3SEC].Content = string.Empty;
                                                m_array_label_wind_avg_value[(int)WIND_AVG_POSITION.SBPT_10MIN].Content = string.Empty;
                                            });

                                        }

                                        if ((LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.WIND_AVG_SAMCA)
                                        {
                                            Dispatcher.Invoke(() =>
                                            {
                                                m_array_label_wind_avg_value[(int)WIND_AVG_POSITION.SAMCA_3SEC].Content = string.Empty;
                                                m_array_label_wind_avg_value[(int)WIND_AVG_POSITION.SAMCA_10MIN].Content = string.Empty;
                                            });
                                        }
                                    });
                                }


                                if (Globals.GetTheInstance().List_slave_entry.All(slave_entry => !slave_entry.Connected) && b_manual_start_stop)
                                {
                                    Manage_logs.SaveLogValue("SE HA DETENIDO PROCESO DE LECTURA ALL SLAVES");

                                    m_list_read_write_state[(int)READ_WRITE_STATE.first_read_scs_finish] = false;
                                    m_list_read_write_state[(int)READ_WRITE_STATE.read_scs_modbus] = false;

                                    Dispatcher.Invoke(() => Checkbox_start.IsChecked = false);
                                    Dispatcher.Invoke(() => ((Storyboard)Resources["BlinkStoryboard"]).Remove());

                                    m_collection_var_entry.ToList().ForEach(modbus_var => modbus_var.Value = string.Empty);
                                    Dispatcher.Invoke(() => Listview_read_var_entry.Items.Refresh());
                                }

                                Dispatcher.Invoke(() => m_array_ellipse_slave.Find(key_value => key_value.Key == args.Slave_name).Value.Fill = slave_entry.Enable_communication ? Brushes.Red : Brushes.Blue);

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
                        TCPModbusSlaveEntry? slave_entry = Globals.GetTheInstance().List_slave_entry.FirstOrDefault(slave_entry => slave_entry.Name == args.Slave_name);
                        if (slave_entry != null)
                        {
                            Dispatcher.Invoke(() => m_array_ellipse_slave.Find(key => key.Key == args.Slave_name).Value.Fill = Brushes.Green);
                        }

                        break;
                    }

                case TCP_ACTION.ERROR_CONNECT:
                    {
                        Manage_logs.SaveCommunicationValue($"ERROR CONNECT MODBUS SLAVE -> {args.Slave_name}");

                        TCPModbusSlaveEntry? slave_entry = Globals.GetTheInstance().List_slave_entry.FirstOrDefault(slave_entry => slave_entry.Name == args.Slave_name);
                        if (slave_entry != null)
                            Check_ini_read_slaves(slave_entry.Name);

                        break;
                    }

                case TCP_ACTION.ERROR_READ:
                    {
                        Manage_logs.SaveCommunicationValue($"ERROR READ MODBUS SLAVE -> { args.Slave_name}");

                        if (Globals.GetTheInstance().List_slave_entry.First(slave_entry => slave_entry.Name == args.Slave_name).Connected)
                        {
                            Dispatcher.Invoke(() => m_array_ellipse_slave.Find(x => x.Key == args.Slave_name).Value.Fill = Brushes.Yellow);
                        }

                        break;
                    }

                case TCP_ACTION.ERROR_WRITE:
                    {
                        Manage_logs.SaveCommunicationValue($"ERROR WRITE MODBUS SLAVE -> { args.Slave_name}");

                        if (Globals.GetTheInstance().List_slave_entry.First(slave_entry => slave_entry.Name == args.Slave_name).Connected)
                        {
                            Dispatcher.Invoke(() => m_array_ellipse_slave.Find(x => x.Key == args.Slave_name).Value.Fill = Brushes.Yellow);
                        }

                        break;
                    }

                case TCP_ACTION.RECONNECT:
                    {
                        TCPModbusSlaveEntry? slave_entry = Globals.GetTheInstance().List_slave_entry.FirstOrDefault(slave_entry => slave_entry.Name == args.Slave_name);
                        if (slave_entry != null)
                        {
                            string s_log = string.Empty;
                            if (slave_entry.Reconnect == false)
                            {
                                slave_entry.Num_communication_error++;
                                s_log += "FIRST RECONNECT";
                                slave_entry.Reconnect = true;

                                if (slave_entry.Slave_type == SLAVE_TYPE.TCU)
                                    Functions.Send_alarm_mail("SBP TRACKER ALARM", "COMMUNICATION WITH SLAVE HAS BEEN LOST -> " + args.Slave_name);
                            }
                            else
                                s_log += "ERROR RECONNECT";

                            Dispatcher.Invoke(() => m_array_ellipse_slave.Find(x => x.Key == args.Slave_name).Value.Fill = slave_entry.Enable_communication ? Brushes.Red : Brushes.Blue);

                            Globals.GetTheInstance().List_manage_thread.Find(x => x.TCP_modbus_slave_entry.Name == args.Slave_name).Stop_tcp_com_thread();

                            if (m_queue_reconnect_slave.Count == 0)
                                m_timer_reconnect_slave.Start();

                            Manage_logs.SaveCommunicationValue($"{s_log} / ENQUEUE MODBUS SLAVE -> { args.Slave_name}");
                            m_queue_reconnect_slave.Enqueue(args.Slave_name);

                        }

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
            Label? label = sender as Label;
            SelectVarMapWindow selected_var_map_window = new();
            selected_var_map_window.Selected_index = m_array_label_graphic_title.IndexOf(label);

            //Save assigned var
            TCPModbusVarEntry current_var_entry = null;
            Globals.GetTheInstance().List_slave_entry.ForEach(slave_entry =>
            {
                slave_entry.List_var_entry.ForEach(var_entry =>
                {
                    if (var_entry.Graphic_pos == selected_var_map_window.Selected_index)
                    {
                        current_var_entry = var_entry;
                        selected_var_map_window.Selected_var = current_var_entry.Name;
                    }
                });
            });


            if (selected_var_map_window.ShowDialog() == true)
            {
                if (string.IsNullOrEmpty(selected_var_map_window.Selected_var))
                {
                    label.Background = Brushes.DarkGray;

                    if (current_var_entry != null)
                        current_var_entry.Graphic_pos = Constants.index_no_selected;
                }

                else
                {
                    Globals.GetTheInstance().List_slave_entry.ForEach(slave_entry =>
                    {
                        slave_entry.List_var_entry.ForEach(var_entry =>
                        {
                            if (var_entry.Name == selected_var_map_window.Selected_var)
                                var_entry.Graphic_pos = selected_var_map_window.Selected_index;
                        });
                    });

                    label.Background = Brushes.Black;
                }
            }
        }

        #endregion

        #region Label graphic enter - leave

        private void LabelGraphicTitle_MouseEnter_EventHandler(object sender, MouseEventArgs e)
        {
            Label? current_label = sender as Label;
            int index = m_array_label_graphic_title.IndexOf(current_label);
            if (index != Constants.index_no_selected)
            {
                List<TCPModbusVarEntry> list_all_var_entry = new();
                Globals.GetTheInstance().List_slave_entry.ForEach(slave_entry =>
                {
                    list_all_var_entry.AddRange(slave_entry.List_var_entry);
                });

                TCPModbusVarEntry? var_entry = list_all_var_entry.FirstOrDefault(var_entry => var_entry.Graphic_pos == index);
                if (var_entry != null)
                {
                    TextblockPopUpVarEntry.Text = var_entry.Name;
                    PopUpVarEntry.IsOpen = true;
                }
            }
        }

        private void LabelGraphicTitle_MouseLeave_EventHandler(object sender, MouseEventArgs e)
        {
            PopUpVarEntry.IsOpen = false;
        }

        #endregion


        #region TCU graphic buttons

        #region Field safety analisis

        private void Button_field_safety_analisis_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<FieldSafetyAnalisisWindow>().FirstOrDefault() == null)
            {
                FieldSafetyAnalisisWindow field_window = new();
                field_window.Owner = this;
                field_window.Show();
            }
        }

        #endregion

        #region Commands

        private void Button_stow_Click(object sender, RoutedEventArgs e)
        {
            List<ushort> list_command_fields = new();
            list_command_fields.Add((int)GRAPHIC_SCADA_COMMANDS.STOW);
            Manage_logs.SaveLogValue($"ENQUEUE GRAPHIC COMMAND (STOW)");
            m_queue_commands.Enqueue(list_command_fields);
        }

        private void Button_offline_Click(object sender, RoutedEventArgs e)
        {
            List<ushort> list_command_fields = new();
            list_command_fields.Add((int)GRAPHIC_SCADA_COMMANDS.OFFLINE);
            Manage_logs.SaveLogValue($"ENQUEUE GRAPHIC COMMAND (OFFLINE)");
            m_queue_commands.Enqueue(list_command_fields);
        }

        private void Button_online_Click(object sender, RoutedEventArgs e)
        {
            List<ushort> list_command_fields = new();
            list_command_fields.Add((int)GRAPHIC_SCADA_COMMANDS.ONLINE);
            Manage_logs.SaveLogValue($"ENQUEUE GRAPHIC COMMNAD (ONLINE)");
            m_queue_commands.Enqueue(list_command_fields);
        }

        private void Button_tracking_Click(object sender, RoutedEventArgs e)
        {
            List<ushort> list_command_fields = new();
            list_command_fields.Add((int)GRAPHIC_SCADA_COMMANDS.TRACKING);
            Manage_logs.SaveLogValue($"ENQUEUE GRAPHIC COMMNAD (TRACKING)");
            m_queue_commands.Enqueue(list_command_fields);
        }

        #endregion

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
                    TCPModbusSlaveEntry? entry = Listview_slave_modbus.SelectedItem as TCPModbusSlaveEntry;
                    Select_slave_var(entry);
                }
            }
        }

        #endregion

        #region Check all slaves

        private void Checkbox_all_slave_Checked(object sender, RoutedEventArgs e)
        {
            if (Globals.GetTheInstance().List_slave_entry != null)
                Select_slave_var(null);
        }


        private void Select_slave_var(TCPModbusSlaveEntry selected_slave_entry)
        {
            m_selected_slave_entry = selected_slave_entry;

            m_collection_var_entry.Clear();

            List<TCPModbusSlaveEntry> list_slave_entry = Globals.GetTheInstance().List_slave_entry;
            if (m_selected_slave_entry != null)
            {
                Manage_logs.SaveLogValue("FILTER VAR FOR SELECTED SLAVE -> " + m_selected_slave_entry.Name);

                list_slave_entry = list_slave_entry.Where(modbus_slave_entry => modbus_slave_entry.Name.Equals(m_selected_slave_entry.Name)).ToList();
                Checkbox_all_slave.IsChecked = false;
            }

            list_slave_entry
                .ForEach(slave_entry => slave_entry.List_var_entry
                .ForEach(var_entry => m_collection_var_entry.Add(var_entry)));

            m_collection_var_entry = new ObservableCollection<TCPModbusVarEntry>(m_collection_var_entry
                .OrderBy(modbus_var => modbus_var.Slave)
                .ThenBy(modbus_var => modbus_var.DirModbus));

            Listview_read_var_entry.ItemsSource = m_collection_var_entry;
            Listview_read_var_entry.Items.Refresh();
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
                TCUCodifiedStatusEntry? selected_tcu_encode = item as TCUCodifiedStatusEntry;
                TCPModbusSlaveEntry? modbus_slave_entry = m_collection_slave_entry.FirstOrDefault(modbus_slave => modbus_slave.Slave_type == SLAVE_TYPE.TCU);

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
                Factor = 1,
                SCS_record = false,
                Status_mask_enable = false,
                List_status_mask = new List<string>(),
                DirModbus = Constants.index_no_selected,
                Link_to_graphic = (int)LINK_TO_GRAPHIC_TCU.NONE,
                Send_to_samca_pos = Constants.index_no_selected.ToString()
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
                TCUCodifiedStatusEntry codified_status_entry = item as TCUCodifiedStatusEntry;

                Globals.GetTheInstance().List_tcu_codified_status.Remove(codified_status_entry);
                bool save_ok = Manage_file.Save_tcu_decodified_entries();
                if (!save_ok)
                    MessageBox.Show("Error deleting selected encode entry", "INFO", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                
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

            BitMaskWindow bit_mask = new()
            {
                Owner = this
            };

            BlurEffect blurEffect = new()
            {
                Radius = 3
            };

            Effect = blurEffect;

            bit_mask.TCU_codified_status_entry = codified_entry;
            bit_mask.ShowDialog();
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

                Label_selected_command.Content = $"{m_selected_command.Index} - {m_selected_command.Name}";

                int index = 0;

                if (m_selected_command.Num_params != 0)
                {
                    do
                    {
                        WrapPanel wrap_param = new()
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(0, 1, 0, 1),
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
                List<ushort> list_command_fields = new();
                list_command_fields.Add((ushort)m_selected_command.Index);

                list_command_fields.Add(0); //Field watchdog

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
                                    list_command_fields.Add((ushort)m_array_decimal_tcu_command_param[index].Value);
                                    break;
                                }

                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                                {
                                    int param_value = (int)m_array_decimal_tcu_command_param[index].Value;
                                    byte[] byte_send_parameter = BitConverter.GetBytes(param_value);
                                    ushort first_send_parameter = BitConverter.ToUInt16(byte_send_parameter, 0);
                                    ushort second_send_parameter = BitConverter.ToUInt16(byte_send_parameter, 2);
                                    list_command_fields.Add((ushort)first_send_parameter);
                                    list_command_fields.Add((ushort)second_send_parameter);

                                    break;
                                }
                        }
                    }
                    while (++index < m_selected_command.Num_params);
                }

                string s_data = string.Empty;
                list_command_fields.ForEach(value => s_data += $"{value} ");
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

            bool load_slave_ok = Manage_file.Load_modbus_slave_entries();
            string s_log = load_slave_ok ? "Read modbus slave entries OK" : "Error reading modbus slave entries";
            Manage_logs.SaveLogValue(s_log);

            bool read_var_map_ok = Manage_file.Load_var_map_entries();
            s_log = load_slave_ok ? "Read var map entries OK" : "Error reading var map entries";
            Manage_logs.SaveLogValue(s_log);

            load_modbus_ok = load_slave_ok && read_var_map_ok;

            try
            {
                if (load_modbus_ok)
                {
                    #region Slave - Var map

                    string s_slave_var_log = $"SLAVE - VAR CONFIG \r\n-----------------------\r\n";

                    m_collection_slave_entry = new ObservableCollection<TCPModbusSlaveEntry>();
                    m_collection_var_entry = new ObservableCollection<TCPModbusVarEntry>();

                    m_list_link_to_send_tcu.ForEach(link =>
                    {
                        link.Label_tcu_mode_value.Content = string.Empty;
                        link.Label_graphic_mode_value.Content = string.Empty;
                    });

                    Globals.GetTheInstance().List_slave_entry.ForEach(slave_entry =>
                    {
                        m_collection_slave_entry.Add(slave_entry);

                        s_slave_var_log += $"{slave_entry.Name} \r\n-----------------------\r\n";

                        slave_entry.List_var_entry.ForEach(var_entry =>
                        {
                            m_collection_var_entry.Add(var_entry);

                            s_slave_var_log += var_entry.Name + "\r\n";

                            if (var_entry.Graphic_pos != Constants.index_no_selected)
                                m_array_label_graphic_title[var_entry.Graphic_pos].Background = Brushes.Black;


                            if ((LINK_TO_SEND_TCU)var_entry.Link_to_send_tcu != LINK_TO_SEND_TCU.NONE)
                            {
                                LinkToSendTCUClass? link_to_send_tcu_class = m_list_link_to_send_tcu.FirstOrDefault(link => link.Link_to_send_tcu == (LINK_TO_SEND_TCU)var_entry.Link_to_send_tcu);
                                if (link_to_send_tcu_class != null)
                                    link_to_send_tcu_class.Unit = var_entry.Unit;
                            }

                        });

                        s_slave_var_log += $"------------------------------\r\n";
                    });

                    Manage_logs.SaveLogValue(s_slave_var_log);


                    #region Refresh lists

                    m_collection_slave_entry = new ObservableCollection<TCPModbusSlaveEntry>(m_collection_slave_entry.OrderBy(modbus_slave => modbus_slave.Name));
                    Listview_slave_modbus.ItemsSource = m_collection_slave_entry;
                    Listview_slave_modbus.Items.Refresh();

                    m_collection_var_entry = new ObservableCollection<TCPModbusVarEntry>(m_collection_var_entry
                        .OrderBy(modbus_var => modbus_var.Slave)
                        .ThenBy(modbus_var => modbus_var.DirModbus));

                    Listview_read_var_entry.ItemsSource = m_collection_var_entry;
                    Listview_read_var_entry.Items.Refresh();

                    #endregion

                    #endregion

                    #region Slave communication control

                    Wrap_slave_state.Children.Clear();
                    m_array_ellipse_slave.Clear();
                    m_collection_slave_entry.ToList().ForEach(slave_entry =>
                    {
                        System.Windows.Shapes.Ellipse ellipse_slave = new();
                        ellipse_slave.Margin = new Thickness(5, 0, 5, 0);
                        ellipse_slave.Width = 15;
                        ellipse_slave.Height = 15;
                        ellipse_slave.Stroke = Brushes.Black;
                        ellipse_slave.StrokeThickness = 1;
                        ellipse_slave.VerticalAlignment = VerticalAlignment.Center;
                        ellipse_slave.Fill = slave_entry.Enable_communication ? Brushes.Red : Brushes.Blue;
                        ellipse_slave.MouseEnter += new MouseEventHandler(EllipseOnMouseEnter);
                        ellipse_slave.MouseLeave += new MouseEventHandler(EllipseOnMouseLeave);

                        Wrap_slave_state.Children.Add(ellipse_slave);
                        m_array_ellipse_slave.Add(new KeyValuePair<string, System.Windows.Shapes.Ellipse>(slave_entry.Name, ellipse_slave));
                    });

                    #endregion

                    #region Start manage thread

                    Globals.GetTheInstance().List_manage_thread = new List<Manage_thread>();
                    Globals.GetTheInstance().List_slave_entry.ForEach(slave_entry =>
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
            string error_comm = Globals.GetTheInstance().List_slave_entry.First(x => x.Name.Equals(slave_name)).Num_communication_error.ToString();
            TextblockPopUpSlave.Text = $"{slave_name} / MB ERR COUNT: {error_comm}";
            PopUpSlaves.IsOpen = true;
        }

        private void EllipseOnMouseLeave(object sender, MouseEventArgs e)
        {
            PopUpSlaves.IsOpen = false;
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
                    Globals.GetTheInstance().List_manage_thread.ForEach(manage_thread =>
                    {
                        if (manage_thread.TCP_modbus_slave_entry.Enable_communication)
                            m_list_manage_thread_in_start_process.Add(manage_thread);
                    });

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
                if (!manageThread.ManageModbus.Is_connected())
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
            m_timer_start_tcp.Stop();
            m_timer_wait_connect.Start();

            for (int i = m_list_manage_thread_in_start_process.Count - 1; i >= 0; i--)
                m_list_manage_thread_in_start_process[i].Start_tcp_com(false);
        }

        #endregion

        #region Timer wait connect

        private void Timer_wait_connect_Tick(object sender, EventArgs e)
        {
            m_timer_wait_connect.Stop();

            m_list_manage_thread_in_start_process.Clear();

            b_some_slave_ok = Globals.GetTheInstance().List_slave_entry.Exists(slave_entry => slave_entry.Connected);

            string s_log = b_some_slave_ok ? "INICIO PROCESO DE LECTURA SLAVES" : "ERROR EN EL INICIO PROCESO DE LECTURA SLAVES. NINGUN SLAVE CONECTADO";
            Manage_logs.SaveLogValue($"TIMEOUT WAIT CONNECT  - {s_log} ");

            if (b_some_slave_ok)
            {
                Dispatcher.Invoke(() => StartAvgValues());
                b_manual_start_stop = false;
                Dispatcher.Invoke(() => ((Storyboard)Resources["BlinkStoryboard"]).Begin());
            }

            Dispatcher.Invoke(() => Checkbox_start.IsChecked = b_some_slave_ok);
            Dispatcher.Invoke(() => Border_wait.Visibility = Visibility.Collapsed);
        }

        #endregion

        #region Check ini read slaves

        private void Check_ini_read_slaves(string slave_name)
        {
            try
            {
                m_list_manage_thread_in_start_process.RemoveAll(Manage_thread => Manage_thread.TCP_modbus_slave_entry.Name == slave_name);

                Manage_logs.SaveLogValue("NUM SLAVES TO CHECK : " + m_list_manage_thread_in_start_process.Count);

                if (m_list_manage_thread_in_start_process.Count == 0)
                {
                    m_timer_wait_connect.Stop();

                    b_some_slave_ok = Globals.GetTheInstance().List_slave_entry.Exists(slave_entry => slave_entry.Connected);

                    string s_log = b_some_slave_ok ? "INICIO PROCESO DE LECTURA SLAVES" : "ERROR EN EL INICIO PROCESO DE LECTURA SLAVES. NINGUN SLAVE CONECTADO";
                    Manage_logs.SaveLogValue(s_log);

                    if (b_some_slave_ok)
                    {
                        Dispatcher.Invoke(() => StartAvgValues());
                        b_manual_start_stop = false;
                        Dispatcher.Invoke(() => ((Storyboard)Resources["BlinkStoryboard"]).Begin());
                    }

                    Dispatcher.Invoke(() => Checkbox_start.IsChecked = b_some_slave_ok);
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
            if (m_list_read_write_state[(int)READ_WRITE_STATE.read_scs_modbus])
            {
                Read_scs_modbus(false);

                m_list_read_write_state[(int)READ_WRITE_STATE.first_read_scs_finish] = true;
                m_list_read_write_state[(int)READ_WRITE_STATE.first_read_samca_finish] = true;
            }
        }

        private void Timer_read_scs_fast_modbus_Tick(object sender, EventArgs e)
        {
            if (m_list_read_write_state[(int)READ_WRITE_STATE.read_scs_modbus])
            {
                Read_scs_modbus(true);
            }
        }


        private void Read_scs_modbus(bool fast_mode)
        {
            if (!b_flag_read_scs)
            {
                b_flag_read_scs = true;
                try
                {
                    //Analizar si está filtrado un slave
                    List<Manage_thread> list_read_thread = Globals.GetTheInstance().List_manage_thread;
                    if (m_selected_slave_entry != null)
                        list_read_thread = list_read_thread.Where(manage_thread => manage_thread.TCP_modbus_slave_entry.Name == m_selected_slave_entry.Name).ToList();


                    list_read_thread.ForEach(manage_thread =>
                    {
                        if (manage_thread.ManageModbus.Is_connected())
                        {
                            if (manage_thread.TCP_modbus_slave_entry.Fast_mode == fast_mode)
                            {
                                Tuple<READ_STATE, ushort[]> tuple_read = null;
                                if (manage_thread.TCP_modbus_slave_entry.Modbus_function == MODBUS_FUNCION.READ_HOLDING_REG)
                                    tuple_read = manage_thread.Read_holding_registers();
                                else
                                    tuple_read = manage_thread.Read_input_registers();


                                if (tuple_read.Item1 == READ_STATE.OK)
                                {
                                    if (tuple_read.Item2.Length != 0)
                                    {
                                        List<TCPModbusVarEntry> list_var_entry = Globals.GetTheInstance().List_slave_entry.First(slave_entry => slave_entry.Name.Equals(manage_thread.TCP_modbus_slave_entry.Name)).List_var_entry;

                                        list_var_entry.ForEach(var_entry =>
                                        {
                                            if (var_entry.DirModbus < manage_thread.TCP_modbus_slave_entry.Read_reg)
                                            {
                                                try
                                                {
                                                    Tuple<string, string> tuple_values = Functions.Read_from_array_convert_scale_offset(tuple_read.Item2, var_entry.DirModbus, var_entry.TypeVar, var_entry.Read_range_min, var_entry.Scaled_range_min, var_entry.Scale_factor, var_entry.Offset);

                                                    var_entry.Value = tuple_values.Item2;

                                                    #region Save into the array to send to SAMCA SLAVE

                                                    if (!string.IsNullOrEmpty(var_entry.Send_to_samca_pos))
                                                        if (int.Parse(var_entry.Send_to_samca_pos) != Constants.index_no_selected)
                                                            if (ushort.TryParse(tuple_values.Item1, out ushort received_value))
                                                                m_array_write_samca_values[int.Parse(var_entry.Send_to_samca_pos)] = received_value;

                                                    #endregion

                                                    #region AVG CALC

                                                    //WIND AVG VALUES
                                                    if ((LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.WIND_AVG_SBPT || (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.WIND_AVG_SAMCA)
                                                    {
                                                        Tuple<DateTime, double> read_value = new(DateTime.Now, double.Parse(var_entry.Value, Globals.GetTheInstance().nfi));

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

                                                                    break;
                                                                }
                                                        }
                                                    }


                                                    //CHECK WIND MAX VALUES
                                                    Dispatcher.Invoke(() =>
                                                    {
                                                        if (manage_thread.ManageModbus.Is_connected())
                                                        {
                                                            m_array_label_wind_avg_value
                                                            .Select((value, index) => new { Container = value, Position = index }).ToList()
                                                            .ForEach(label =>
                                                            {
                                                                if (m_array_label_wind_avg_value[label.Position].Content != null)
                                                                {
                                                                    //No se ha superado el valor máximo -> Analizar si se supera
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
                                                    if (manage_thread.ManageModbus.Is_connected())
                                                    {
                                                        m_array_wind_max_break_in_range.Select((value, index) => new { Value = value, Position = index }).ToList()
                                                        .ForEach(wind_break_in_range =>
                                                        {
                                                            //Se habia superado valor máximo -> analizar vuelta a la normalidad
                                                            if (!wind_break_in_range.Value)
                                                            {
                                                                string s_add_info = string.Empty;
                                                                switch ((WIND_AVG_POSITION)wind_break_in_range.Position)
                                                                {
                                                                    case WIND_AVG_POSITION.SBPT_3SEC:
                                                                        {
                                                                            if (m_array_wind_avg_values[wind_break_in_range.Position] < Globals.GetTheInstance().SBPT_trigger_3sec)
                                                                                b_wind_ok = DateTime.Now.Subtract(m_array_date_trigger_start_3sec[(int)WIND_DATE_TRIGGER_3SEC.SBPT_3SEC]) > TimeSpan.FromMinutes(Globals.GetTheInstance().SBPT_wind_delay_time_3sec);

                                                                            else
                                                                                m_array_date_trigger_start_3sec[(int)WIND_DATE_TRIGGER_3SEC.SBPT_3SEC] = DateTime.Now;

                                                                            break;
                                                                        }

                                                                    case WIND_AVG_POSITION.SAMCA_3SEC:
                                                                        {
                                                                            if (m_array_wind_avg_values[wind_break_in_range.Position] < Globals.GetTheInstance().SAMCA_trigger_3sec)
                                                                                b_wind_ok = DateTime.Now.Subtract(m_array_date_trigger_start_3sec[(int)WIND_DATE_TRIGGER_3SEC.SAMCA_3SEC]) > TimeSpan.FromMinutes(Globals.GetTheInstance().SAMCA_wind_delay_time_3sec);

                                                                            else
                                                                                m_array_date_trigger_start_3sec[(int)WIND_DATE_TRIGGER_3SEC.SAMCA_3SEC] = DateTime.Now;

                                                                            break;
                                                                        }

                                                                    case WIND_AVG_POSITION.SBPT_10MIN:
                                                                        {
                                                                            b_wind_ok = m_array_wind_avg_values[wind_break_in_range.Position] < m_array_low_histeresis_10min[(int)WIND_LOW_HIST_10MIN.SBPT_10MIN];

                                                                            break;
                                                                        }

                                                                    case WIND_AVG_POSITION.SAMCA_10MIN:
                                                                        {
                                                                            b_wind_ok = m_array_wind_avg_values[wind_break_in_range.Position] < m_array_low_histeresis_10min[(int)WIND_LOW_HIST_10MIN.SAMCA_10MIN];

                                                                            break;
                                                                        }
                                                                }
                                                            }
                                                        });
                                                    }


                                                    //INCLINOMETER AVG VALUES
                                                    if ((LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC1_SLOPE_LAT_AVG_SBPT ||
                                                            (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC2_SLOPE_LAT_AVG_SBPT ||
                                                            (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC3_SLOPE_LAT_AVG_SBPT ||
                                                            (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC4_SLOPE_LAT_AVG_SBPT ||
                                                            (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC5_SLOPE_LAT_AVG_SBPT)
                                                    {
                                                        if (!fast_mode)
                                                        {
                                                            Tuple<DateTime, double> read_value = new(DateTime.Now, double.Parse(var_entry.Value, Globals.GetTheInstance().nfi));

                                                            INC_LABEL_AVG_POSITION inc_label_pos =
                                                            (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC1_SLOPE_LAT_AVG_SBPT ? INC_LABEL_AVG_POSITION.INC1 :
                                                            (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC2_SLOPE_LAT_AVG_SBPT ? INC_LABEL_AVG_POSITION.INC2 :
                                                            (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC3_SLOPE_LAT_AVG_SBPT ? INC_LABEL_AVG_POSITION.INC3 :
                                                            (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC4_SLOPE_LAT_AVG_SBPT ? INC_LABEL_AVG_POSITION.INC4 :
                                                            INC_LABEL_AVG_POSITION.INC5;

                                                            m_list_inc_slope_read_values[(int)inc_label_pos].Add(read_value);
                                                            m_list_inc_slope_read_values[(int)inc_label_pos] = m_list_inc_slope_read_values[(int)inc_label_pos].Where(p => DateTime.Now.Subtract(p.Item1) < TimeSpan.FromSeconds(Globals.GetTheInstance().SBPT_inc_avg_interval)).ToList();
                                                            m_array_inc_slope_avg_values[(int)inc_label_pos] = m_list_inc_slope_read_values[(int)inc_label_pos].Select(p => p.Item2).Average();

                                                            var_entry.Value = m_array_inc_slope_avg_values[(int)inc_label_pos].ToString("0.00", Globals.GetTheInstance().nfi);

                                                            //Compare with TCU INC VALUE and change inrange arrays
                                                            if (m_list_inc_slope_read_values[(int)INC_LABEL_AVG_POSITION.TCU].Count >= 0 && !m_list_inc_slope_read_values.Exists(x => x.Count == 0))
                                                            {
                                                                m_array_inc_slope_emerg_stow_in_range[(int)inc_label_pos] = (Globals.GetTheInstance().Max_diff_tcu_inc_emergency_stow == 0) || (Math.Abs(m_array_inc_slope_avg_values[(int)inc_label_pos] - m_array_inc_slope_avg_values[(int)INC_LABEL_AVG_POSITION.TCU]) < Globals.GetTheInstance().Max_diff_tcu_inc_emergency_stow);
                                                                m_array_inc_slope_alarm_in_range[(int)inc_label_pos] = (Globals.GetTheInstance().Max_diff_tcu_inc_alarm == 0) || (Math.Abs(m_array_inc_slope_avg_values[(int)inc_label_pos] - m_array_inc_slope_avg_values[(int)INC_LABEL_AVG_POSITION.TCU]) < Globals.GetTheInstance().Max_diff_tcu_inc_alarm);
                                                            }
                                                        }
                                                    }


                                                    //CORRECTION LOAD PIN -> CHECK INC2 -> FACTORIZE DYN VALUES (este cálculo después de INC AVG y antes de DYN AVG )
                                                    if (var_entry.Correction_load_pin)
                                                    {
                                                        //Calcular el factor sobre INC2_MAINDRIVE_SLOPE_LAT_Y
                                                        TCPModbusVarEntry? correction_load_pin_alphatt_var = Globals.GetTheInstance().List_slave_entry
                                                               .SelectMany(slave_entry => slave_entry.List_var_entry)
                                                               .FirstOrDefault(var_entry => (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC2_SLOPE_LAT_AVG_SBPT);

                                                        if (correction_load_pin_alphatt_var != null)
                                                        {
                                                            if (!string.IsNullOrEmpty(correction_load_pin_alphatt_var.Value))
                                                            {
                                                                double key_value_second = Globals.GetTheInstance().List_slope_correction_alphaTT.FirstOrDefault(x => x >= double.Parse(correction_load_pin_alphatt_var.Value, Globals.GetTheInstance().nfi));
                                                                int index_second = Globals.GetTheInstance().List_slope_correction_alphaTT.FindIndex(x => x == key_value_second);
                                                                double key_value_first = index_second == 0 ? key_value_second : Globals.GetTheInstance().List_slope_correction_alphaTT[index_second - 1];

                                                                m_correction_load_pin_factor =
                                                                    Globals.GetTheInstance().Dictionary_slope_correction[key_value_first] +
                                                                    ((Globals.GetTheInstance().Dictionary_slope_correction[key_value_second] - Globals.GetTheInstance().Dictionary_slope_correction[key_value_first]) /
                                                                    (key_value_second - key_value_first)) *
                                                                    (double.Parse(correction_load_pin_alphatt_var.Value, Globals.GetTheInstance().nfi) - key_value_first);

                                                                //Para enviar a field safety window
                                                                DYN_LABEL_AVG_POSITION dyn_pos = (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.DYN2_AVG_SBPT ? DYN_LABEL_AVG_POSITION.DYN2 : DYN_LABEL_AVG_POSITION.DYN3;
                                                                m_list_dyn_values[(int)dyn_pos] = double.Parse(var_entry.Value, Globals.GetTheInstance().nfi);

                                                                var_entry.Value = Math.Round(double.Parse(var_entry.Value, Globals.GetTheInstance().nfi) / m_correction_load_pin_factor, 6).ToString("0.00", Globals.GetTheInstance().nfi);
                                                            }
                                                        }
                                                    }


                                                    //DYNANOMETER AVG VALUES
                                                    if ((LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.DYN1_AVG_SBPT ||
                                                          (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.DYN2_AVG_SBPT ||
                                                          (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.DYN3_AVG_SBPT)
                                                    {
                                                        double d_value = double.Parse(var_entry.Value, Globals.GetTheInstance().nfi);

                                                        //Para enviar a field safety window
                                                        if ((LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.DYN1_AVG_SBPT)
                                                            m_list_dyn_values[(int)DYN_LABEL_AVG_POSITION.DYN1] = d_value;


                                                        Tuple<DateTime, double> read_value = new(DateTime.Now, d_value);

                                                        DYN_LABEL_AVG_POSITION dyn_label_pos =
                                                           (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.DYN1_AVG_SBPT ? DYN_LABEL_AVG_POSITION.DYN1 :
                                                           (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.DYN2_AVG_SBPT ? DYN_LABEL_AVG_POSITION.DYN2 :
                                                           DYN_LABEL_AVG_POSITION.DYN3;

                                                        m_list_dyn_read_values[(int)dyn_label_pos].Add(read_value);
                                                        m_list_dyn_read_values[(int)dyn_label_pos] = m_list_dyn_read_values[(int)dyn_label_pos].Where(p => DateTime.Now.Subtract(p.Item1) < TimeSpan.FromSeconds(Globals.GetTheInstance().SBPT_dyn_avg_interval)).ToList();
                                                        m_array_dyn_avg_values[(int)dyn_label_pos] = m_list_dyn_read_values[(int)dyn_label_pos].Select(p => p.Item2).Average();

                                                        var_entry.Value = m_array_dyn_avg_values[(int)dyn_label_pos].ToString("0.00", Globals.GetTheInstance().nfi);


                                                        //Compare with limits and change inrange arrays
                                                        if (!m_list_dyn_read_values.Exists(x => x.Count == 0))
                                                        {
                                                            bool TCU_MOVING = true; //Todavia no se puede leer del TCU

                                                            if (TCU_MOVING && m_tracker_state != TRACKER_STATE.NONE)
                                                                m_array_dyn_excesive_force_emerg_stow_in_range[(int)dyn_label_pos] = (Globals.GetTheInstance().SBPT_dyn_max_mov_emerg_stow == 0) || (m_array_dyn_avg_values[(int)dyn_label_pos] < Globals.GetTheInstance().SBPT_dyn_max_mov_emerg_stow);

                                                            if (TCU_MOVING && m_tracker_state == TRACKER_STATE.STOW || m_tracker_state == TRACKER_STATE.EMERG_CMD_POS_OS_STOW || m_tracker_state == TRACKER_STATE.EMERG_AUTO_POS_OS_STOW)
                                                            {
                                                                bool excesive_force_alarm_moving_in_range = (Globals.GetTheInstance().SBPT_dyn_max_mov_alarm == 0) || (m_array_dyn_avg_values[(int)dyn_label_pos] < Globals.GetTheInstance().SBPT_dyn_max_mov_alarm);
                                                                bool excesive_force_alarm_static_in_range = (Globals.GetTheInstance().SBPT_dyn_max_static_alarm == 0) || (m_array_dyn_avg_values[(int)dyn_label_pos] < Globals.GetTheInstance().SBPT_dyn_max_static_alarm);

                                                                m_array_dyn_excesive_force_alarm_in_range[(int)dyn_label_pos] = excesive_force_alarm_moving_in_range && excesive_force_alarm_static_in_range;
                                                            }
                                                        }

                                                    }

                                                    #endregion

                                                    #region LINK TO SEND TO TCU

                                                    if ((LINK_TO_SEND_TCU)var_entry.Link_to_send_tcu != LINK_TO_SEND_TCU.NONE)
                                                    {
                                                        LinkToSendTCUClass? link_to_send_tcu_class = m_list_link_to_send_tcu.FirstOrDefault(link => link.Link_to_send_tcu == (LINK_TO_SEND_TCU)var_entry.Link_to_send_tcu);
                                                        if (link_to_send_tcu_class != null)
                                                        {
                                                            link_to_send_tcu_class.Value = Math.Round(double.Parse(var_entry.Value, Globals.GetTheInstance().nfi), 2);

                                                            if ((LINK_TO_SEND_TCU)var_entry.Link_to_send_tcu == LINK_TO_SEND_TCU.WIN_SPEED)
                                                            {
                                                                KeyValuePair<string, double>? tuple_slave_var = m_dictionary_wind_speed_slave_value.FirstOrDefault(tuple => tuple.Key == var_entry.Slave);
                                                                if (tuple_slave_var == null)
                                                                    m_dictionary_wind_speed_slave_value.Add(var_entry.Slave, link_to_send_tcu_class.Value);
                                                                else
                                                                    m_dictionary_wind_speed_slave_value[var_entry.Slave] = link_to_send_tcu_class.Value;

                                                                link_to_send_tcu_class.Value = m_dictionary_wind_speed_slave_value.Values.Max();
                                                            }
                                                        }
                                                    }

                                                    #endregion
                                                }
                                                catch (Exception ex)
                                                {
                                                    Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Read_scs_modbus)} -> list var entry foreach -> {var_entry.Name} / {var_entry.Value}  ->  { ex.Message}");
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
                }
                catch (Exception ex)
                {
                    Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Read_scs_modbus)} -> { ex.Message}");
                }

                b_flag_read_scs = false;
            }
        }

        #endregion


        #region Timer read TCU modbus

        private void Timer_read_tcu_modbus_Tick(object sender, EventArgs e)
        {
            if (m_list_read_write_state[(int)READ_WRITE_STATE.read_write_tcu_modbus])
            {
                Read_tcu_modbus();

                m_list_read_write_state[(int)READ_WRITE_STATE.first_read_tcu_finish] = true;
            }
        }

        private void Read_tcu_modbus()
        {
            if (!b_flag_read_tcu)
            {
                b_flag_read_tcu = true;
                try
                {
                    TCPModbusSlaveEntry? slave_entry = m_collection_slave_entry.FirstOrDefault(modbus_slave => modbus_slave.Slave_type == SLAVE_TYPE.TCU);
                    if (slave_entry != null)
                    {
                        Manage_thread manage_thread = Globals.GetTheInstance().List_manage_thread.First(manage_thread => manage_thread.TCP_modbus_slave_entry.Name == slave_entry.Name);
                        if (manage_thread.ManageModbus.Is_connected())
                        {
                            Tuple<READ_STATE, ushort[]> tuple_read = null;
                            if (slave_entry.Modbus_function == MODBUS_FUNCION.READ_HOLDING_REG)
                                tuple_read = manage_thread.Read_holding_registers();
                            else
                                tuple_read = manage_thread.Read_input_registers();

                            if (tuple_read.Item1 == READ_STATE.OK)
                            {
                                if (tuple_read.Item2.Length != 0)
                                {
                                    #region CHECK TCU CONSTANT

                                    bool read_ok_constant = true;
                                    TCUCodifiedStatusEntry? codified_status = Globals.GetTheInstance().List_tcu_codified_status.FirstOrDefault(codified_status => (LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic == LINK_TO_GRAPHIC_TCU.READ_OK_CONSTANT);
                                    if (codified_status != null)
                                    {
                                        Tuple<string, string> tuple_check_read_ok = Functions.Read_from_array(tuple_read.Item2, codified_status.DirModbus, codified_status.TypeVar, codified_status.Factor);
                                        if (int.Parse(tuple_check_read_ok.Item1) != codified_status.DirModbus)
                                            Manage_logs.SaveDepurValue($"ERROR EN VERIFICACION DE CONSTANTE PARA CHEQUEO DE LECTURA CORRECTA / CONSTANTE {codified_status.DirModbus} / LECTURA {int.Parse(tuple_check_read_ok.Item1)} ");
                                    }

                                    #endregion

                                    if (read_ok_constant)
                                    {
                                        Globals.GetTheInstance().List_tcu_codified_status.ForEach(codified_status =>
                                        {
                                            if (codified_status.DirModbus < manage_thread.TCP_modbus_slave_entry.Read_reg)
                                            {
                                                Tuple<string, string> tuple_values = Functions.Read_from_array(tuple_read.Item2, codified_status.DirModbus, codified_status.TypeVar, codified_status.Factor);

                                                codified_status.Value = tuple_values.Item2;

                                                #region Save into the array to send to SAMCA SLAVE

                                                if (!string.IsNullOrEmpty(codified_status.Send_to_samca_pos))
                                                    if (int.Parse(codified_status.Send_to_samca_pos) != Constants.index_no_selected)
                                                        if (ushort.TryParse(tuple_values.Item1, out ushort received_value))
                                                            m_array_write_samca_values[int.Parse(codified_status.Send_to_samca_pos)] = received_value;

                                                #endregion

                                                //TRACKER STATE
                                                if ((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic == LINK_TO_GRAPHIC_TCU.TRACKER_STATE_TCU)
                                                {
                                                    if (Enum.IsDefined(typeof(TRACKER_STATE), int.Parse(codified_status.Value)))
                                                    {
                                                        TRACKER_STATE m_codified_status_state_last = m_tracker_state;

                                                        m_tracker_state = (TRACKER_STATE)int.Parse(codified_status.Value);

                                                        codified_status.Value =
                                                            m_tracker_state == TRACKER_STATE.POWER_ON_DIAGNOSIS ? "POWER ON DIAGNOSIS" :
                                                            m_tracker_state == TRACKER_STATE.OFFLINE ? "OFFLINE" :
                                                            m_tracker_state == TRACKER_STATE.OFFLINE_WITH_ERROR ? "OFFLINE WITH ERROR" :
                                                            m_tracker_state == TRACKER_STATE.ONLINE_WAITING_CMD ? "ONLINE WAITING" :
                                                            m_tracker_state == TRACKER_STATE.POSITION_EL_DEG ? "POSITION ELEVATION" :
                                                            m_tracker_state == TRACKER_STATE.POSITION_MAIN_DRIVE_MM ? "POSITION MAIN DRIVE" :
                                                            m_tracker_state == TRACKER_STATE.STOW ? "STOW" :
                                                            m_tracker_state == TRACKER_STATE.TRACKING_SUN_XYZ ? "TRACKING SUN XYZ" :
                                                            m_tracker_state == TRACKER_STATE.TRACKING_DEF_XYZ ? "TRACKING DEF XYZ" :
                                                            m_tracker_state == TRACKER_STATE.MAINT_CHK_LOCK_1 ? "MAINTENANCE CHK LOCK 1" :
                                                            m_tracker_state == TRACKER_STATE.MAINT_CHK_LOCK_2 ? "MAINTENANCE CHK LOCK 1" :
                                                            m_tracker_state == TRACKER_STATE.MAIN_CHK_1_LH ? "MAINTENANCE CHK 1 LH" :
                                                            m_tracker_state == TRACKER_STATE.DRIVE_SPEED ? "DRIVE SPEED" :
                                                            m_tracker_state == TRACKER_STATE.EMERG_CMD_POS_OS_STOW ? "EMERG CMD POS OS STOW" :
                                                            m_tracker_state == TRACKER_STATE.EMERG_AUTO_POS_OS_STOW ? "EMERG AUTO POS OS STOW" :
                                                            string.Empty;

                                                        if (m_tracker_state == TRACKER_STATE.OFFLINE_WITH_ERROR && m_codified_status_state_last != TRACKER_STATE.NONE && m_tracker_state != m_codified_status_state_last)
                                                        {
                                                            Functions.Send_alarm_mail("SBP TRACKER ALARM", $"TRACKER LAST STATUS: {m_codified_status_state_last} - CURRENT STATUS : { m_tracker_state }");
                                                        }
                                                    }

                                                    else
                                                        codified_status.Value = string.Empty;
                                                }

                                                //TRACKER MODE
                                                if ((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic == LINK_TO_GRAPHIC_TCU.TRACKER_MODE_TCU)
                                                {
                                                    string codif_value =
                                                        (int.Parse(codified_status.Value) & (int)TRACKER_MODE.LMS_MODE) == (int)TRACKER_MODE.LMS_MODE ? "LMS MODE" :
                                                        (int.Parse(codified_status.Value) & (int)TRACKER_MODE.SCS_MODE) == (int)TRACKER_MODE.SCS_MODE ? "SCS MODE" : "-";

                                                    codified_status.Value = $"({string.Format("0x{0:X2}", int.Parse(codified_status.Value))}) {codif_value}";
                                                }

                                                //TCU POS EL
                                                else if ((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic == LINK_TO_GRAPHIC_TCU.TRACKER_POS_EL)
                                                {
                                                    Tuple<DateTime, double> read_value = new(DateTime.Now, double.Parse(codified_status.Value, Globals.GetTheInstance().nfi));

                                                    m_list_inc_slope_read_values[(int)INC_LABEL_AVG_POSITION.TCU].Add(read_value);
                                                    m_list_inc_slope_read_values[(int)INC_LABEL_AVG_POSITION.TCU] = m_list_inc_slope_read_values[(int)INC_LABEL_AVG_POSITION.TCU].Where(p => DateTime.Now.Subtract(p.Item1) < TimeSpan.FromSeconds(Globals.GetTheInstance().SBPT_inc_avg_interval)).ToList();
                                                    m_array_inc_slope_avg_values[(int)INC_LABEL_AVG_POSITION.TCU] = m_list_inc_slope_read_values[(int)INC_LABEL_AVG_POSITION.TCU].Select(p => p.Item2).Average();

                                                    codified_status.Value = m_array_inc_slope_avg_values[(int)INC_LABEL_AVG_POSITION.TCU].ToString("0.00", Globals.GetTheInstance().nfi);
                                                }

                                                else if ((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic == LINK_TO_GRAPHIC_TCU.DATE_RTC)
                                                {
                                                    TimeSpan time = TimeSpan.FromSeconds(int.Parse(codified_status.Value));
                                                    DateTime dateTime = DateTime.UnixEpoch.Add(time);
                                                    dateTime = dateTime.AddMilliseconds(m_tracker_milliseconds);
                                                    codified_status.Value = dateTime.ToString(Globals.GetTheInstance().Date_format, new CultureInfo(Globals.GetTheInstance().Format_provider));
                                                }

                                                else if ((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic == LINK_TO_GRAPHIC_TCU.MILISECONDS)
                                                    m_tracker_milliseconds = int.Parse(codified_status.Value);

                                                else if ((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic == LINK_TO_GRAPHIC_TCU.SAFETY_SUPERVISOR)
                                                    codified_status.Value = string.Format("0x{0:X4}", int.Parse(codified_status.Value));

                                                else if ((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic == LINK_TO_GRAPHIC_TCU.ALARM_WARNING_INDEX)
                                                {
                                                    string s_codified = string.Empty;
                                                    if (Enum.IsDefined(typeof(TRACKER_ALARM_DIAGNOSIS), int.Parse(codified_status.Value)))
                                                    {
                                                        s_codified = Enum.GetName(typeof(TRACKER_ALARM_DIAGNOSIS), int.Parse(codified_status.Value));
                                                    }
                                                    codified_status.Value = $"{codified_status.Value} - {s_codified.Replace("_", " ")}";

                                                }
                                            }
                                        });
                                    }
                                }
                            }

                            else if (tuple_read.Item1 == READ_STATE.ERROR && Globals.GetTheInstance().Depur_enable == BIT_STATE.ON)
                                Manage_logs.SaveErrorValue($"Error read image -> {manage_thread.TCP_modbus_slave_entry.Name} / {manage_thread.TCP_modbus_slave_entry.IP_primary} / {manage_thread.TCP_modbus_slave_entry.Port} / {manage_thread.TCP_modbus_slave_entry.Dir_ini} / {manage_thread.TCP_modbus_slave_entry.UnitId}");

                        }
                    }
                }
                catch (Exception ex)
                {
                    Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Read_tcu_modbus)} -> {ex.Message}");
                }
                b_flag_read_tcu = false;
            }
        }

        #endregion


        #region Timer write TCU command modbus

        private void Timer_write_tcu_command_modbus_Tick(object sender, EventArgs e)
        {
            if (m_list_read_write_state[(int)READ_WRITE_STATE.read_write_tcu_modbus] && Globals.GetTheInstance().Enable_write_tcu == BIT_STATE.ON)
                Write_tcu_command_modbus();
        }

        private void Write_tcu_command_modbus()
        {
            TCPModbusSlaveEntry? slave_entry = Globals.GetTheInstance().List_slave_entry.FirstOrDefault(slave_entry => slave_entry.Slave_type == SLAVE_TYPE.TCU);
            if (slave_entry != null)
            {
                try
                {
                    List<ushort> list_write_values = new();

                    bool queue_command = m_queue_commands.Count != 0;
                    string s_log_command = m_queue_commands.Count != 0 ? "QUEUE COMMAND TO TCU - " : "WATCHDOG COMMAND TO TCU - ";

                    if (m_queue_commands.Count != 0)
                        list_write_values = m_queue_commands.Dequeue();
                    
                    else
                    {
                        list_write_values.Add(Constants.CMD_SCS_METEO_STATION_VALUES);

                        Dispatcher.Invoke(() =>
                        {
                            m_list_link_to_send_tcu.ForEach(link_to_send_tcu =>
                            {
                                ushort send_to_tcu_value =
                                    link_to_send_tcu.Link_to_send_tcu == LINK_TO_SEND_TCU.TRACKER_WD || link_to_send_tcu.Link_to_send_tcu == LINK_TO_SEND_TCU.METEO_WD || link_to_send_tcu.Link_to_send_tcu == LINK_TO_SEND_TCU.SAFETY_SUPERVISOR ? (ushort)link_to_send_tcu.Value :
                                    (ushort)(link_to_send_tcu.Value * 100); //Multiplicarlo por 100 para los decimales

                                list_write_values.Add(send_to_tcu_value);
                            });
                        });
                    }

                    list_write_values.ForEach(value => s_log_command += $"{value} ");
                    Manage_logs.SaveCommandValue(s_log_command);

                    Manage_thread manage_thread = Globals.GetTheInstance().List_manage_thread.First(manage_thread => manage_thread.TCP_modbus_slave_entry.Name.Equals(slave_entry.Name));
                    bool write_ok = manage_thread.Write_multiple_registers(Globals.GetTheInstance().Modbus_dir_tcu_command, list_write_values.ToArray());
                    if (queue_command && !write_ok)
                        m_queue_commands.Enqueue(list_write_values);

                    LinkToSendTCUClass link_to_send_tcu_class_scada = m_list_link_to_send_tcu.First(link => link.Link_to_send_tcu == LINK_TO_SEND_TCU.TRACKER_WD);
                    link_to_send_tcu_class_scada.Value = link_to_send_tcu_class_scada.Value == Constants.MAX_SCADA_WD ? 1 : link_to_send_tcu_class_scada.Value + 1;

                    LinkToSendTCUClass link_to_send_tcu_class_meteo = m_list_link_to_send_tcu.First(link => link.Link_to_send_tcu == LINK_TO_SEND_TCU.METEO_WD);
                    link_to_send_tcu_class_meteo.Value = link_to_send_tcu_class_meteo.Value == ushort.MaxValue ? 1 : link_to_send_tcu_class_meteo.Value + 1;
                }
                catch (Exception ex)
                {
                    Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Write_tcu_command_modbus)} -> {ex.Message}");
                }
            }
        }

        #endregion

        #region Timer write TCU datetime modbus

        private void Timer_write_tcu_datetime_modbus_Tick(object sender, EventArgs e)
        {

            TCPModbusSlaveEntry? slave_entry = Globals.GetTheInstance().List_slave_entry.FirstOrDefault(slave_entry => slave_entry.Slave_type == SLAVE_TYPE.TCU);
            if (slave_entry != null && Globals.GetTheInstance().Enable_write_tcu == BIT_STATE.ON)
            {
                try
                {
                    //Mostrar valor de las variables de lectura
                    if (m_list_read_write_state[(int)READ_WRITE_STATE.read_write_tcu_modbus])
                    {
                        ushort year = (ushort)DateTime.Now.Year;
                        ushort month = (ushort)DateTime.Now.Month;
                        ushort day = (ushort)DateTime.Now.Day;
                        ushort hour = (ushort)DateTime.Now.Hour;
                        ushort minute = (ushort)DateTime.Now.Minute;
                        ushort second = (ushort)DateTime.Now.Second;
                        ushort millisecond = (ushort)DateTime.Now.Millisecond;

                        List<ushort> list_values = new() { year, month, day, hour, minute, second, millisecond };

                        Manage_thread manage_thread = Globals.GetTheInstance().List_manage_thread.First(manage_thread => manage_thread.TCP_modbus_slave_entry.Name == slave_entry.Name);
                        manage_thread.Write_multiple_registers(Globals.GetTheInstance().Modbus_dir_tcu_datetime, list_values.ToArray());

                        string s_log_command = $"DATETIME COMMAND TO TCU -";
                        list_values.ForEach(value => s_log_command += $"{value} ");
                        Manage_logs.SaveCommandValue(s_log_command);
                    }
                }
                catch (Exception ex)
                {
                    Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Timer_write_tcu_datetime_modbus_Tick)} -> {ex.Message}");
                }
            }
        }

        #endregion


        #region Timer write SAMCA

        private void Timer_write_samca_modbus_Tick(object sender, EventArgs e)
        {
            TCPModbusSlaveEntry? slave_entry = Globals.GetTheInstance().List_slave_entry.FirstOrDefault(slave_entry => slave_entry.Slave_type == SLAVE_TYPE.SAMCA);
            if (slave_entry != null && m_list_read_write_state[(int)READ_WRITE_STATE.read_write_samca_modbus] && Globals.GetTheInstance().Enable_write_samca == BIT_STATE.ON)
            {
                try
                {
                    //Los Watchdog se cogen del array send to TCU
                    m_array_write_samca_values[(int)WD_SEND_TO_SAMCA_POS.SBPT_METEO] = (ushort)m_list_link_to_send_tcu.First(x => x.Link_to_send_tcu == LINK_TO_SEND_TCU.METEO_WD).Value;
                    m_array_write_samca_values[(int)WD_SEND_TO_SAMCA_POS.SBP_TRACKER] = (ushort)m_list_link_to_send_tcu.First(x => x.Link_to_send_tcu == LINK_TO_SEND_TCU.TRACKER_WD).Value;

                    Manage_thread manage_thread = Globals.GetTheInstance().List_manage_thread.First(manage_thread => manage_thread.TCP_modbus_slave_entry.Name == slave_entry.Name);
                    manage_thread.Write_multiple_registers(Globals.GetTheInstance().Modbus_dir_write_samca, m_array_write_samca_values);

                    string s_log_command = $"COMMAND TO SAMCA - ";
                    m_array_write_samca_values.ToList().ForEach(value => s_log_command += $"{value} ");
                    Manage_logs.SaveCommandValue(s_log_command);
                }
                catch (Exception ex)
                {
                    Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Timer_write_samca_modbus_Tick)} -> {ex.Message}");
                }
            }
        }
        #endregion



        #region Timer refresh scada

        private void Timer_refresh_scada_Tick(object sender, EventArgs e)
        {
            if (m_list_read_write_state[(int)READ_WRITE_STATE.read_scs_modbus])
            {
                //Var entry
                Globals.GetTheInstance().List_manage_thread
                    .Where(manage_thread => manage_thread.ManageModbus.Is_connected())
                    .Select(manage_thread => manage_thread.TCP_modbus_slave_entry).ToList()
                    .ForEach(slave_entry => slave_entry.List_var_entry
                        .ForEach(var_entry =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                //Graphic mode
                                if (Radio_graphic_mode.IsChecked == true)
                                {
                                    //Var entry value
                                    ControlLinq? control_linq = m_array_label_graphic_title
                                    .Select((item, index) => new ControlLinq { Value = item.Content.ToString(), Position = index })
                                    .FirstOrDefault(control => control.Position.Equals(var_entry.Graphic_pos));
                                    if (control_linq != null)
                                        m_array_label_graphic_value[control_linq.Position].Content = $"{var_entry.Value} {var_entry.Unit}";

                                    //WIND AVG values
                                    m_array_label_wind_avg_value[(int)WIND_AVG_POSITION.SBPT_3SEC].Content = Math.Round(m_array_wind_avg_values[(int)WIND_AVG_POSITION.SBPT_3SEC], 2).ToString(Globals.GetTheInstance().nfi);
                                    m_array_label_wind_avg_value[(int)WIND_AVG_POSITION.SBPT_10MIN].Content = Math.Round(m_array_wind_avg_values[(int)WIND_AVG_POSITION.SBPT_10MIN], 2).ToString(Globals.GetTheInstance().nfi);

                                    m_array_label_wind_avg_value[(int)WIND_AVG_POSITION.SAMCA_3SEC].Content = Math.Round(m_array_wind_avg_values[(int)WIND_AVG_POSITION.SAMCA_3SEC], 2).ToString(Globals.GetTheInstance().nfi);
                                    m_array_label_wind_avg_value[(int)WIND_AVG_POSITION.SAMCA_10MIN].Content = Math.Round(m_array_wind_avg_values[(int)WIND_AVG_POSITION.SAMCA_10MIN], 2).ToString(Globals.GetTheInstance().nfi);

                                    m_array_wind_max_break_in_range.Select((value, index) => new { Value = value, Position = index }).ToList()
                                        .ForEach(wind_break_in_range =>
                                        {
                                            //Se habia superado valor máximo -> analizar vuelta a la normalidad
                                            if (!wind_break_in_range.Value && b_wind_ok)
                                            {
                                                Manage_logs.SaveLogValue($"SE HA BAJADO DE LA VELOCIDAD HISTERESIS PARA LA MEDIA -> {(WIND_AVG_POSITION)wind_break_in_range.Position} / VELOCIDAD MEDIA REG -> {m_array_wind_avg_values[wind_break_in_range.Position]}");

                                                m_array_border_wind_avg_value[wind_break_in_range.Position].BorderBrush = Brushes.Black;
                                                m_array_label_wind_avg_value[wind_break_in_range.Position].Foreground = Brushes.Black;
                                                m_array_wind_max_break_in_range[wind_break_in_range.Position] = true;
                                            }
                                        });

                                    //Field safety window
                                    if ((LINK_TO_AVG)var_entry.Link_to_avg != LINK_TO_AVG.NONE)
                                    {
                                        FieldSafetyAnalisisWindow? safety_analisis_window = Application.Current.Windows.OfType<FieldSafetyAnalisisWindow>().FirstOrDefault();
                                        if (safety_analisis_window != null)
                                        {
                                            safety_analisis_window.ShowValue((LINK_TO_AVG)var_entry.Link_to_avg, var_entry.Value);

                                            if ((LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.INC2_SLOPE_LAT_AVG_SBPT)
                                                safety_analisis_window.ShowCorrectionLoadPinFactor(m_correction_load_pin_factor);

                                            if ((LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.DYN1_AVG_SBPT ||
                                                (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.DYN2_AVG_SBPT ||
                                                (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.DYN3_AVG_SBPT)
                                            {
                                                DYN_LABEL_AVG_POSITION dyn_pos =
                                                    (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.DYN1_AVG_SBPT ? DYN_LABEL_AVG_POSITION.DYN1 :
                                                    (LINK_TO_AVG)var_entry.Link_to_avg == LINK_TO_AVG.DYN2_AVG_SBPT ? DYN_LABEL_AVG_POSITION.DYN2 : DYN_LABEL_AVG_POSITION.DYN3;

                                                safety_analisis_window.ShowDynReadValue((LINK_TO_AVG)var_entry.Link_to_avg, m_list_dyn_values[(int)dyn_pos]);
                                            }
                                        }

                                    }

                                }

                                //Table mode
                                else if (Radio_table_mode.IsChecked == true)
                                {
                                    m_collection_var_entry.First(var_entry_table => var_entry_table.Name.Equals(var_entry.Name)).Value = var_entry.Value;
                                }
                            });
                        })
                    );

                //Mode table -> List var 
                Dispatcher.Invoke(() =>
                {
                    if (Radio_table_mode.IsChecked == true)
                        Listview_read_var_entry.Items.Refresh();
                });

            }


            if (m_list_read_write_state[(int)READ_WRITE_STATE.read_write_tcu_modbus])
            {
                //Manage send to TCU var
                Dispatcher.Invoke(() =>
                {
                    #region SAFETY SUPERVISOR

                    int safety_supervisor_value = 0x00;

                    #region AUTOTRACK SCADA

                    safety_supervisor_value =
                        SwitchSlider_autotracking.ToggledState == false ?
                        Functions.SetBitTo1(safety_supervisor_value, (int)FIELD_SAFETY_VALUE_BIT.AUTOTRACK_DISABLE) :
                        Functions.SetBitTo0(safety_supervisor_value, (int)FIELD_SAFETY_VALUE_BIT.AUTOTRACK_DISABLE);

                    m_array_border_field_safety[(int)FIELD_SAFETY_CHECK.AUTOTRACK_SCADA].Background = SwitchSlider_autotracking.ToggledState == true ? Brushes.DarkBlue : Brushes.White;

                    #endregion

                    #region WIND CONDITIONS

                    safety_supervisor_value =
                        m_array_wind_max_break_in_range.ToList().Exists(wind_break_in_range => wind_break_in_range == false) ?
                        Functions.SetBitTo0(safety_supervisor_value, (int)FIELD_SAFETY_VALUE_BIT.WIND_CONDITIONS_OK) :
                        Functions.SetBitTo1(safety_supervisor_value, (int)FIELD_SAFETY_VALUE_BIT.WIND_CONDITIONS_OK);

                    m_array_border_field_safety[(int)FIELD_SAFETY_CHECK.WIND_CONDITIONS_OK].Background = m_array_wind_max_break_in_range.ToList().Exists(wind_break_in_range => wind_break_in_range == false) ? Brushes.White : Brushes.DarkBlue;

                    #endregion

                    #region SLAVE COMMUNICATION

                    safety_supervisor_value =
                        Globals.GetTheInstance().List_slave_entry.Exists(slave_entry => !slave_entry.Connected && slave_entry.Enable_communication) ?
                        Functions.SetBitTo0(safety_supervisor_value, (int)FIELD_SAFETY_VALUE_BIT.SLAVE_COMM_OK) :
                        Functions.SetBitTo1(safety_supervisor_value, (int)FIELD_SAFETY_VALUE_BIT.SLAVE_COMM_OK);

                    m_array_border_field_safety[(int)FIELD_SAFETY_CHECK.SBPT_SLAVES_COMM_OK].Background = Globals.GetTheInstance().List_slave_entry.Exists(slave_entry => !slave_entry.Connected && slave_entry.Enable_communication) ? Brushes.White : Brushes.DarkBlue;

                    #endregion

                    #region EMERGENCY STOW PUSH BUTTON

                    safety_supervisor_value =
                        Toogle_emergency_stop.IsChecked == false ?
                        Functions.SetBitTo0(safety_supervisor_value, (int)FIELD_SAFETY_VALUE_BIT.EMERGENCY_STOW_BUTTON) :
                        Functions.SetBitTo1(safety_supervisor_value, (int)FIELD_SAFETY_VALUE_BIT.EMERGENCY_STOW_BUTTON);

                    m_array_border_field_safety[(int)FIELD_SAFETY_CHECK.SBPT_EMERGENCY_STOW].Background = Toogle_emergency_stop.IsChecked == true ? Brushes.DarkBlue : Brushes.White;

                    #endregion

                    #region SBPT INC IN RANGES

                    safety_supervisor_value =
                        m_array_inc_slope_emerg_stow_in_range.All(x => x == true) ?
                        Functions.SetBitTo1(safety_supervisor_value, (int)FIELD_SAFETY_VALUE_BIT.INC_IN_RANGES_FOR_EMERGENCY_STOW) :
                        Functions.SetBitTo0(safety_supervisor_value, (int)FIELD_SAFETY_VALUE_BIT.INC_IN_RANGES_FOR_EMERGENCY_STOW);

                    m_array_border_field_safety[(int)FIELD_SAFETY_CHECK.SBPT_INC_IN_RANGES_FOR_EMERG_STOW].Background = m_array_inc_slope_emerg_stow_in_range.All(x => x == true) ? Brushes.DarkBlue : Brushes.White;

                    safety_supervisor_value =
                        m_array_inc_slope_alarm_in_range.All(x => x == true) ?
                        Functions.SetBitTo1(safety_supervisor_value, (int)FIELD_SAFETY_VALUE_BIT.INC_IN_RANGES_FOR_ALARM) :
                        Functions.SetBitTo0(safety_supervisor_value, (int)FIELD_SAFETY_VALUE_BIT.INC_IN_RANGES_FOR_ALARM);

                    m_array_border_field_safety[(int)FIELD_SAFETY_CHECK.SBPT_INC_IN_RANGES_FOR_ALARM].Background = m_array_inc_slope_alarm_in_range.All(x => x == true) ? Brushes.DarkBlue : Brushes.White;

                    #endregion

                    #region SBPT DYN IN RANGES

                    safety_supervisor_value =
                        m_array_dyn_excesive_force_emerg_stow_in_range.All(x => x == true) ?
                        Functions.SetBitTo1(safety_supervisor_value, (int)FIELD_SAFETY_VALUE_BIT.DYN_IN_RANGES_FOR_EMERGENCY_STOW) :
                        Functions.SetBitTo0(safety_supervisor_value, (int)FIELD_SAFETY_VALUE_BIT.DYN_IN_RANGES_FOR_EMERGENCY_STOW);

                    m_array_border_field_safety[(int)FIELD_SAFETY_CHECK.SBPT_DYN_IN_RANGES_FOR_EMERG_STOW].Background = m_array_dyn_excesive_force_emerg_stow_in_range.All(x => x == true) ? Brushes.DarkBlue : Brushes.White;

                    safety_supervisor_value =
                        m_array_dyn_excesive_force_alarm_in_range.All(x => x == true) ?
                        Functions.SetBitTo1(safety_supervisor_value, (int)FIELD_SAFETY_VALUE_BIT.DYN_IN_RANGES_FOR_ALARM) :
                        Functions.SetBitTo0(safety_supervisor_value, (int)FIELD_SAFETY_VALUE_BIT.DYN_IN_RANGES_FOR_ALARM);

                    m_array_border_field_safety[(int)FIELD_SAFETY_CHECK.SBPT_DYN_IN_RANGES_FOR_ALARM].Background = m_array_dyn_excesive_force_alarm_in_range.All(x => x == true) ? Brushes.DarkBlue : Brushes.White;

                    #endregion

                    m_list_link_to_send_tcu.First(link => link.Link_to_send_tcu == LINK_TO_SEND_TCU.SAFETY_SUPERVISOR).Value = Globals.GetTheInstance().TCU_depur == BIT_STATE.ON ? 0xFFFF : safety_supervisor_value;

                    #endregion

                    m_list_link_to_send_tcu.ForEach(link =>
                    {
                        link.Label_tcu_mode_value.Content = link.Link_to_send_tcu == LINK_TO_SEND_TCU.SAFETY_SUPERVISOR ? string.Format("0x{0:X4}", link.Value) : $"{link.Value} {link.Unit}";
                        link.Label_graphic_mode_value.Content = link.Link_to_send_tcu == LINK_TO_SEND_TCU.SAFETY_SUPERVISOR ? string.Format("0x{0:X4}", link.Value) : $"{link.Value} {link.Unit}";
                    });
                });

                //Show graphic mode values
                Dispatcher.Invoke(() =>
                {
                    if (Radio_graphic_mode.IsChecked == true)
                    {
                        Globals.GetTheInstance().List_tcu_codified_status.ForEach(codified_status =>
                        {
                            if (!string.IsNullOrEmpty(codified_status.Value))
                            {
                                if ((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic != LINK_TO_GRAPHIC_TCU.NONE)
                                {
                                    try
                                    {
                                        //A nivel de BIT
                                        if ((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic == LINK_TO_GRAPHIC_TCU.DIGITAL_OUTPUTS)
                                        {
                                            bool main_enable = false;
                                            bool lock1_enable = false;
                                            bool lock2_enable = false;

                                            var drive_bits = Enum.GetValues(typeof(DIGITAL_OUTPUT_BITS));
                                            foreach (dynamic drive_bit in drive_bits)
                                            {
                                                switch (drive_bit)
                                                {
                                                    case DIGITAL_OUTPUT_BITS.ECU_DRV_ENABLE_MAIN:
                                                        {
                                                            main_enable = Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)drive_bit);
                                                            Border_main_drive.Background = main_enable ? new SolidColorBrush(Color.FromArgb(0xFF, 0x09, 0x09, 0xB5)) : new SolidColorBrush(Color.FromArgb(0x00, 0x09, 0x09, 0xB5));
                                                            break;
                                                        }

                                                    case DIGITAL_OUTPUT_BITS.ECU_DRV_DIR_MAIN:
                                                        {
                                                            Image_main_drive_forward_enable.Visibility = main_enable && Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;
                                                            Image_main_drive_forward_disable.Visibility = !main_enable || Functions.IsBitSetTo0(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;

                                                            Image_main_drive_backward_enable.Visibility = main_enable && Functions.IsBitSetTo0(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;
                                                            Image_main_drive_backward_disable.Visibility = !main_enable || Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;

                                                            break;
                                                        }

                                                    case DIGITAL_OUTPUT_BITS.ECU_DRV_ENABLE_LOCK1:
                                                        {
                                                            lock1_enable = Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)drive_bit);
                                                            Border_lock1.Background = lock1_enable ? new SolidColorBrush(Color.FromArgb(0xFF, 0x09, 0x09, 0xB5)) : new SolidColorBrush(Color.FromArgb(0x00, 0x09, 0x09, 0xB5));
                                                            break;
                                                        }

                                                    case DIGITAL_OUTPUT_BITS.ECU_DRV_DIR_LOCK1:
                                                        {
                                                            Image_lock1_forward_enable.Visibility = lock1_enable && Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;
                                                            Image_lock1_forward_disable.Visibility = !lock1_enable || Functions.IsBitSetTo0(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;

                                                            Image_lock1_backward_enable.Visibility = lock1_enable && Functions.IsBitSetTo0(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;
                                                            Image_lock1_backward_disable.Visibility = !lock1_enable || Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;

                                                            break;
                                                        }

                                                    case DIGITAL_OUTPUT_BITS.ECU_DRV_ENABLE_LOCK2:
                                                        {
                                                            lock2_enable = Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)drive_bit);
                                                            Border_lock2.Background = lock2_enable ? new SolidColorBrush(Color.FromArgb(0xFF, 0x09, 0x09, 0xB5)) : new SolidColorBrush(Color.FromArgb(0x00, 0x09, 0x09, 0xB5));
                                                            break;
                                                        }

                                                    case DIGITAL_OUTPUT_BITS.ECU_DRV_DIR_LOCK2:
                                                        {
                                                            Image_lock2_forward_enable.Visibility = lock2_enable && Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;
                                                            Image_lock2_forward_disable.Visibility = !lock2_enable || Functions.IsBitSetTo0(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;

                                                            Image_lock2_backward_enable.Visibility = lock2_enable && Functions.IsBitSetTo0(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;
                                                            Image_lock2_backward_disable.Visibility = !lock2_enable || Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)drive_bit) ? Visibility.Visible : Visibility.Collapsed;

                                                            break;
                                                        }
                                                }
                                            }
                                        }

                                        else if ((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic == LINK_TO_GRAPHIC_TCU.LOCK_UNLOCK)
                                        {
                                            m_l1_state =
                                                Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)LOCK_UNLOCK_BITS.RUN_LOCK_LD1) ? LOCK_UNLOCK_STATE.LOCK :
                                                Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)LOCK_UNLOCK_BITS.RUN_UNLOCK_LD1) ? LOCK_UNLOCK_STATE.UNLOCK :
                                                m_l1_state;

                                            m_l2_state =
                                                Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)LOCK_UNLOCK_BITS.RUN_LOCK_LD2) ? LOCK_UNLOCK_STATE.LOCK :
                                                Functions.IsBitSetTo1(int.Parse(codified_status.Value), (int)LOCK_UNLOCK_BITS.RUN_UNLOCK_LD2) ? LOCK_UNLOCK_STATE.UNLOCK :
                                                m_l2_state;

                                            Manage_logs.SaveDepurValue($"LOCK1 STATE - {m_l1_state} / LOCK2 STATE {m_l2_state}");
                                        }

                                        //A nivel de valor
                                        else
                                        {
                                            //EXTRACT - RETRACT
                                            if (m_list_extract_retract.Contains((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic))
                                            {
                                                bool save = false;
                                                if ((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic == LINK_TO_GRAPHIC_TCU.LOCK1_CURRENT_EXTRACT && m_l1_state == LOCK_UNLOCK_STATE.LOCK)
                                                    save = true;
                                                else if ((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic == LINK_TO_GRAPHIC_TCU.LOCK1_CURRENT_RETRACT && m_l1_state == LOCK_UNLOCK_STATE.UNLOCK)
                                                    save = true;
                                                else if ((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic == LINK_TO_GRAPHIC_TCU.LOCK1_SEC_EXTRACT && m_l1_state == LOCK_UNLOCK_STATE.LOCK)
                                                    save = true;
                                                else if ((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic == LINK_TO_GRAPHIC_TCU.LOCK1_SEC_RETRACT && m_l1_state == LOCK_UNLOCK_STATE.UNLOCK)
                                                    save = true;

                                                else if ((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic == LINK_TO_GRAPHIC_TCU.LOCK2_CURRENT_EXTRACT && m_l2_state == LOCK_UNLOCK_STATE.LOCK)
                                                    save = true;
                                                else if ((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic == LINK_TO_GRAPHIC_TCU.LOCK2_CURRENT_RETRACT && m_l2_state == LOCK_UNLOCK_STATE.UNLOCK)
                                                    save = true;
                                                else if ((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic == LINK_TO_GRAPHIC_TCU.LOCK2_SEC_EXTRACT && m_l2_state == LOCK_UNLOCK_STATE.LOCK)
                                                    save = true;
                                                else if ((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic == LINK_TO_GRAPHIC_TCU.LOCK2_SEC_RETRACT && m_l2_state == LOCK_UNLOCK_STATE.UNLOCK)
                                                    save = true;

                                                if (save)
                                                {
                                                    Label current_label = m_keyValuePair_codified_status_value.First(key_value => key_value.Key == (LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic).Value;
                                                    current_label.Content = $"{codified_status.Value} {codified_status.Unit}";

                                                    Manage_logs.SaveDepurValue($"LINK TO GRAPHIC TCU - {(LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic} / VALUE - {codified_status.Value} / LABEL - {current_label.Name} / LOCK1 STATE - {m_l1_state} / LOCK2 - STATE {m_l2_state}");
                                                }
                                            }

                                            //OTHER
                                            else
                                            {
                                                Label current_label = m_keyValuePair_codified_status_value.FirstOrDefault(key_value => key_value.Key == (LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic).Value;
                                                if (current_label != null)
                                                    current_label.Content = $"{codified_status.Value} {codified_status.Unit}";

                                                //Field safety analisis window -> Tracker current POS
                                                if ((LINK_TO_GRAPHIC_TCU)codified_status.Link_to_graphic == LINK_TO_GRAPHIC_TCU.TRACKER_POS_EL)
                                                {
                                                    FieldSafetyAnalisisWindow? safety_analisis_window = Application.Current.Windows.OfType<FieldSafetyAnalisisWindow>().FirstOrDefault();
                                                    if (safety_analisis_window != null)
                                                        safety_analisis_window.ShowValue(LINK_TO_AVG.INC_TCU_SLOPE_LAT_AVG_SBPT, codified_status.Value);
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Timer_refresh_scada_Tick)} -> TCU -> {ex.Message}");
                                    }
                                }
                            }
                        });
                    }

                    else if (Radio_tcu_mode.IsChecked == true)
                        Listview_tcu_codified_status.Items.Refresh();
                });
            }
        }

        #endregion



        #region Timer reconnect slave

        private void Timer_reconnect_slave_Tick(object sender, EventArgs e)
        {
            m_timer_reconnect_slave.Stop();
            if (m_queue_reconnect_slave.Count != 0)
            {
                if (m_list_read_write_state.All(state => state == false))
                    m_queue_reconnect_slave.Clear();

                else
                {
                    string slave = m_queue_reconnect_slave.Dequeue();
                    Manage_logs.SaveCommunicationValue($"TIMER RECONNECT SLAVE -> {slave}");

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
            if (b_record && m_list_read_write_state[(int)READ_WRITE_STATE.read_scs_modbus] && m_list_read_write_state[(int)READ_WRITE_STATE.first_read_scs_finish])
            {
                try
                {
                    string s_dir = $"{AppDomain.CurrentDomain.BaseDirectory}{Constants.Record_dir}\\{string.Format("{0:0000}", DateTime.Now.Year)}{string.Format("{0:00}", DateTime.Now.Month)}";
                    if (!Directory.Exists(s_dir))
                        Directory.CreateDirectory(s_dir);

                    s_file = $"{s_dir}\\{s_file}{DateTime.Now.Year:0000}{DateTime.Now.Month:00}{DateTime.Now.Day:00}.csv";
                    if (!File.Exists(s_file))
                    {
                        using FileStream fs = File.Create(s_file);
                        fs.Close();

                        //Date head
                        string s_head = $"UTCDATE{Globals.GetTheInstance().SField_sep}";
                        s_head += $"DDATE{Globals.GetTheInstance().SField_sep}";

                        //Modbus var head
                        Globals.GetTheInstance().List_slave_entry
                            .ForEach(slave_entry => slave_entry.List_var_entry
                            .ForEach(var_entry =>
                            {
                                bool save = var_entry.SCS_record && (!fast_mode || var_entry.Fast_mode_record);
                                if (save)
                                    s_head += $"{var_entry.Name}[{var_entry.Unit}]{Globals.GetTheInstance().SField_sep}";
                            })
                        );


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
                    Globals.GetTheInstance().List_slave_entry
                        .ForEach(slave_entry => slave_entry.List_var_entry
                        .ForEach(var_entry =>
                        {
                            bool save = var_entry.SCS_record && (!fast_mode || var_entry.Fast_mode_record);
                            if (save)
                            {
                                s_line += slave_entry.Connected ? var_entry.Value : Constants.Error_code;
                                s_line += Globals.GetTheInstance().SField_sep;
                            }
                        })
                    );


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
            if (b_record && m_list_read_write_state[(int)READ_WRITE_STATE.read_write_tcu_modbus] && m_list_read_write_state[(int)READ_WRITE_STATE.first_read_tcu_finish])
            {
                try
                {
                    string s_dir = $"{AppDomain.CurrentDomain.BaseDirectory}{Constants.Record_dir}\\{string.Format("{0:0000}", DateTime.Now.Year)}{string.Format("{0:00}", DateTime.Now.Month)}";
                    if (!Directory.Exists(s_dir))
                        Directory.CreateDirectory(s_dir);

                    s_file = $"{s_dir}\\{s_file}{DateTime.Now.Year:0000}{DateTime.Now.Month:00}{DateTime.Now.Day:00}.csv";
                    if (!File.Exists(s_file))
                    {
                        using FileStream fs = File.Create(s_file);
                        fs.Close();

                        string s_head = $"UTCDATE{Globals.GetTheInstance().SField_sep}";
                        s_head += $"DDATE{Globals.GetTheInstance().SField_sep}";

                        Globals.GetTheInstance().List_tcu_codified_status.ToList().ForEach(codified_status =>
                        {
                            if (codified_status.TCU_record)
                                s_head += $"{codified_status.Name}[{codified_status.Unit}]{Globals.GetTheInstance().SField_sep}";
                        });

                        s_head = s_head.Remove(s_head.Length - 1);

                        using StreamWriter stream_writer_head = new(s_file);
                        stream_writer_head.WriteLine(s_head);
                    }

                    double d_value = DateTime.UtcNow.ToOADate();

                    //Fecha formato UTC
                    string s_line = $"{DateTime.UtcNow.ToString(Globals.GetTheInstance().Date_format, new CultureInfo(Globals.GetTheInstance().Format_provider))}{Globals.GetTheInstance().SField_sep}";
                    s_line += $"{DateTime.UtcNow.ToOADate().ToString(Globals.GetTheInstance().nfi)}{Globals.GetTheInstance().SField_sep}";

                    Globals.GetTheInstance().List_tcu_codified_status.ForEach(codified_status =>
                    {
                        if (codified_status.TCU_record)
                            s_line += $"{codified_status.Value}{Globals.GetTheInstance().SField_sep}";
                    });

                    s_line = s_line.Remove(s_line.Length - 1);

                    using StreamWriter stream_writer_tcu = new(s_file, true);
                    stream_writer_tcu.WriteLine(s_line);
                }
                catch (Exception ex)
                {
                    Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Record_tcu)} -> {ex.Message}");
                }
            }

            else
                Manage_logs.SaveDepurValue($"RECORD TCU DISABLED - RW TCU STATE : { m_list_read_write_state[(int)READ_WRITE_STATE.read_write_tcu_modbus]} - RW FIRST TCU STATE : {m_list_read_write_state[(int)READ_WRITE_STATE.first_read_tcu_finish]}");
        }



        private void Timer_record_samca_Tick(object sender, EventArgs e)
        {
            Record_samca(Constants.Record_samca);
        }

        private void Record_samca(string s_file)
        {
            if (b_record && m_list_read_write_state[(int)READ_WRITE_STATE.read_write_samca_modbus] && m_list_read_write_state[(int)READ_WRITE_STATE.first_read_samca_finish])
            {
                try
                {
                    string s_dir = $"{AppDomain.CurrentDomain.BaseDirectory}{Constants.Record_dir}\\{string.Format("{0:0000}", DateTime.Now.Year)}{string.Format("{0:00}", DateTime.Now.Month)}"; 
                    if (!Directory.Exists(s_dir))
                        Directory.CreateDirectory(s_dir);

                    s_file = $"{s_dir}\\{s_file}{DateTime.Now.Year:0000}{DateTime.Now.Month:00}{DateTime.Now.Day:00}.csv";
                    if (!File.Exists(s_file))
                    {
                        using FileStream fs = File.Create(s_file);
                        fs.Close();

                        string s_head = $"UTCDATE{Globals.GetTheInstance().SField_sep}";
                        s_head += $"DDATE{Globals.GetTheInstance().SField_sep}";

                        //Modbus var head
                        Globals.GetTheInstance().List_slave_entry
                            .ForEach(slave_entry => slave_entry.List_var_entry
                            .ForEach(var_entry =>
                            {
                                if (var_entry.SAMCA_record)
                                    s_head += $"{var_entry.Name}[{var_entry.Unit}]{Globals.GetTheInstance().SField_sep}";
                            })
                        );

                        s_head = s_head.Remove(s_head.Length - 1);

                        using StreamWriter stream_writer_head = new(s_file);
                        stream_writer_head.WriteLine(s_head);
                    }

                    double d_value = DateTime.UtcNow.ToOADate();

                    //Fecha formato UTC
                    string s_line = $"{DateTime.UtcNow.ToString(Globals.GetTheInstance().Date_format, new CultureInfo(Globals.GetTheInstance().Format_provider))}{Globals.GetTheInstance().SField_sep}";
                    s_line += $"{DateTime.UtcNow.ToOADate().ToString(Globals.GetTheInstance().nfi)}{Globals.GetTheInstance().SField_sep}";

                    Globals.GetTheInstance().List_slave_entry
                        .ForEach(slave_entry => slave_entry.List_var_entry
                        .ForEach(var_entry =>
                        {
                            if (var_entry.SAMCA_record)
                            {
                                s_line += slave_entry.Connected ? var_entry.Value : Constants.Error_code;
                                s_line += Globals.GetTheInstance().SField_sep;
                            }
                        })
                    );

                    s_line = s_line.Remove(s_line.Length - 1);

                    using StreamWriter stream_writer_samca = new(s_file, true);
                    stream_writer_samca.WriteLine(s_line);
                }
                catch (Exception ex)
                {
                    Manage_logs.SaveErrorValue($"{GetType().Name} -> {nameof(Record_samca)} -> {ex.Message}");
                }
            }
        }

        #endregion

        #endregion


        #region Mail

        #region Redefine send mail instant

        private void Send_mail_instant_start()
        {
            Tuple<bool, double> tuple_send_mail = Functions.Redefine_send_mail_instant();
            if (tuple_send_mail.Item1)
            {
                m_mail_state = MAIL_STATE.COMPRESS;
                m_timer_send_mail.Interval = tuple_send_mail.Item2;
                m_timer_send_mail.Start();
            }
            else
                MessageBox.Show("Error redefining send mail instant", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);

        }

        #endregion

        #region Timer send mail

        private void Timer_send_mail_Tick(object sender, EventArgs e)
        {
            if (Globals.GetTheInstance().Mail_data_on == BIT_STATE.ON)
            {
                List<string> list_record_file = new() { Constants.Record_scs1, Constants.Record_scs2, Constants.Record_samca, Constants.Record_tcu };

                switch (m_mail_state)
                {
                    case MAIL_STATE.COMPRESS:
                        {
                            try
                            {
                                m_timer_send_mail.Stop();

                                string s_dir = AppDomain.CurrentDomain.BaseDirectory + Constants.Record_dir + @"\" + String.Format("{0:0000}", DateTime.Now.Year) + String.Format("{0:00}", DateTime.Now.Month);

                                string s_log = "COMPRESS FILES OK -> ";

                                list_record_file.ForEach(record_file =>
                                {
                                    string s_file = s_dir + @"\" + record_file + DateTime.Now.Year.ToString("0000") + DateTime.Now.Month.ToString("00") + DateTime.Now.AddDays(-1).Day.ToString("00") + ".csv";

                                    if (File.Exists(s_file))
                                    {
                                        string s_zip_file = Path.GetFileNameWithoutExtension(s_file) + Constants.Compress_extension;
                                        string path_zip_file = Constants.Compress_dir + @"\" + s_zip_file;

                                        if (File.Exists(path_zip_file))
                                            File.Delete(path_zip_file);

                                        using (ZipFile zip = new())
                                        {
                                            //zip.Password = "SBPTRACKER";
                                            zip.AddFile(s_file, Path.GetFileNameWithoutExtension(s_file));
                                            zip.Save(path_zip_file);
                                        }

                                        s_log += Path.GetFileName(s_file) + ";";
                                    }
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

                            Tuple<bool, List<string>> tuple_send_mail = Functions.Send_record_mail(list_record_file);
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
            Stop_timers();
            m_setting_option = SETTING_OPTION.APP;
            m_timer_setting.Start();
        }

        #endregion

        #region Setting advanced
        private void Button_setting_advanced_Click(object sender, RoutedEventArgs e)
        {
            if (m_list_read_write_state.Any(state => state == true))
                Checkbox_start.IsChecked = false;

            if (Globals.GetTheInstance().Enable_web_api == BIT_STATE.ON)
                Globals.GetTheInstance().ManageWebAPI.m_API_data_started = false;

            Stop_timers();
            m_setting_option = SETTING_OPTION.ADVANCED;
            m_timer_setting.Start();
        }

        #endregion

        #region Setting mail

        private void Button_setting_mail_Click(object sender, RoutedEventArgs e)
        {
            Stop_timers();
            m_setting_option = SETTING_OPTION.MAIL;
            m_timer_setting.Start();
        }

        #endregion

        #region Setting modbus

        private void Button_setting_modbus_Click(object sender, RoutedEventArgs e)
        {
            Stop_timers();
            m_setting_option = SETTING_OPTION.MODBUS;
            m_timer_setting.Start();
        }

        #endregion


        #region Timer setting

        private void Stop_timers()
        {

            if (m_list_read_write_state.Any(state => state == true))
                Checkbox_start.IsChecked = false;

            m_timer_refresh_scada.Stop();
            m_timer_reconnect_slave.Stop();

            m_timer_read_scs_normal_modbus.Stop();
            m_timer_read_scs_fast_modbus.Stop();
            m_timer_read_tcu_modbus.Stop();

            m_timer_write_tcu_command_modbus.Stop();
            m_timer_write_tcu_datetime_modbus.Stop();
            m_timer_write_samca_modbus.Stop();

            m_timer_record_scs_normal.Stop();
            m_timer_record_scs_fast.Stop();
            m_timer_record_tcu.Stop();
            m_timer_record_samca.Stop();
        }

        private void Start_timer()
        {
            m_timer_refresh_scada.Start();

            m_timer_read_scs_normal_modbus.Start();
            m_timer_read_scs_fast_modbus.Start();
            m_timer_read_tcu_modbus.Start();

            m_timer_write_tcu_command_modbus.Start();
            m_timer_write_tcu_datetime_modbus.Start();
            m_timer_write_samca_modbus.Start();

            m_timer_record_scs_normal.Start();
            m_timer_record_scs_fast.Start();
            m_timer_record_tcu.Start();
        }

        private void Redefine_timer_interval()
        {
            m_timer_reconnect_slave.Interval = Globals.GetTheInstance().Modbus_reconnect_interval;

            m_timer_read_scs_normal_modbus.Interval = Globals.GetTheInstance().Modbus_read_scs_normal_interval;
            m_timer_read_scs_fast_modbus.Interval = Globals.GetTheInstance().Modbus_read_scs_fast_interval;
            m_timer_read_tcu_modbus.Interval = Globals.GetTheInstance().Modbus_read_tcu_interval;

            m_timer_write_tcu_command_modbus.Interval = Globals.GetTheInstance().Modbus_write_tcu_watchdog_interval;
            m_timer_write_tcu_datetime_modbus.Interval = Globals.GetTheInstance().Modbus_write_tcu_datetime_interval;
            m_timer_write_samca_modbus.Interval = Globals.GetTheInstance().Modbus_write_samca_interval;

            m_timer_record_scs_normal.Interval = Globals.GetTheInstance().Record_scs_normal_interval;
            m_timer_record_scs_fast.Interval = Globals.GetTheInstance().Record_scs_fast_interval;
            m_timer_record_tcu.Interval = Globals.GetTheInstance().Record_tcu_interval;
            m_timer_record_samca.Interval = Globals.GetTheInstance().Record_samca_interval;
        }

        private void Timer_setting_Tick(object sender, EventArgs e)
        {
            m_timer_setting.Stop();

            Dispatcher.Invoke(() =>
            {
                switch (m_setting_option)
                {
                    case SETTING_OPTION.MODBUS:
                        {
                            SettingModbusWindow setting_modbus_window = new()
                            {
                                Left = this.Left,
                                Top = this.Top,
                            };
                            setting_modbus_window.ShowDialog();

                            Load_modbus_slave_var();

                            break;
                        }

                    case SETTING_OPTION.APP:
                        {
                            SettingAppWindow settingAppWindow = new()
                            {
                                Left = this.Left,
                                Top = this.Top,
                            };
                            settingAppWindow.ShowDialog();

                            Globals.GetTheInstance().nfi.NumberDecimalSeparator = Globals.GetTheInstance().Decimal_sep == DECIMAL_SEP.PUNTO ? "." : ",";
                            this.Height = Globals.GetTheInstance().Depur_enable == BIT_STATE.ON ? Constants.depur_enable_height : Constants.depur_disable_height;

                            StartAvgValues();

                            Redefine_timer_interval();

                            break;
                        }

                    case SETTING_OPTION.ADVANCED:
                        {

                            SettingAdvancedWindow settingAdvancedWindow = new()
                            {
                                Left = this.Left,
                                Top = this.Top,
                            };
                            settingAdvancedWindow.ShowDialog();

                            if (Globals.GetTheInstance().Enable_web_api == BIT_STATE.ON)
                            {
                                Globals.GetTheInstance().ManageWebAPI.m_API_data_started = true;
                                Globals.GetTheInstance().ManageWebAPI.Start_timer_modbus_API_state(BIT_STATE.ON, (int)Globals.GetTheInstance().Send_state_interval_web_API);
                                Globals.GetTheInstance().ManageWebAPI.Start_timer_modbus_API_data(BIT_STATE.ON, (int)Globals.GetTheInstance().Send_data_interval_web_API);
                            }

                            break;
                        }

                    case SETTING_OPTION.MAIL:
                        {
                            Process? process = Process.GetProcesses().ToList().FirstOrDefault(process => process.ProcessName == "python" && process.MainModule.FileName == Globals.GetTheInstance().Python_path);
                            if (process != null)
                                process.Kill();


                            SettingMailWindow settingMailWindow = new()
                            {
                                Left = this.Left,
                                Top = this.Top,
                            };
                            settingMailWindow.ShowDialog();

                            m_timer_send_mail.Stop();
                            if (Globals.GetTheInstance().Mail_instant != "__:__")
                                Send_mail_instant_start();

                            Functions.Cloud_upload_start(Globals.GetTheInstance().Cloud_check_interval, 1);

                            break;
                        }
                }

                Start_timer();

            });
        }

        #endregion

        #endregion



        #region Exit

        private void Button_exit_Click(object sender, RoutedEventArgs e)
        {
            Process? process = Process.GetProcesses().ToList().FirstOrDefault(process => process.ProcessName == "python" && process.MainModule.FileName == Globals.GetTheInstance().Python_path);
            if (process != null)
                process.Kill();

            Application.Current.Shutdown();
        }

        #endregion


    }
}
