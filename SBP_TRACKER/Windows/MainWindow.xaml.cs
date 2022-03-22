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
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
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

        private System.Timers.Timer m_timer_read_met_modbus;
        private System.Timers.Timer m_timer_read_tcu_modbus;
        private System.Timers.Timer m_timer_write_tcu_modbus;

        private System.Timers.Timer m_timer_record_data_met1;
        private System.Timers.Timer m_timer_record_data_met2;
        private System.Timers.Timer m_timer_record_data_tcu;
        private System.Timers.Timer m_timer_send_mail;

        #endregion

        private double[] m_scs_meteo_station_values = new double[Enum.GetNames(typeof(EMET_WATCHDOG_ASSOC)).Length];
        private int m_counter_scs = 0;

        private bool b_unity_start_stop;
        private bool b_first_read_met_finish;
        private bool b_first_read_tcu_finish;
        private bool b_read_met_modbus;
        private bool b_read_tcu_modbus;
        private bool b_write_tcu_modbus;
        private bool b_record = true;
        private MAIL_STATE m_mail_state;

        private List<Manage_thread> m_list_start_thread = new List<Manage_thread>();
        private ObservableCollection<TCPModbusSlaveEntry> m_collection_modbus_slave;
        private ObservableCollection<TCPModbusVarEntry> m_collection_modbus_var;
        private TCPModbusSlaveEntry? m_selected_slave_entry = null;
        private TCUCommand? m_selected_command = null;

        private Queue<List<int>> m_queue_commands = new();

        #region Array controles

        private List<string> m_list_var_map_schema = new();
        private List<Label> m_array_label_schema_title = new();
        private List<Label> m_array_label_schema_value = new();
        private List<Image> m_array_image_schema_value = new();

        private List<Label> m_array_label_emet_watchdog_value = new();
        private List<Label> m_array_label_emet_watchdog_unit= new();

        private List<Label> m_array_label_tcu_command_param = new();
        private List<DecimalUpDown> m_array_decimal_tcu_command_param = new();

        #endregion



        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            Title = "SBP TRACKER / ver " + Constants.version;
            Label_ver.Content = "SBP TRACKER / ver " + Constants.version;


            #region Array controles

            m_array_label_schema_title = new List<Label>
            {
                Label_sick_inc5_title,
                Label_sick_inc4_title,
                Label_sick_inc3_title,
                Label_dyn1_title,
                Label_sick_inc1_title,
                Label_temp_probe1_title,
                Label_temp_probe2_title,
                Label_tracker_irr1_title,
                Label_dyn3_title,
                Label_dyn2_title,
                Label_sick_inc0_title,
                Label_sick_inc2_title,
                Label_temp_probe3_title,
                Label_temp_probe4_title,
                Label_wd1_title,
                Label_anenomether1_title,
                Label_anenomether2_title,
                Label_fixed_irr2_title
            };

            m_array_label_schema_title.ForEach(label_schema =>
            {
                label_schema.PreviewMouseDown += new MouseButtonEventHandler(LabelSchema_PreviewMouseDown_EventHandler);
                label_schema.PreviewDragOver += new DragEventHandler(LabelSchema_PreviewDragOver_EventHandler);
                label_schema.PreviewDrop += new DragEventHandler(LabelSchema_PreviewDrop_EventHandler);
            });

            m_array_label_schema_value = new List<Label>
            {
                Label_sick_inc5_value,
                Label_sick_inc4_value,
                Label_sick_inc3_value,
                Label_dyn1_value,
                Label_sick_inc1_value,
                Label_temp_probe1_value,
                Label_temp_probe2_value,
                Label_tracker_irr1_value,
                Label_dyn3_value,
                Label_dyn2_value,
                Label_sick_inc0_value,
                Label_sick_inc2_value,
                Label_temp_probe3_value,
                Label_temp_probe4_value,
                Label_wd1_value,
                Label_anenomether1_value,
                Label_anenomether2_value,
                Label_fixed_irr2_value
            };

            m_array_image_schema_value = new List<Image>
            {
                Image_sick_inc5,
                Image_sick_inc4,
                Image_sick_inc3,
                Image_dyn1,
                Image_sick_inc1,
                Image_temp_probe1,
                Image_temp_probe2,
                Image_tracker_irr1,
                Image_dyn3,
                Image_dyn2,
                Image_sick_inc0,
                Image_sick_inc2,
                Image_temp_probe3,
                Image_temp_probe4,
                Image_wd1,
                Image_anenomether1,
                Image_anenomether2,
                Image_fixed_irr2
            };
            m_array_image_schema_value.ForEach(image_schema =>
            {
                image_schema.MouseEnter += new MouseEventHandler(ImageSchema_MouseEnter_EventHandler);
                image_schema.MouseLeave += new MouseEventHandler(ImageSchema_MouseLeave_EventHandler);
            });


            m_array_label_emet_watchdog_value = new List<Label>
            {
                Label_wd_counter_scs_value,
                Label_wind_speed_value,
                Label_wind_direction_value,
                Label_temp_value,
                Label_radiation_value,
                Label_safety_supervisor_value
            };

            m_array_label_emet_watchdog_unit = new List<Label>
            {
                Label_wd_counter_scs_unit,
                Label_wind_speed_unit,
                Label_wind_direction_unit,
                Label_temp_unit,
                Label_radiation_unit,
                Label_safety_supervisor_unit
            };

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
            bool load_modbus_ok = Load_modbus_slave_var();
            bool load_tcu_codified_status = Manage_file.Load_tcu_codified_status();
            bool load_commands = Manage_file.Load_modbus_commands();

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


            Globals.GetTheInstance().nfi = new NumberFormatInfo
            {
                NumberDecimalSeparator = Globals.GetTheInstance().Decimal_sep == DECIMAL_SEP.PUNTO ? "." : ","
            };

            ToggleButton_tcu_control.IsChecked = false;
            this.Height = Globals.GetTheInstance().Depur_enable == BIT_STATE.ON ? Constants.depur_enable_height : Constants.depur_disable_height;

            #region Timer

            m_timer_start_tcp = new System.Timers.Timer();
            m_timer_start_tcp.Elapsed += Timer_start_tcp_Tick;
            m_timer_start_tcp.Interval = 1000;
            m_timer_start_tcp.Stop();

            b_read_met_modbus = false;

            m_timer_read_met_modbus = new System.Timers.Timer();
            m_timer_read_met_modbus.Elapsed += Timer_read_met_modbus_Tick;
            m_timer_read_met_modbus.Interval = Globals.GetTheInstance().Modbus_read_met_interval;
            m_timer_read_met_modbus.Start();

            b_read_tcu_modbus = false;

            m_timer_read_tcu_modbus = new System.Timers.Timer();
            m_timer_read_tcu_modbus.Elapsed += Timer_read_tcu_modbus_Tick;
            m_timer_read_tcu_modbus.Interval = Globals.GetTheInstance().Modbus_read_tcu_interval;
            m_timer_read_tcu_modbus.Start();

            b_write_tcu_modbus = false;

            m_timer_write_tcu_modbus = new System.Timers.Timer();
            m_timer_write_tcu_modbus.Elapsed += Timer_write_tcu_modbus_Tick;
            m_timer_write_tcu_modbus.Interval = Globals.GetTheInstance().Modbus_write_tcu_interval;
            m_timer_write_tcu_modbus.Start();

            m_timer_record_data_met1 = new System.Timers.Timer();
            m_timer_record_data_met1.Elapsed += Timer_record_met1_data_Tick;
            m_timer_record_data_met1.Interval = Globals.GetTheInstance().Record_data_met1_interval;
            m_timer_record_data_met1.Start();

            m_timer_record_data_met2 = new System.Timers.Timer();
            m_timer_record_data_met2.Elapsed += Timer_record_met2_data_Tick;
            m_timer_record_data_met2.Interval = Globals.GetTheInstance().Record_data_met2_interval;
            m_timer_record_data_met2.Start();

            m_timer_record_data_tcu = new System.Timers.Timer();
            m_timer_record_data_tcu.Elapsed += Timer_record_tcu_data_Tick;
            m_timer_record_data_tcu.Interval = Globals.GetTheInstance().Record_data_tcu_interval;
            m_timer_record_data_tcu.Start();


            m_timer_send_mail = new System.Timers.Timer();
            m_timer_send_mail.Elapsed += Timer_send_mail_Tick;
            m_timer_send_mail.Interval = 10000000;
            m_timer_send_mail.Stop();

            #endregion


            if (Globals.GetTheInstance().Mail_instant != "__:__")
                Send_mail_instant_start();
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
                            try
                            {
                                slave_entry.Connected = true;
                                if (slave_entry.TCU)
                                {
                                    b_read_tcu_modbus = true;
                                    b_write_tcu_modbus = true;
                                }

                                m_list_start_thread.RemoveAll(Manage_thread => Manage_thread.Slave_name == slave_entry.Name);
                                if (m_list_start_thread.Count == 0)
                                {
                                    if (Globals.GetTheInstance().List_modbus_slave_entry.Exists(entry => entry.Connected))
                                        b_read_met_modbus = true;
                                    else
                                        Dispatcher.Invoke(() => Checkbox_start.IsChecked = false);

                                    Dispatcher.Invoke(() => Checkbox_start.IsChecked = b_read_met_modbus);
                                    Dispatcher.Invoke(() => Border_wait.Visibility = Visibility.Collapsed);
                                }

                                Dispatcher.Invoke(() => Listview_slave_modbus.Items.Refresh());
                            }
                            catch (Exception ex)
                            {
                                Manage_logs.SaveErrorValue(GetType().Name + " ->  " + nameof(TCP_events_to_main) + " -> CONNECT -> " + ex.Message.ToString());
                            }
                        }

                        break;
                    }

                case TCP_ACTION.DISCONNECT:
                    {
                        Manage_logs.SaveLogValue("Disconnected modbus slave -> " + args.Slave_name);

                        TCPModbusSlaveEntry slave_entry = Globals.GetTheInstance().List_modbus_slave_entry.FirstOrDefault(slave_entry => slave_entry.Name == args.Slave_name);
                        if (slave_entry != null)
                        {
                            try
                            {
                                slave_entry.Connected = false;

                                if (slave_entry.TCU)
                                {
                                    b_read_tcu_modbus = false;
                                    b_write_tcu_modbus = false;

                                    Globals.GetTheInstance().List_tcu_codified_status.ForEach(modbus_var => modbus_var.Value = string.Empty);
                                    Listview_tcu_codified_status.Items.Refresh();
                                }

                                else
                                {
                                    //Modbus var list
                                    m_collection_modbus_var.ToList()
                                        .Where(modbus_var => modbus_var.Slave == slave_entry.Name).ToList()
                                        .ForEach(modbus_var => modbus_var.Value = string.Empty);

                                    Dispatcher.Invoke(() => Listview_read_modbus.Items.Refresh());


                                    slave_entry.List_modbus_var.ForEach(modbus_var =>
                                    {
                                    //Meteo station values -> EMET WATHCDOG
                                    if (modbus_var.Watchdog_assoc != (int)EMET_WATCHDOG_ASSOC.NONE)
                                            m_scs_meteo_station_values[modbus_var.Watchdog_assoc] = 0;

                                    //Schema
                                    m_array_label_schema_title
                                            .Select((item, index) => new ControlLinq { Value = item.Content.ToString(), Position = index }).ToList()
                                            .ForEach(control =>
                                            {
                                                if (control.Value.Equals(modbus_var.Name))
                                                    Dispatcher.Invoke(() => m_array_label_schema_value[control.Position].Content = string.Empty);
                                            });
                                    });
                                }


                                if (Globals.GetTheInstance().List_modbus_slave_entry.All(entry => !entry.Connected))
                                {
                                    b_read_met_modbus = false;
                                    Checkbox_start.IsChecked = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                Manage_logs.SaveErrorValue(GetType().Name + " ->  " + nameof(TCP_events_to_main) + " -> DISCONNECT -> " + ex.Message.ToString());
                            }
                        }

                        Dispatcher.Invoke(() => Listview_slave_modbus.Items.Refresh());

                        break;
                    }

                case TCP_ACTION.READ:
                    {
                        break;
                    }

                case TCP_ACTION.ERROR_CONNECT:
                    {
                        Manage_logs.SaveLogValue("Error connection modbus slave -> " + args.Slave_name);

                        TCPModbusSlaveEntry slave_entry = Globals.GetTheInstance().List_modbus_slave_entry.FirstOrDefault(entry => entry.Name == args.Slave_name);

                        if (slave_entry != null)
                        {
                            try
                            {
                                m_list_start_thread.RemoveAll(Manage_thread => Manage_thread.Slave_name == slave_entry.Name);
                                if (m_list_start_thread.Count == 0)
                                {
                                    if (Globals.GetTheInstance().List_modbus_slave_entry.Exists(entry => entry.Connected))
                                        b_read_met_modbus = true;
                                    else
                                        Dispatcher.Invoke(() => Checkbox_start.IsChecked = false);

                                    Dispatcher.Invoke(() => Checkbox_start.IsChecked = b_read_met_modbus);
                                    Dispatcher.Invoke(() => Border_wait.Visibility = Visibility.Collapsed);
                                }

                                Dispatcher.Invoke(() => Listview_slave_modbus.Items.Refresh());
                            }
                            catch (Exception ex)
                            {
                                Manage_logs.SaveErrorValue(GetType().Name + " ->  " + nameof(TCP_events_to_main) + " -> ERROR_CONNECT -> " + ex.Message.ToString());
                            }
                        }

                        break;
                    }

                case TCP_ACTION.ERROR_READ:
                    {
                        Manage_logs.SaveLogValue("Error read modbus slave -> " + args.Slave_name);

                        Manage_thread? manageThread = Globals.GetTheInstance().List_manage_thread.FirstOrDefault(manage_thread => manage_thread.Slave_name == args.Slave_name);
                        if (manageThread != null)
                        {
                            manageThread.ManageTCP.Disconnect();
                            Dispatcher.Invoke(() => Listview_slave_modbus.Items.Refresh());
                        }

                        break;
                    }
            }
        }

        #endregion



        #region Show mode -> switch events

        private void SwitchSliderShowMode_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((bool)ToggleButton_tcu_control.IsChecked == false)
            {
                Grid_table.Visibility = SwitchSliderShowMode.ToggledState ? Visibility.Collapsed : Visibility.Visible;
                Dockpanel_schema.Visibility = SwitchSliderShowMode.ToggledState ? Visibility.Visible : Visibility.Collapsed;
                Select_slave_var(null);
            }
        }

        private void SwitchSliderShowMode_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((bool)ToggleButton_tcu_control.IsChecked == false)
            {
                Grid_table.Visibility = SwitchSliderShowMode.ToggledState ? Visibility.Collapsed : Visibility.Visible;
                Dockpanel_schema.Visibility = SwitchSliderShowMode.ToggledState ? Visibility.Visible : Visibility.Collapsed;
                Select_slave_var(null);
            }
        }

        #endregion



        #region Schema mode

        #region Save schema map

        private void Button_save_schema_map_Click(object sender, RoutedEventArgs e)
        {
            Globals.GetTheInstance().List_modbus_slave_entry.ForEach(entry =>
            {
                entry.List_modbus_var.ForEach(modbus_var =>
                {
                    ControlLinq? control_linq = m_array_label_schema_title
                    .Select((item, index) => new ControlLinq { Value = item.Content.ToString(), Position = index })
                    .FirstOrDefault(control => control.Value.Equals(modbus_var.Name));

                    modbus_var.Schema_pos = control_linq != null ? control_linq.Position : Constants.index_no_selected;

                });
            });

            bool save_ok = Manage_file.Save_var_map_entries();
            if (!save_ok)
                MessageBox.Show("Error saving new config.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);

            else
                MessageBox.Show("Schema saved.", "INFO", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
        }
        #endregion

        #region Listview schema drag and drop

        private void Listview_schema_var_map_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                if (Listview_schema_var_map.SelectedItems.Count > 0)
                    if (Listview_schema_var_map.SelectedItem is string mySelectedItem)
                    {
                        DragDrop.DoDragDrop(Listview_schema_var_map, mySelectedItem, DragDropEffects.Move);
                    }
        }

        private void Listview_schema_var_map_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }

        private void Listview_schema_var_map_PreviewDrop(object sender, DragEventArgs e)
        {
            if (m_label_drag != null)
                m_label_drag.Content = string.Empty;

            e.Handled = true;
        }

        #endregion


        #region Label schema Drag & drop

        Label m_label_drag;

        private void LabelSchema_PreviewMouseDown_EventHandler(object sender, MouseButtonEventArgs e)
        {
            m_label_drag = sender as Label;
            DragDrop.DoDragDrop(m_label_drag, m_label_drag.Content, DragDropEffects.Move);
        }

        private void LabelSchema_PreviewDragOver_EventHandler(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }

        private void LabelSchema_PreviewDrop_EventHandler(object sender, DragEventArgs e)
        {
            if (b_read_met_modbus)
                MessageBox.Show("Stop comunications before shift modbus vars", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            else
            {
                Label label = sender as Label;
                string s_content = e.Data.GetData(typeof(string)) as string;


                //Se mueve desde un textbox a otro
                if (m_label_drag != null)
                {
                    m_label_drag.Content = String.Empty;
                    m_label_drag = null;
                }

                if (m_array_label_schema_title.Exists(label => label.Content.ToString().Equals(s_content)))
                    MessageBox.Show("Duplicate var in other box", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                else
                    label.Content = s_content;
            }

            e.Handled = true;
        }

        #endregion


        #region Image popup

        private void ImageSchema_MouseEnter_EventHandler(object sender, MouseEventArgs e)
        {
            Image image = sender as Image;
            int control_index = m_array_image_schema_value.IndexOf(image);
            Globals.GetTheInstance().List_modbus_slave_entry.ForEach(entry =>
            {

                TCPModbusVarEntry var_entry = entry.List_modbus_var.FirstOrDefault(modbus_var => modbus_var.Schema_pos == control_index);
                if (var_entry != null)
                    Label_popup.Content = var_entry.Description;
            });

            if (Label_popup.Content != null)
            {
                PopupVarMap.PlacementTarget = image;
                PopupVarMap.Placement = PlacementMode.MousePoint;
                PopupVarMap.IsOpen = true;
            }
        }

        private void ImageSchema_MouseLeave_EventHandler(object sender, MouseEventArgs e)
        {
            Image image = sender as Image;
            Label_popup.Content = null;
            PopupVarMap.IsOpen = false;
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

        #region CHECK - UNCHECK

        private void ToggleButton_tcu_control_Checked(object sender, RoutedEventArgs e)
        {
            if (Grid_table != null)
            {
                Grid_table.Visibility = Visibility.Collapsed;
                Dockpanel_schema.Visibility = Visibility.Collapsed;
                Grid_tcu_control.Visibility = Visibility.Visible;
            }
        }
        private void ToggleButton_tcu_control_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Grid_table != null)
            {
                SwitchSliderShowMode.IniState();
                Grid_table.Visibility = Visibility.Visible;
                Dockpanel_schema.Visibility = Visibility.Collapsed;
                Grid_tcu_control.Visibility = Visibility.Collapsed;
            }
        }

        #endregion


        #region Codif status actions

        private void Listview_codif_state_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as ListView).SelectedItem;
            if (item != null)
            {
                TCUCodifiedStatusEntry selected_tcu_encode = item as TCUCodifiedStatusEntry;
                TCPModbusSlaveEntry? modbus_slave_entry = m_collection_modbus_slave.FirstOrDefault(modbus_slave => modbus_slave.TCU);

                CodifiedStatusWindow codified_window = new();
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
            CodifiedStatusWindow codified_window = new();

            codified_window.TCU_codified_status_entry = new() {  SCS_record = false, Status_mask_enable = false, List_status_mask = new List<string>() };
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
                            Margin = new Thickness(0, 5, 0, 5),
                            Height = 30,
                        };


                        Label label_tcu_command = new()
                        {
                            Style = Application.Current.Resources["Label_setting"] as Style,
                            Width = 170,
                            Content = m_selected_command.Name_params[index].ToString()
                        };
                        wrap_param.Children.Add(label_tcu_command);

                        m_array_label_tcu_command_param.Add(label_tcu_command);


                        Tuple <decimal,decimal> tuple_min_max = Functions.TypeCode_min_max(m_selected_command.Type_params[index]);

                        decimal stepsize = m_selected_command.Type_params[index] == TypeCode.Single ? (decimal)0.1 : (decimal)1;

                        DecimalUpDown decimal_tcu_command = new DecimalUpDown
                        {
                            Style = Application.Current.Resources["DecimalUpDownStyle"] as Style,
                            DisplayLength = 6,
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
                if (m_selected_command.Num_params != 0) {
                    do {
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
                    m_list_var_map_schema.Clear();
                    m_array_label_emet_watchdog_unit.ForEach(label_emet => label_emet.Content = string.Empty);

                    Globals.GetTheInstance().List_modbus_slave_entry.ForEach(modbus_slave_entry =>
                    {
                        m_collection_modbus_slave.Add(modbus_slave_entry);
                        s_slave_var_log += modbus_slave_entry.Name + "\r\n" + "-----------------------" + "\r\n";

                        modbus_slave_entry.List_modbus_var.ForEach(modbus_slave_var =>
                        {
                            m_collection_modbus_var.Add(modbus_slave_var);

                            m_list_var_map_schema.Add(modbus_slave_var.Name);
                            s_slave_var_log += modbus_slave_var.Name + "\r\n";

                            if (modbus_slave_var.Schema_pos != Constants.index_no_selected)
                                m_array_label_schema_title[modbus_slave_var.Schema_pos].Content = modbus_slave_var.Name;

                            if (modbus_slave_var.Watchdog_assoc != (int)EMET_WATCHDOG_ASSOC.NONE)
                                m_array_label_emet_watchdog_unit[modbus_slave_var.Watchdog_assoc].Content = modbus_slave_var.Unit;
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

                    Listview_schema_var_map.ItemsSource = m_list_var_map_schema;
                    Listview_schema_var_map.Items.Refresh();

                    #endregion

                    #endregion


                    #region Start manage thread

                    Globals.GetTheInstance().List_manage_thread = new List<Manage_thread>();
                    Globals.GetTheInstance().List_modbus_slave_entry.ForEach(slave_entry =>
                    {
                        Manage_thread manage_thread = new(slave_entry.Name, slave_entry.IP_primary, slave_entry.Port, slave_entry.UnitId, slave_entry.Dir_ini, slave_entry.Read_bytes);
                        Globals.GetTheInstance().List_manage_thread.Add(manage_thread);
                    });

                    #endregion
                }
            }
            catch (Exception ex)
            {
                load_modbus_ok = false;
                Manage_logs.SaveErrorValue(GetType().Name + " ->  " + nameof(Load_modbus_slave_var) + " -> " + ex.Message.ToString());
            }

            return load_modbus_ok;
        }

        #endregion



        #region Start / Stop TCP modbus

        #region Checkbox start - stop

        private void Checkbox_start_Checked(object sender, RoutedEventArgs e)
        {
            if (!b_unity_start_stop)
            {
                m_list_start_thread.Clear();
                Globals.GetTheInstance().List_manage_thread.ForEach(manage_thread => m_list_start_thread.Add(manage_thread));

                Border_wait.Visibility = Visibility.Visible;
                m_timer_start_tcp.Start();
            }
            b_unity_start_stop = false;
        }


        private void Checkbox_start_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!b_unity_start_stop)
            {
                Globals.GetTheInstance().List_manage_thread.ForEach(manage => manage.Stop_tcp_com_thread());
            }
            b_unity_start_stop = false;
        }

        #endregion

        #region Start stop in gridview
        private void Gridview_state_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            TCPModbusSlaveEntry slaveEntry = (TCPModbusSlaveEntry)btn.DataContext;

            Manage_thread manageThread = Globals.GetTheInstance().List_manage_thread.First(manage_thread => manage_thread.Slave_name == slaveEntry.Name);
            if (!manageThread.ManageTCP.Is_connected())
            {
                b_unity_start_stop = true;
                m_list_start_thread.Clear();
                m_list_start_thread.Add(manageThread);
                Border_wait.Visibility = Visibility.Visible;
                m_timer_start_tcp.Start();
            }
            else
                manageThread.Stop_tcp_com_thread();
        }

        #endregion

        #region Timer start

        private void Timer_start_tcp_Tick(object sender, EventArgs e)
        {
            b_first_read_met_finish = false;
            b_first_read_tcu_finish = false;
            m_timer_start_tcp.Stop();
            m_list_start_thread.ForEach(manage => manage.Start_tcp_com());
        }

        #endregion

        #endregion



        #region Timer read MET modbus

        private void Timer_read_met_modbus_Tick(object sender, EventArgs e)
        {
            if (b_read_met_modbus)
                Read_met_modbus();
        }

        private void Read_met_modbus()
        {
            try
            {
                //Analizar si está filtrado un slave
                List<Manage_thread> list_read_thread = Globals.GetTheInstance().List_manage_thread;
                if (m_selected_slave_entry != null)
                    list_read_thread = list_read_thread.Where(manage_thread => manage_thread.Slave_name == m_selected_slave_entry.Name).ToList();


                list_read_thread.ForEach(manage_thread =>
                {
                    if (manage_thread.ManageTCP.Is_connected())
                    {
                        Tuple<bool, int[]> tuple_read = manage_thread.Read_holding_registers_int32();

                        if (tuple_read.Item1)
                        {
                            if (tuple_read.Item2.Length != 0)
                            {
                                List<TCPModbusVarEntry> list_var_entry = m_collection_modbus_var.Where(modbus_var => modbus_var.Slave == manage_thread.Slave_name).ToList();

                                list_var_entry.ForEach(var_entry =>
                                {
                                    int read_array_pos = var_entry.DirModbus - manage_thread.Dir_ini;
                                    if (read_array_pos >= 0)
                                    {
                                    //Table
                                    var_entry.Value = Functions.Read_from_array_convert_scale(tuple_read.Item2, read_array_pos, var_entry.TypeVar, var_entry.Scaled_range_min, var_entry.Scale_factor);

                                    //Schema
                                    Dispatcher.Invoke(() =>
                                            {
                                                ControlLinq? control_linq = m_array_label_schema_title
                                                .Select((item, index) => new ControlLinq { Value = item.Content.ToString(), Position = index })
                                                .FirstOrDefault(control => control.Value.Equals(var_entry.Name));
                                                if (control_linq != null)
                                                    m_array_label_schema_value[control_linq.Position].Content = var_entry.Value + " " + var_entry.Unit;
                                            });


                                    //Analizar si es una variable a enviar en el EMET WATCHDOG
                                    if (var_entry.Watchdog_assoc != (int)EMET_WATCHDOG_ASSOC.NONE)
                                            m_scs_meteo_station_values[var_entry.Watchdog_assoc] = Math.Round(float.Parse(var_entry.Value, Globals.GetTheInstance().nfi), 2);
                                    }
                                });
                            }
                        }
                        else
                        {
                            Manage_logs.SaveErrorValue("Error read image -> " + manage_thread.Slave_name + " / " + manage_thread.IP_address + " / " + manage_thread.Port + " / " + manage_thread.Dir_ini + " / " + manage_thread.UnitId);
                        }
                    }
                });

                b_first_read_met_finish = true;

                Dispatcher.Invoke(() => Listview_read_modbus.Items.Refresh());
            }
            catch (Exception ex)
            {
                Manage_logs.SaveErrorValue(GetType().Name + " ->  " + nameof(Read_met_modbus) + " -> " + ex.Message.ToString());
            }
        }

        #endregion


        #region Timer read TCU modbus

        private void Timer_read_tcu_modbus_Tick(object sender, EventArgs e)
        {
            if (b_read_tcu_modbus)
                Read_tcu_modbus();
        }

        private void Read_tcu_modbus()
        {
            TCPModbusSlaveEntry? slave_entry = m_collection_modbus_slave.FirstOrDefault(modbus_slave => modbus_slave.TCU);
            if (slave_entry != null)
            {
                try
                {
                    Manage_thread manage_thread = Globals.GetTheInstance().List_manage_thread.First(manage_thread => manage_thread.Slave_name == slave_entry.Name);
                    if (manage_thread.ManageTCP.Is_connected())
                    {
                        Tuple<bool, int[]> tuple_read = manage_thread.Read_holding_registers_int32();

                        if (tuple_read.Item1)
                        {
                            if (tuple_read.Item2.Length != 0)
                            {
                                Globals.GetTheInstance().List_tcu_codified_status.ForEach(encode_state =>
                                {
                                    int read_array_pos = encode_state.DirModbus - manage_thread.Dir_ini;
                                    if (read_array_pos >= 0)
                                        encode_state.Value = Functions.Read_from_array(tuple_read.Item2, read_array_pos, encode_state.TypeVar);
                                });
                            }
                        }
                        else
                        {
                            Manage_logs.SaveErrorValue("Error read image -> " + manage_thread.Slave_name + " / " + manage_thread.IP_address + " / " + manage_thread.Port + " / " + manage_thread.Dir_ini + " / " + manage_thread.UnitId);
                        }
                    }

                    b_first_read_tcu_finish = true;

                    Dispatcher.Invoke(() => Listview_tcu_codified_status.Items.Refresh());
                }
                catch (Exception ex)
                {
                    Manage_logs.SaveErrorValue(GetType().Name + " ->  " + nameof(Read_tcu_modbus) + " -> " + ex.Message.ToString());
                }
            }
        }

        #endregion


        #region Timer write TCU modbus

        private void Timer_write_tcu_modbus_Tick(object sender, EventArgs e)
        {
            //Mostrar valor de las variables de lectura
            if (b_read_met_modbus)
            {
                m_scs_meteo_station_values[0] = m_counter_scs;
                Dispatcher.Invoke(() =>
                {
                    m_array_label_emet_watchdog_value.Select((item, index) => new { Item = item, Position = index }).ToList()
                    .ForEach(label_emet => label_emet.Item.Content = m_scs_meteo_station_values[label_emet.Position].ToString(Globals.GetTheInstance().nfi));
                });
            }

            if (b_write_tcu_modbus)
                Write_tcu_modbus();
        }

        private void Write_tcu_modbus()
        {
            TCPModbusSlaveEntry? slave_entry = Globals.GetTheInstance().List_modbus_slave_entry.FirstOrDefault(entry => entry.TCU);
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
                    }

                    else
                    {
                        s_log += "WATCHDOG ";

                        list_write_values.Add(Constants.CMD_SCS_METEO_STATION_VALUES);

                        Dispatcher.Invoke(() =>
                        {
                            m_array_label_emet_watchdog_value.Select((item, index) => new { Item = item, Position = index }).ToList()
                               .ForEach(label_emet =>
                               {
                                   if (label_emet.Item.Content != null)
                                   {
                                       int emet_value = 0;

                                       if (!string.IsNullOrEmpty(label_emet.Item.Content.ToString()))
                                       {
                                           if (label_emet.Position == (int)EMET_WATCHDOG_ASSOC.NONE)
                                               emet_value = int.Parse(label_emet.Item.Content.ToString()); //Counter SCS

                                           else if (label_emet.Position == (int)EMET_WATCHDOG_ASSOC.FIELD_SAFETY_SUPERVISOR)
                                           {
                                               label_emet.Item.Content = "0xFFFF";
                                               emet_value = 0xFFFF;
                                           }

                                           else
                                               emet_value = (int)(float.Parse(label_emet.Item.Content.ToString()) * 100); //Multiplicarlo por 100 para los decimales
                                       }

                                       list_write_values.Add(emet_value);
                                   }

                               });
                        });
                    }

                    s_log += " COMMAND -> ";
                    list_write_values.ForEach(value => s_log += value.ToString() + " ");
                    Manage_logs.SaveLogValue(s_log);

                    Manage_thread manage_thread = Globals.GetTheInstance().List_manage_thread.First(manage_thread => manage_thread.Slave_name == slave_entry.Name);
                    manage_thread.Write_multiple_registers(Globals.GetTheInstance().Modbus_dir_scs_command, list_write_values.ToArray());

                    m_counter_scs = m_counter_scs == UInt16.MaxValue ? 0 : m_counter_scs + 1;
                }
                catch (Exception ex)
                {
                    Manage_logs.SaveErrorValue(GetType().Name + " ->  " + nameof(Write_tcu_modbus) + " -> " + ex.Message.ToString());
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

        private void Timer_record_met1_data_Tick(object sender, EventArgs e)
        {
            string s_record_file = "Record_scs1_";
            Record_data_met(s_record_file);
        }

        private void Timer_record_met2_data_Tick(object sender, EventArgs e)
        {
            string s_record_file = "Record_scs2_";
            Record_data_met(s_record_file);
        }

        private void Record_data_met(string s_file)
        {
            if (b_read_met_modbus && b_record && b_first_read_met_finish)
            {
                try
                {
                    string s_dir = AppDomain.CurrentDomain.BaseDirectory + Constants.Record_dir + @"\" + String.Format("{0:0000}", DateTime.Now.Year) + @"\" + String.Format("{0:00}", DateTime.Now.Month);
                    if (!Directory.Exists(s_dir))
                        Directory.CreateDirectory(s_dir);


                    s_file = s_dir + @"\" + s_file + DateTime.Now.Day.ToString("00") + DateTime.Now.Month.ToString("00") + DateTime.Now.Year.ToString("0000") + ".csv";
                    if (!File.Exists(s_file))
                    {
                        using FileStream fs = File.Create(s_file);
                        fs.Close();

                        string s_head = $"UTCDATE{Globals.GetTheInstance().SField_sep}";
                        s_head += $"DDATE{Globals.GetTheInstance().SField_sep}";
                        m_collection_modbus_var.ToList().ForEach(modbus_var => s_head += $"{modbus_var.Name}{ Globals.GetTheInstance().SField_sep}");

                        //Analizar codified status var
                        Globals.GetTheInstance().List_tcu_codified_status
                            .Where(codified_status => codified_status.SCS_record).ToList()
                            .ForEach(codified_status => s_head+= $"{codified_status.Name}{ Globals.GetTheInstance().SField_sep}");

                        s_head = s_head.Remove(s_head.Length - 1);

                        using StreamWriter stream_writer_head = new(s_file);
                        stream_writer_head.WriteLine(s_head);
                    }

                    double d_value = DateTime.UtcNow.ToOADate();

                    //Fecha formato UTC
                    string s_line = $"{DateTime.UtcNow.ToString(Globals.GetTheInstance().Date_format, new CultureInfo(Globals.GetTheInstance().Format_provider))}{Globals.GetTheInstance().SField_sep}";
                    s_line += $"{DateTime.UtcNow.ToOADate().ToString(Globals.GetTheInstance().nfi)}{Globals.GetTheInstance().SField_sep}";

                    m_collection_modbus_var.ToList().ForEach(modbus_var => s_line += $"{modbus_var.Value}{ Globals.GetTheInstance().SField_sep}");

                    //Analizar codified status var
                    Globals.GetTheInstance().List_tcu_codified_status
                        .Where(codified_status => codified_status.SCS_record).ToList()
                        .ForEach(codified_status => s_line += $"{codified_status.Value}{ Globals.GetTheInstance().SField_sep}");

                    s_line = s_line.Remove(s_line.Length - 1);

                    using StreamWriter stream_writer_data = new(s_file, true);
                    stream_writer_data.WriteLine(s_line);
                }
                catch (Exception ex)
                {
                    Manage_logs.SaveErrorValue(GetType().Name + " ->  " + nameof(Record_data_met) + " -> " + ex.Message.ToString());
                }
            }
        }


        private void Timer_record_tcu_data_Tick(object sender, EventArgs e)
        {
            string s_record_file = "Record_tcu_";
            Record_data_tcu(s_record_file);
        }

        private void Record_data_tcu(string s_file)
        {
            if (b_read_tcu_modbus && b_record && b_first_read_tcu_finish)
            {
                try
                {
                    string s_dir = AppDomain.CurrentDomain.BaseDirectory + Constants.Record_dir + @"\" + String.Format("{0:0000}", DateTime.Now.Year) + @"\" + String.Format("{0:00}", DateTime.Now.Month);
                    if (!Directory.Exists(s_dir))
                        Directory.CreateDirectory(s_dir);


                    s_file = s_dir + @"\" + s_file + DateTime.Now.Day.ToString("00") + DateTime.Now.Month.ToString("00") + DateTime.Now.Year.ToString("0000") + ".csv";
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

                    using StreamWriter stream_writer_data = new(s_file, true);
                    stream_writer_data.WriteLine(s_line);
                }
                catch (Exception ex)
                {
                    Manage_logs.SaveErrorValue(GetType().Name + " ->  " + nameof(Record_data_tcu) + " -> " + ex.Message.ToString());
                }
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

                                string s_log = "COMPRESS FILES OK -> ";

                                List<string> list_record_file = new() { "Record_s_", "Record_l_" };
                                list_record_file.ForEach(record_file =>
                                {
                                    string s_file = AppDomain.CurrentDomain.BaseDirectory + Constants.Record_dir + @"\" + record_file + DateTime.Now.AddDays(-1).Day.ToString("00") + DateTime.Now.Month.ToString("00") + DateTime.Now.Year.ToString("0000") + ".csv";

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
                                Manage_logs.SaveErrorValue(GetType().Name + " ->  " + nameof(Timer_send_mail_Tick) + " -> " + ex.Message.ToString());
                                m_mail_state = MAIL_STATE.END;
                            }
                            m_timer_send_mail.Start();

                            break;
                        }

                    case MAIL_STATE.SEND:
                        {
                            m_timer_send_mail.Stop();

                            Tuple<bool, List<string>> tuple_send_mail = Functions.Send_mail();
                            List<string> list_report_files = tuple_send_mail.Item2;
                            if (tuple_send_mail.Item1)
                            {
                                string s_log = "SEND MAIL OK. Files -> ";
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
            if (b_read_met_modbus)
                Checkbox_start.IsChecked = false;

            SettingAppWindow settingAppWindow = new()
            {
                Left = this.Left,
                Top = this.Top,
            };
            settingAppWindow.ShowDialog();

            #region Redefine timers

            m_timer_record_data_met1.Stop();
            m_timer_record_data_met2.Stop();
            m_timer_record_data_tcu.Stop();

            m_timer_read_met_modbus.Stop();
            m_timer_read_tcu_modbus.Stop();
            m_timer_write_tcu_modbus.Stop();


            m_timer_record_data_met1.Interval = Globals.GetTheInstance().Record_data_met1_interval;
            m_timer_record_data_met2.Interval = Globals.GetTheInstance().Record_data_met2_interval;
            m_timer_record_data_tcu.Interval = Globals.GetTheInstance().Record_data_tcu_interval;

            m_timer_read_met_modbus.Interval = Globals.GetTheInstance().Modbus_read_met_interval;
            m_timer_read_tcu_modbus.Interval = Globals.GetTheInstance().Modbus_read_tcu_interval;
            m_timer_write_tcu_modbus.Interval = Globals.GetTheInstance().Modbus_write_tcu_interval;


            m_timer_record_data_met1.Start();
            m_timer_record_data_met2.Start();
            m_timer_record_data_tcu.Start();

            m_timer_read_met_modbus.Start();
            m_timer_read_tcu_modbus.Start();
            m_timer_write_tcu_modbus.Start();

            #endregion

            Globals.GetTheInstance().nfi.NumberDecimalSeparator = Globals.GetTheInstance().Decimal_sep == DECIMAL_SEP.PUNTO ? "." : ",";
            this.Height = Globals.GetTheInstance().Depur_enable == BIT_STATE.ON ? Constants.depur_enable_height : Constants.depur_disable_height;
        }

        #endregion

        #region Setting mail

        private void Button_setting_mail_Click(object sender, RoutedEventArgs e)
        {
            if (b_read_met_modbus)
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
            if (b_read_met_modbus)
                Checkbox_start.IsChecked = false;


            m_timer_read_met_modbus.Stop();
            m_timer_read_tcu_modbus.Stop();
            m_timer_write_tcu_modbus.Stop();

            m_timer_record_data_met1.Stop();
            m_timer_record_data_met2.Stop();
            m_timer_record_data_tcu.Stop();

            SettingModbusWindow setting_modbus_window = new()
            {
                Left = this.Left,
                Top = this.Top,
            };
            setting_modbus_window.ShowDialog();

            m_timer_read_met_modbus.Interval = Globals.GetTheInstance().Modbus_read_met_interval;
            m_timer_read_tcu_modbus.Interval = Globals.GetTheInstance().Modbus_read_tcu_interval;
            m_timer_write_tcu_modbus.Interval = Globals.GetTheInstance().Modbus_write_tcu_interval;
            m_timer_record_data_met1.Interval = Globals.GetTheInstance().Record_data_met1_interval;
            m_timer_record_data_met2.Interval = Globals.GetTheInstance().Record_data_met2_interval;
            m_timer_record_data_tcu.Interval = Globals.GetTheInstance().Record_data_tcu_interval;

            m_timer_read_met_modbus.Start();
            m_timer_read_tcu_modbus.Start();
            m_timer_write_tcu_modbus.Start();

            m_timer_record_data_met1.Start();
            m_timer_record_data_met2.Start();
            m_timer_record_data_tcu.Start();

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
