using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;



namespace SBP_TRACKER
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SettingModbusWindow : Window
    {
        private FORM_ACTION m_action;

        private ObservableCollection<TCPModbusVarEntry> m_collection_var_entry;

        private TCPModbusSlaveEntry m_current_slave_entry;
        private TCPModbusVarEntry m_current_var_entry;


        #region Constructor

        public SettingModbusWindow()
        {
            InitializeComponent();

            m_collection_var_entry = new ObservableCollection<TCPModbusVarEntry>();
            Listview_var_map.ItemsSource = m_collection_var_entry;
        }

        #endregion


        #region Loaded

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Wrap_edit_tcp_slave.IsEnabled = false;
            Border_mapped.IsEnabled = false;

            Listview_tcp_slave.ItemsSource = Globals.GetTheInstance().List_slave_entry.OrderBy(slave_entry => slave_entry.Name);
            Listview_tcp_slave.Items.Refresh();
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




        #region Button exit

        private void Button_exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion



        #region TCP slave 

        #region Listview TCP slave selection changed
        private void Listview_tcp_slave_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Listview_tcp_slave.SelectedItems.Count > 0)
            {
                if (Listview_tcp_slave.SelectedItem is TCPModbusSlaveEntry mySelectedItem)
                {
                    m_current_slave_entry = Globals.GetTheInstance().List_slave_entry.First(slave_entry => slave_entry.Name.Equals(mySelectedItem.Name));
                    Textbox_tcp_slave_name.Text = m_current_slave_entry.Name;


                    #region IP primary

                    Textbox_tcp_slave_ip_primary.firstBox.Text = String.Empty;
                    Textbox_tcp_slave_ip_primary.secondBox.Text = String.Empty;
                    Textbox_tcp_slave_ip_primary.thirdBox.Text = String.Empty;
                    Textbox_tcp_slave_ip_primary.fourthBox.Text = String.Empty;

                    byte pos_box = 0;
                    IEnumerable<char> enum_tcp_primary = m_current_slave_entry.IP_primary.Take(m_current_slave_entry.IP_primary.Length);
                    enum_tcp_primary.ToList().Select((item, index) => new { Item = item, Index = index }).ToList()
                        .ForEach(x =>
                        {
                            if (x.Item == Convert.ToChar("."))
                                pos_box++;
                            else {
                                TextBox current_textbox =
                                    pos_box == 0 ? Textbox_tcp_slave_ip_primary.firstBox :
                                    pos_box == 1 ? Textbox_tcp_slave_ip_primary.secondBox :
                                    pos_box == 2 ? Textbox_tcp_slave_ip_primary.thirdBox :
                                    Textbox_tcp_slave_ip_primary.fourthBox;

                                current_textbox.Text += x.Item;
                            }
                        });

                    #endregion

                    DecimalUpDown_port.Value = m_current_slave_entry.Port;
                    DecimalUpDown_UnitId.Value = m_current_slave_entry.UnitId;
                    DecimalUpDown_dir_ini.Value = m_current_slave_entry.Dir_ini;
                    DecimalUpDown_read_reg.Value = m_current_slave_entry.Read_reg;

                    Combobox_mb_function.SelectedIndex = (int)m_current_slave_entry.Modbus_function;
                    Checkbox_fast_mode.IsChecked = m_current_slave_entry.Fast_mode;

                    Radiobutton_General.IsChecked = m_current_slave_entry.Slave_type == SLAVE_TYPE.GENERAL;
                    Radiobutton_TCU.IsChecked = m_current_slave_entry.Slave_type == SLAVE_TYPE.TCU;
                    Radiobutton_Samca.IsChecked = m_current_slave_entry.Slave_type == SLAVE_TYPE.SAMCA;

                    #region Var map list

                    m_collection_var_entry.Clear();
                    m_current_slave_entry.List_var_entry.ForEach(var_entry => m_collection_var_entry.Add(var_entry));
                
                    m_collection_var_entry = new ObservableCollection<TCPModbusVarEntry>(m_collection_var_entry.OrderBy(modbus_var => modbus_var.DirModbus));
                    Listview_var_map.ItemsSource = m_collection_var_entry;
                    CollectionViewSource.GetDefaultView(Listview_var_map.ItemsSource).Refresh();

                    #endregion
                }
            }
        }

        #endregion

        #region Listview TCP slave double click
        private void Listview_tcp_slave_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (m_current_slave_entry != null)
            {
                Wrap_edit_tcp_slave.IsEnabled = true;
                Border_mapped.IsEnabled = true;
                m_action = FORM_ACTION.UPDATE;

                #region Bind data to list var map

                m_collection_var_entry.Clear();
                m_current_slave_entry.List_var_entry.ForEach(var_entry => m_collection_var_entry.Add(var_entry));

                m_collection_var_entry = new ObservableCollection<TCPModbusVarEntry>(m_collection_var_entry.OrderBy(modbus_var => modbus_var.DirModbus));
                Listview_var_map.ItemsSource = m_collection_var_entry;
                CollectionViewSource.GetDefaultView(Listview_var_map.ItemsSource).Refresh();

                #endregion
            }
        }


        #endregion

        #region New TCP slave

        private void Button_new_tcp_slave_Click(object sender, RoutedEventArgs e)
        {
            m_current_slave_entry = new TCPModbusSlaveEntry();

            #region Ini form control values

            Textbox_tcp_slave_name.Text = String.Empty;
            DecimalUpDown_UnitId.Value = 247;

            Textbox_tcp_slave_ip_primary.firstBox.Text = String.Empty;
            Textbox_tcp_slave_ip_primary.secondBox.Text = String.Empty;
            Textbox_tcp_slave_ip_primary.thirdBox.Text = String.Empty;
            Textbox_tcp_slave_ip_primary.fourthBox.Text = String.Empty;

            DecimalUpDown_port.Value = 502;

            #endregion

            m_action = FORM_ACTION.CREATE;

            Border_slave_list.IsEnabled = false;
            Border_mapped.IsEnabled = false;
            Wrap_edit_tcp_slave.IsEnabled = true;

            m_collection_var_entry.Clear();
            CollectionViewSource.GetDefaultView(Listview_var_map.ItemsSource).Refresh();
        }

        #endregion

        #region Remove TCP slave

        private void Button_remove_tcp_slave_Click(object sender, RoutedEventArgs e)
        {
            m_action = FORM_ACTION.REMOVE;
            if (m_current_slave_entry != null)
            {
                Globals.GetTheInstance().List_slave_entry.Remove(m_current_slave_entry);

                bool save_slave_ok =  Manage_file.Save_modbus_slave_entries();
                bool save_var_map_ok = Manage_file.Save_var_map_entries();
                if (!save_slave_ok || !save_var_map_ok)
                    MessageBox.Show("Error deleting slave", "Error saving", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                
                else
                {
                    Listview_tcp_slave.ItemsSource = Globals.GetTheInstance().List_slave_entry.OrderBy(slave_entry => slave_entry.Name);
                    Listview_tcp_slave.Items.Refresh();
                    MessageBox.Show("Slave deleted", "Save ok", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            Clear_controls(FORM_ACTION.REMOVE);
        }

        #endregion

        #region Update TCP slave
        private void Button_update_tcp_slave_Click(object sender, RoutedEventArgs e)
        {
            #region Check parameters

            bool save = true;

            if (
                Textbox_tcp_slave_ip_primary.firstBox.Text == String.Empty ||
                Textbox_tcp_slave_ip_primary.secondBox.Text == String.Empty ||
                Textbox_tcp_slave_ip_primary.thirdBox.Text == String.Empty ||
                Textbox_tcp_slave_ip_primary.fourthBox.Text == String.Empty)
                save = false;

            if (Textbox_tcp_slave_name.Text == String.Empty)
                save = false;

            if (Combobox_mb_function.SelectedIndex == Constants.index_no_selected)
                save = false;

            if (!save)
                MessageBox.Show("Check parameters", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            

            if (save)
            {
                if ((bool)Radiobutton_TCU.IsChecked == true && m_current_slave_entry.Slave_type != SLAVE_TYPE.TCU  && Globals.GetTheInstance().List_slave_entry.Exists(slave_entry => slave_entry.Slave_type == SLAVE_TYPE.TCU))
                {
                    MessageBox.Show("There is already modbus slav  configured as TCU in the system", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }

                if ((bool)Radiobutton_Samca.IsChecked == true && m_current_slave_entry.Slave_type != SLAVE_TYPE.SAMCA && Globals.GetTheInstance().List_slave_entry.Exists(slave_entry => slave_entry.Slave_type == SLAVE_TYPE.SAMCA))
                {
                    MessageBox.Show("There is already modbus slave configured as SAMCA in the system", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }

            #endregion

            if (save)
            { 
                string modbus_slave_csv = AppDomain.CurrentDomain.BaseDirectory + @"SettingModbusSlave\modbusSlave.csv";

                if (m_action == FORM_ACTION.CREATE)
                {
                    m_current_slave_entry.Name = Textbox_tcp_slave_name.Text;
                    m_current_slave_entry.IP_primary = Textbox_tcp_slave_ip_primary.firstBox.Text + "." + Textbox_tcp_slave_ip_primary.secondBox.Text + "." + Textbox_tcp_slave_ip_primary.thirdBox.Text + "." + Textbox_tcp_slave_ip_primary.fourthBox.Text;
                    m_current_slave_entry.Port = (int)DecimalUpDown_port.Value;
                    m_current_slave_entry.UnitId = (byte)DecimalUpDown_UnitId.Value;
                    m_current_slave_entry.Dir_ini = (int)DecimalUpDown_dir_ini.Value;
                    m_current_slave_entry.Read_reg = (int)DecimalUpDown_read_reg.Value;
                    m_current_slave_entry.Modbus_function = (MODBUS_FUNCION) Combobox_mb_function.SelectedIndex;
                    m_current_slave_entry.Slave_type = Radiobutton_General.IsChecked == true ? SLAVE_TYPE.GENERAL : Radiobutton_TCU.IsChecked == true ? SLAVE_TYPE.TCU : SLAVE_TYPE.SAMCA; 
                    m_current_slave_entry.Fast_mode = (bool)Checkbox_fast_mode.IsChecked;
                    m_current_slave_entry.List_var_entry = new List<TCPModbusVarEntry>();

                    Globals.GetTheInstance().List_slave_entry.Add(m_current_slave_entry);
                }

                else if (m_action == FORM_ACTION.UPDATE)
                {
                    Globals.GetTheInstance().List_slave_entry.Where(slave_entry => slave_entry.Name == m_current_slave_entry.Name).Select(slave_entry =>
                    {
                        slave_entry.Name = Textbox_tcp_slave_name.Text;
                        slave_entry.IP_primary = Textbox_tcp_slave_ip_primary.firstBox.Text + "." + Textbox_tcp_slave_ip_primary.secondBox.Text + "." + Textbox_tcp_slave_ip_primary.thirdBox.Text + "." + Textbox_tcp_slave_ip_primary.fourthBox.Text;
                        slave_entry.Port = (int)DecimalUpDown_port.Value;
                        slave_entry.UnitId = (byte)DecimalUpDown_UnitId.Value;
                        slave_entry.Dir_ini = (int)DecimalUpDown_dir_ini.Value;
                        slave_entry.Read_reg = (int)DecimalUpDown_read_reg.Value;
                        slave_entry.Modbus_function = (MODBUS_FUNCION) Combobox_mb_function.SelectedIndex;
                        slave_entry.Slave_type = Radiobutton_General.IsChecked == true ? SLAVE_TYPE.GENERAL : Radiobutton_TCU.IsChecked == true ? SLAVE_TYPE.TCU : SLAVE_TYPE.SAMCA;
                        slave_entry.Fast_mode = (bool)Checkbox_fast_mode.IsChecked;
                        return slave_entry;
                    }).ToList();
                }
 
                bool save_slave_ok = Manage_file.Save_modbus_slave_entries();
                if (!save_slave_ok)
                    MessageBox.Show("Error saving system config", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                
                else
                {
                    Clear_controls(FORM_ACTION.UPDATE);
                    Listview_tcp_slave.ItemsSource = Globals.GetTheInstance().List_slave_entry.OrderBy(slave_entry => slave_entry.Name);
                    Listview_tcp_slave.Items.Refresh();
                }
            }
        }

        #endregion

        #region Cancel TCP slave

        private void Button_cancel_tcp_slave_Click(object sender, RoutedEventArgs e)
        {
            Clear_controls(FORM_ACTION.CANCEL);
        }

        #endregion

        #region Clear controls

        private void Clear_controls(FORM_ACTION form_action) {

            if (form_action != FORM_ACTION.UPDATE)
            {
                Textbox_tcp_slave_name.Text = String.Empty;
                DecimalUpDown_UnitId.Value = 247;
                DecimalUpDown_port.Value = 502;

                Textbox_tcp_slave_ip_primary.firstBox.Text = string.Empty;
                Textbox_tcp_slave_ip_primary.secondBox.Text = string.Empty;
                Textbox_tcp_slave_ip_primary.thirdBox.Text = string.Empty;
                Textbox_tcp_slave_ip_primary.fourthBox.Text = string.Empty;
                m_current_slave_entry = null;
                m_current_var_entry = null;
            }

            Border_slave_list.IsEnabled = true;
            Wrap_edit_tcp_slave.IsEnabled = false;
            Border_mapped.IsEnabled = false;

            m_collection_var_entry.Clear();
            CollectionViewSource.GetDefaultView(Listview_var_map.ItemsSource).Refresh();
        }

        #endregion

        #endregion


        #region Var mapped

        #region Listview var map selection changed
        private void Listview_var_map_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Listview_var_map.SelectedItems.Count > 0)
            {
                if (Listview_var_map.SelectedItem is TCPModbusVarEntry)
                {
                    TCPModbusVarEntry selected_var_map = Listview_var_map.SelectedItem as TCPModbusVarEntry;
                    m_current_var_entry = m_current_slave_entry.List_var_entry.First(var_entry => var_entry.Name.Equals(selected_var_map.Name));
                }
            }
        }

        #endregion


        #region Listview double click

        private void Listview_var_map_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as ListView).SelectedItem;
            if (item != null)
            {
                TCPModbusVarEntry selected_var_entry = item as TCPModbusVarEntry;

                SettingVarMapWindow varMapWindow = new()
                {
                    Slave_entry = m_current_slave_entry,
                    Var_entry = selected_var_entry
                };

                if (varMapWindow.ShowDialog() == true)
                {
                    bool save_ok = Manage_file.Save_var_map_entries();
                    if (!save_ok)
                        MessageBox.Show("Error saving new config.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    
                    else
                    {
                        m_collection_var_entry.Clear();
                        m_current_slave_entry.List_var_entry.ForEach(var_entry => m_collection_var_entry.Add(var_entry));

                        m_collection_var_entry = new ObservableCollection<TCPModbusVarEntry>(m_collection_var_entry.OrderBy(modbus_var => modbus_var.DirModbus));
                        Listview_var_map.ItemsSource = m_collection_var_entry;
                        CollectionViewSource.GetDefaultView(Listview_var_map.ItemsSource).Refresh();
                    }

                }
            }
        }

        #endregion

        #region New var map

        private void Button_new_var_map_Click(object sender, RoutedEventArgs e)
        {
            if (m_current_slave_entry != null)
            {
                if (m_current_slave_entry.Slave_type == SLAVE_TYPE.TCU)
                    MessageBox.Show("Cannot define variables for TCU slave", "WARNING", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                
                else
                {
                    SettingVarMapWindow varMapWindow = new();
                    varMapWindow.Slave_entry = m_current_slave_entry;
                    varMapWindow.Var_entry = new() {
                        Slave = m_current_slave_entry.Name,
                        Graphic_pos = Constants.index_no_selected,
                        DirModbus = Constants.index_no_selected,
                        Send_to_samca_pos = Constants.index_no_selected.ToString(),
                        Link_to_send_tcu = (int) LINK_TO_SEND_TCU.NONE,
                        Link_to_avg = (int)LINK_TO_AVG.NONE,
                        SCS_record = true,
                        Fast_mode_record = false
                    };

                    if (varMapWindow.ShowDialog() == true)
                    {
                        m_current_slave_entry.List_var_entry.Add(varMapWindow.Var_entry);
                        bool save_ok = Manage_file.Save_var_map_entries();
                        if (!save_ok)
                        {
                            MessageBox.Show("Error saving new config.", "INFO", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                        }
                        else
                        {
                            m_collection_var_entry.Add(varMapWindow.Var_entry);
                            m_collection_var_entry = new ObservableCollection<TCPModbusVarEntry>(m_collection_var_entry.OrderBy(modbus_var => modbus_var.DirModbus));
                            Listview_var_map.ItemsSource = m_collection_var_entry;
                            CollectionViewSource.GetDefaultView(Listview_var_map.ItemsSource).Refresh();
                        }
                    }
                }
            }
        }

        #endregion


        #region Remove var map

        private void Button_remove_var_map_Click(object sender, RoutedEventArgs e)
        {
            if (m_current_slave_entry != null && m_current_var_entry != null)
            {
                m_current_slave_entry.List_var_entry.Remove(m_current_var_entry);
                bool save_ok = Manage_file.Save_var_map_entries();
                if (!save_ok)
                {
                    MessageBox.Show("Error deleting selected var.", "INFO", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                else
                {
                    m_collection_var_entry.Remove(m_current_var_entry);
                    Listview_var_map.Items.Refresh();

                    MessageBox.Show("Var. deleted", "INFO", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
        }

        #endregion

        #endregion
    }
}
