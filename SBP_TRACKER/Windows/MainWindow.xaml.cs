using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace SBP_TRACKER
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Timers.Timer m_timer_start_tcp;
        private System.Timers.Timer m_timer_read_image;
        private System.Timers.Timer m_timer_record_data;
        private bool b_read_image;
        private bool b_record = true;

        private ObservableCollection<TCPModbusSlaveEntry> m_collection_modbus_slave;
        private ObservableCollection<TCPModbusVar> m_collection_modbus_var;

        private TCPModbusSlaveEntry m_selected_slave_entry;


        private List<string> m_list_var_map_schema = new();

        private List<Label> m_array_label_schema_title = new();
        private List<Label> m_array_label_schema_value = new();


        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            Title = "SBP TRACKER / ver " + Constants.version;
            Label_ver.Content = "SBP TRACKER / ver " + Constants.version;


            #region Array controles

            m_array_label_schema_title = new List<Label>();
            m_array_label_schema_title.Add(Label_wd1_title);
            m_array_label_schema_title.Add(Label_anenomether1_title);
            m_array_label_schema_title.Add(Label_dyn1_title);
            m_array_label_schema_title.Add(Label_sick_inc1_title);

            m_array_label_schema_title.ForEach(label_schema =>
            {
                label_schema.PreviewMouseDown += new MouseButtonEventHandler(LabelSchema_PreviewMouseDown_EventHandler);
                label_schema.PreviewDragOver += new DragEventHandler(LabelSchema_PreviewDragOver_EventHandler);
                label_schema.PreviewDrop += new DragEventHandler(LabelSchema_PreviewDrop_EventHandler);
            });

            m_array_label_schema_value = new List<Label>();
            m_array_label_schema_value.Add(Label_wd1_value);
            m_array_label_schema_value.Add(Label_anenomether1_value);
            m_array_label_schema_value.Add(Label_dyn1_value);
            m_array_label_schema_value.Add(Label_sick_inc1_value);


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


            bool load_app_ok = Manage_file.Load_app_setting();
            bool load_modbus_ok = Load_modbus_slave_var();
            if (!load_app_ok || !load_modbus_ok)
            {
                MessageBox.Show("Error loading config. files. Check the system", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                Application.Current.Shutdown();
            }


            Wrap_read_depur.Visibility = Globals.GetTheInstance().Depur_enable == BIT_STATE.ON ? Visibility.Visible : Visibility.Collapsed;
            this.Height= Globals.GetTheInstance().Depur_enable == BIT_STATE.ON ? 740 : 690;

            #region Timer

            m_timer_start_tcp = new System.Timers.Timer();
            m_timer_start_tcp.Elapsed += Timer_start_tcp_Tick;
            m_timer_start_tcp.Interval = 1000;
            m_timer_start_tcp.Stop();

            m_timer_record_data = new System.Timers.Timer();
            m_timer_record_data.Elapsed += Timer_record_data_Tick;
            m_timer_record_data.Interval = Globals.GetTheInstance().Record_data_interval;
            m_timer_record_data.Start();


            m_timer_read_image = new System.Timers.Timer();
            m_timer_read_image.Elapsed += Timer_read_image_Tick;
            m_timer_read_image.Interval = Globals.GetTheInstance().Modbus_read_interval;
            m_timer_read_image.Start();
            b_read_image = false;

            #endregion
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
                        TCPModbusSlaveEntry entry = Globals.GetTheInstance().List_modbus_slave_entry.First(entry => entry.Name == args.Slave_name);
                        entry.Connected = true;
                        entry.Check_start_conn = true;

                        if (Globals.GetTheInstance().List_modbus_slave_entry.All(entry => entry.Check_start_conn) && Globals.GetTheInstance().List_modbus_slave_entry.Exists(entry => entry.Connected))
                        {
                            if (Globals.GetTheInstance().List_modbus_slave_entry.Exists(entry => entry.Connected))
                                b_read_image = true;
                            else
                                Dispatcher.Invoke(() => Checkbox_start.IsChecked = false);

                            Dispatcher.Invoke(() => Border_wait.Visibility = Visibility.Collapsed);

                        }

                        Dispatcher.Invoke(() => Listview_slave_modbus.Items.Refresh());

                        break;
                    }

                case TCP_ACTION.DISCONNECT:
                    {
                        Globals.GetTheInstance().List_modbus_slave_entry.First(entry => entry.Name == args.Slave_name).Connected = false;
                        Dispatcher.Invoke(() => Listview_slave_modbus.Items.Refresh());

                        break;
                    }

                case TCP_ACTION.READ:
                    {
                        byte[] array_data = args.List_data.ToArray().SelectMany(BitConverter.GetBytes).ToArray();

                        Dispatcher.Invoke(() => Label_depur.Content = DataConverter.ByteArrayToHex(array_data));
                        break;
                    }

                case TCP_ACTION.ERROR_CONNECT:
                    {
                        TCPModbusSlaveEntry entry = Globals.GetTheInstance().List_modbus_slave_entry.First(entry => entry.Name == args.Slave_name);
                        entry.Check_start_conn = true;

                        if (Globals.GetTheInstance().List_modbus_slave_entry.All(entry => entry.Check_start_conn))
                        {
                            if (Globals.GetTheInstance().List_modbus_slave_entry.Exists(entry => entry.Connected))
                                b_read_image = true;
                            else
                                Dispatcher.Invoke(() => Checkbox_start.IsChecked = false);


                            Dispatcher.Invoke(() => Border_wait.Visibility = Visibility.Collapsed);
                        }

                        Globals.GetTheInstance().List_manage_thread.First(manage_thread => manage_thread.Slave_name == args.Slave_name).ManageTCP.Disconnect();
                        Dispatcher.Invoke(() => Listview_slave_modbus.Items.Refresh());
                        break;
                    }

                case TCP_ACTION.ERROR_READ:
                    {
                        Globals.GetTheInstance().List_manage_thread.First(manage_thread => manage_thread.Slave_name == args.Slave_name).ManageTCP.Disconnect();
                        Dispatcher.Invoke(() => Listview_slave_modbus.Items.Refresh());
                        break;
                    }
            }
        }

        #endregion



        #region Show mode -> switch events

        private void SwitchSliderShowMode_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Dockpanel_table.Visibility = SwitchSliderShowMode.ToggledState ? Visibility.Collapsed : Visibility.Visible;
            Dockpanel_schema.Visibility = SwitchSliderShowMode.ToggledState ? Visibility.Visible : Visibility.Collapsed;

            Select_slave_var(null);
        }

        private void SwitchSliderShowMode_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Dockpanel_table.Visibility = SwitchSliderShowMode.ToggledState ? Visibility.Collapsed : Visibility.Visible;
            Dockpanel_schema.Visibility = SwitchSliderShowMode.ToggledState ? Visibility.Visible : Visibility.Collapsed;

            Select_slave_var(null);
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

        #region Listview schema drag and dropVar2_Ander

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
            Label label = sender as Label;
            string s_content = e.Data.GetData(typeof(string)) as string;
            label.Content = s_content;
            e.Handled = true;

            //Se mueve desde un textbox a otro
            if (m_label_drag != null)
            {
                m_label_drag.Content = String.Empty;
                m_label_drag = null;
            }


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
                list_entry = list_entry.Where(modbus_slave_entry => modbus_slave_entry.Name == entry.Name).ToList();
                Checkbox_all_slave.IsChecked = false;
            }


            list_entry.ForEach(modbus_slave_entry =>
            {
                modbus_slave_entry.List_modbus_var.ForEach(modbus_slave_var =>
                {
                    m_collection_modbus_var.Add(modbus_slave_var);
                });
            });


            m_collection_modbus_var = new ObservableCollection<TCPModbusVar>(m_collection_modbus_var.OrderBy(modbus_var => modbus_var.Slave).ThenBy(modbus_var => modbus_var.Dir));
            Listview_read_modbus.ItemsSource = m_collection_modbus_var;
            Listview_read_modbus.Items.Refresh();
        }

        #endregion

        #endregion




        #region Load Modbus slave / var

        private bool Load_modbus_slave_var()
        {
            bool load_modbus_ok = true;

            bool read_slave_ok = Manage_file.Read_modbus_slave_entries();
            bool read_var_map_ok = Manage_file.Read_var_map_entries();

            load_modbus_ok = read_slave_ok && read_var_map_ok;

            try
            {
                if (load_modbus_ok)
                {
                    #region Slave - Var map

                    m_collection_modbus_slave = new ObservableCollection<TCPModbusSlaveEntry>();
                    m_collection_modbus_var = new ObservableCollection<TCPModbusVar>();
                    m_list_var_map_schema.Clear();
                    Globals.GetTheInstance().List_modbus_slave_entry.ForEach(modbus_slave_entry =>
                    {
                        m_collection_modbus_slave.Add(modbus_slave_entry);

                        modbus_slave_entry.List_modbus_var.ForEach(modbus_slave_var =>
                        {
                            m_collection_modbus_var.Add(modbus_slave_var);

                            m_list_var_map_schema.Add(modbus_slave_var.Name);

                            if (modbus_slave_var.Schema_pos != Constants.index_no_selected)
                            {
                                m_array_label_schema_title[modbus_slave_var.Schema_pos].Content = modbus_slave_var.Name;
                            }
                        });
                    });

                    Listview_slave_modbus.ItemsSource = m_collection_modbus_slave;
                    Listview_slave_modbus.Items.Refresh();

                    m_collection_modbus_var = new ObservableCollection<TCPModbusVar>(m_collection_modbus_var.OrderBy(modbus_var => modbus_var.Slave).ThenBy(modbus_var => modbus_var.Dir));
                    Listview_read_modbus.ItemsSource = m_collection_modbus_var;
                    Listview_read_modbus.Items.Refresh();

                    Listview_schema_var_map.ItemsSource = m_list_var_map_schema;
                    Listview_schema_var_map.Items.Refresh();

                    #endregion


                    #region Start manage thread

                    Globals.GetTheInstance().List_manage_thread = new List<Manage_thread>();
                    Globals.GetTheInstance().List_modbus_slave_entry.ForEach(slave_entry =>
                    {
                        Manage_thread manage_thread = new(slave_entry.Name, slave_entry.IP_primary, slave_entry.Port, slave_entry.UnitId);
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



        #region Start / Stop

        private void Checkbox_start_Checked(object sender, RoutedEventArgs e)
        {
            Border_wait.Visibility = Visibility.Visible;
            m_timer_start_tcp.Start();
        }

        private void Timer_start_tcp_Tick(object sender, EventArgs e)
        {
            m_timer_start_tcp.Stop();
            Globals.GetTheInstance().List_manage_thread.ForEach(manage => manage.Start_tcp_com());
        }


        private void Checkbox_start_Unchecked(object sender, RoutedEventArgs e)
        {
            b_read_image = false;
            Globals.GetTheInstance().List_manage_thread.ForEach(manage => manage.Stop_tcp_com_thread());

            m_collection_modbus_var.ToList().ForEach(modbus_var => modbus_var.Value = string.Empty);
            Listview_read_modbus.Items.Refresh();
            m_array_label_schema_value.ForEach(label => label.Content = string.Empty);
        }

        #endregion

        #region Read modbus

        private void Button_read_modbus_Click(object sender, RoutedEventArgs e)
        {
            Read_image();
        }

        #endregion

        #region Timer read image

        private void Timer_read_image_Tick(object sender, EventArgs e)
        {
            if (b_read_image)
                Read_image();
        }


        private void Read_image()
        {

            //Analizar si está filtrado un slave
            List<Manage_thread> list_read_thread = Globals.GetTheInstance().List_manage_thread;
            if (m_selected_slave_entry != null)
            {
                list_read_thread = list_read_thread.Where(manage_thread => manage_thread.Slave_name == m_selected_slave_entry.Name).ToList();
            }

            string s_content = string.Empty;
            list_read_thread.ForEach(manage_thread =>
            {
                if (manage_thread.ManageTCP.Is_connected())
                {
                    Tuple<bool, int[]> tuple_read = manage_thread.ManageTCP.Read_holding_registers_int32(Globals.GetTheInstance().Modbus_start_address, Constants.MAX_MODBUS_REG);

                    string s_data = manage_thread.Slave_name + " -> " + DataConverter.ByteArrayToHex(tuple_read.Item2.SelectMany(BitConverter.GetBytes).ToArray());
                    s_content += s_data + "\r\n";

                    if (tuple_read.Item2.Length != 0)
                    {
                        List<TCPModbusVar> list_modbus_var = m_collection_modbus_var.Where(modbus_var => modbus_var.Slave == manage_thread.Slave_name).ToList();

            
                        list_modbus_var.ForEach(modbus_var =>
                        {
                            int read_array_pos = modbus_var.Dir - Globals.GetTheInstance().Modbus_start_address;

                            //Table
                            modbus_var.Value = Functions.Read_value(tuple_read.Item2, read_array_pos, modbus_var.Type);

                            //Schema
                            Dispatcher.Invoke(() =>
                            {

                                ControlLinq? control_linq = m_array_label_schema_title
                                .Select((item, index) => new ControlLinq { Value = item.Content.ToString(), Position = index })
                                .FirstOrDefault(control => control.Value.Equals(modbus_var.Name));
                                if (control_linq != null)
                                    m_array_label_schema_value[control_linq.Position].Content = modbus_var.Value;
                            });

                        });
                    }
                }
            });

            Dispatcher.Invoke(() => Label_depur.Content = s_content);

            Dispatcher.Invoke(() => Listview_read_modbus.Items.Refresh());
        }

        #endregion



        #region Timer record data

        private void Timer_record_data_Tick(object sender, EventArgs e)
        {
            if (b_read_image && b_record)
            {
                DateTime record_date = DateTime.Now;
                using StreamWriter stream_writer = new(AppDomain.CurrentDomain.BaseDirectory + Constants.Report_dir + @"\dataLog.csv", true);

                m_collection_modbus_var.ToList().ForEach(modbus_var =>
                {
                    if (!string.IsNullOrEmpty(modbus_var.Value))
                    {
                        stream_writer.WriteLine($"{record_date};{modbus_var.Slave};{modbus_var.Name};{modbus_var.SType};{modbus_var.Dir};{modbus_var.Value}");
                    }
                });
            }
        }

        #endregion

        #region Record data

        private void Button_record_Click(object sender, RoutedEventArgs e)
        {
            Image_record_on.Visibility = b_record ? Visibility.Collapsed : Visibility.Visible;
            Image_record_off.Visibility = b_record ? Visibility.Visible : Visibility.Collapsed;
            b_record = !b_record;
        }

        #endregion


        #region Setting APP
        private void Button_setting_app_Click(object sender, RoutedEventArgs e)
        {
            if (b_read_image)
                Checkbox_start.IsChecked = false;

            SettingAppWindow settingAppWindow = new()
            {
                Left = this.Left,
                Top = this.Top,
            };
            settingAppWindow.ShowDialog();


            Wrap_read_depur.Visibility = Globals.GetTheInstance().Depur_enable == BIT_STATE.ON ? Visibility.Visible : Visibility.Collapsed;
            this.Height = Globals.GetTheInstance().Depur_enable == BIT_STATE.ON ? 740 : 690;

        }

        #endregion

        #region Setting modbus

        private void Button_setting_modbus_Click(object sender, RoutedEventArgs e)
        {
            if (b_read_image)
                Checkbox_start.IsChecked = false;


            SettingModbusWindow setting_modbus_window = new()
            {
                Left = this.Left,
                Top = this.Top,
            };
            setting_modbus_window.ShowDialog();


            Load_modbus_slave_var();
        }

        #endregion


        #region Exit

        private void Button_exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }








        #endregion

    }
}
