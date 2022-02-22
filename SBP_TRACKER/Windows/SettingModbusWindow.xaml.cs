using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CsvHelper;
using CsvHelper.Configuration;


namespace SBP_TRACKER
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SettingModbusWindow : Window
    {
        private FORM_ACTION m_action;

        private List<string> m_list_modbus_entry = new();
        private ObservableCollection<TCPModbusVar> m_collection_var_map;

        private TCPModbusSlaveEntry m_current_entry;
        private TCPModbusVar m_current_var_map;


        #region Constructor

        public SettingModbusWindow()
        {
            InitializeComponent();

            m_collection_var_map = new ObservableCollection<TCPModbusVar>();
            Listview_var_map.ItemsSource = m_collection_var_map;
        }

        #endregion


        #region Loaded

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Wrap_edit_tcp_slave.IsEnabled = false;
            Border_mapped.IsEnabled = false;

            Globals.GetTheInstance().List_modbus_slave_entry.ForEach(modbus_slave_entry =>  m_list_modbus_entry.Add(modbus_slave_entry.Name));
            Listview_tcp_slave.ItemsSource = m_list_modbus_entry;
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

        #region Listview selection changed
        private void Listview_tcp_slave_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Listview_tcp_slave.SelectedItems.Count > 0)
            {
                if (Listview_tcp_slave.SelectedItem is string mySelectedItem)
                {
                    m_current_entry = Globals.GetTheInstance().List_modbus_slave_entry.First(x => x.Name == mySelectedItem);
                    Textbox_tcp_slave_name.Text = m_current_entry.Name;


                    #region IP primary

                    Textbox_tcp_slave_ip_primary.firstBox.Text = String.Empty;
                    Textbox_tcp_slave_ip_primary.secondBox.Text = String.Empty;
                    Textbox_tcp_slave_ip_primary.thirdBox.Text = String.Empty;
                    Textbox_tcp_slave_ip_primary.fourthBox.Text = String.Empty;

                    byte pos_box = 0;
                    IEnumerable<char> enum_tcp_primary = m_current_entry.IP_primary.Take(m_current_entry.IP_primary.Length);
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


                    DecimalUpDown_UnitId.Value = m_current_entry.UnitId;
                    DecimalUpDown_port.Value = m_current_entry.Port;

                    #region Var map list

                    m_collection_var_map.Clear();
                    m_current_entry.List_modbus_var.ForEach(var_map => m_collection_var_map.Add(var_map));
                
                    CollectionViewSource.GetDefaultView(Listview_var_map.ItemsSource).Refresh();
                    Listview_var_map.ItemsSource = m_collection_var_map;

                    #endregion
                }
            }
        }

        #endregion

        #region Listview double click
        private void Listview_tcp_slave_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Wrap_edit_tcp_slave.IsEnabled = true;
            Border_mapped.IsEnabled = true;
            m_action = FORM_ACTION.UPDATE;

            #region Bind data to list var map

            m_collection_var_map.Clear();
            m_current_entry.List_modbus_var.ForEach(x => m_collection_var_map.Add(x));
            CollectionViewSource.GetDefaultView(Listview_var_map.ItemsSource).Refresh();
            ListCollectionView view = (ListCollectionView)CollectionViewSource.GetDefaultView(m_collection_var_map);
            Listview_var_map.ItemsSource = m_collection_var_map;

            #endregion
        }


        #endregion

        #region New TCP slave

        private void Button_new_tcp_slave_Click(object sender, RoutedEventArgs e)
        {
            m_current_entry = new TCPModbusSlaveEntry();

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

            m_collection_var_map.Clear();
            CollectionViewSource.GetDefaultView(Listview_var_map.ItemsSource).Refresh();
        }

        #endregion

        #region Remove TCP slave

        private void Button_remove_tcp_slave_Click(object sender, RoutedEventArgs e)
        {
            m_action = FORM_ACTION.REMOVE;
            if (m_current_entry != null)
            {
                Globals.GetTheInstance().List_modbus_slave_entry.Remove(m_current_entry);

                bool save_slave_ok =  Manage_file.Save_modbus_slave_entries();
                bool save_var_map_ok = Manage_file.Save_var_map_entries();
                if (!save_slave_ok || !save_var_map_ok)
                {
                    MessageBox.Show("Error deleting slave", "Error saving", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                else
                {
                    //Refresh entry list
                    m_list_modbus_entry.Clear();
                    Globals.GetTheInstance().List_modbus_slave_entry.ForEach(entry => m_list_modbus_entry.Add(entry.Name));
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

            #endregion

            if (!save)
            {
                MessageBox.Show("Check parameters", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
            else
            {
                string modbus_slave_csv = AppDomain.CurrentDomain.BaseDirectory + @"SettingModbusSlave\modbusSlave.csv";

                if (m_action == FORM_ACTION.CREATE)
                {
                    m_current_entry.Name = Textbox_tcp_slave_name.Text;
                    m_current_entry.UnitId = (byte)DecimalUpDown_UnitId.Value;
                    m_current_entry.IP_primary = Textbox_tcp_slave_ip_primary.firstBox.Text + "." + Textbox_tcp_slave_ip_primary.secondBox.Text + "." + Textbox_tcp_slave_ip_primary.thirdBox.Text + "." + Textbox_tcp_slave_ip_primary.fourthBox.Text;
                    m_current_entry.Port = (int)DecimalUpDown_port.Value;
                    m_current_entry.List_modbus_var = new List<TCPModbusVar>();

                    Globals.GetTheInstance().List_modbus_slave_entry.Add(m_current_entry);
                }

                else if (m_action == FORM_ACTION.UPDATE)
                {
                    Globals.GetTheInstance().List_modbus_slave_entry.Where(c => c.Name == m_current_entry.Name).Select(c =>
                    {
                        c.Name = Textbox_tcp_slave_name.Text;
                        c.UnitId = (byte)DecimalUpDown_UnitId.Value;
                        c.IP_primary = Textbox_tcp_slave_ip_primary.firstBox.Text + "." + Textbox_tcp_slave_ip_primary.secondBox.Text + "." + Textbox_tcp_slave_ip_primary.thirdBox.Text + "." + Textbox_tcp_slave_ip_primary.fourthBox.Text;
                        c.Port = (int)DecimalUpDown_port.Value;
                        return c;
                    }).ToList();
                }
 
                bool save_slave_ok = Manage_file.Save_modbus_slave_entries();
                if (!save_slave_ok)
                {
                    MessageBox.Show("Error saving system config", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                else
                {
                    //Refresh list
                    m_list_modbus_entry.Clear();
                    Globals.GetTheInstance().List_modbus_slave_entry.ForEach(entry => m_list_modbus_entry.Add(entry.Name));
                    Listview_tcp_slave.Items.Refresh();

                    Clear_controls(FORM_ACTION.UPDATE);

                    MessageBox.Show("Parameters saved", "INFO", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
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
                m_current_entry = null;
                m_current_var_map = null;
            }

            Border_slave_list.IsEnabled = true;
            Wrap_edit_tcp_slave.IsEnabled = false;
            Border_mapped.IsEnabled = false;

            m_collection_var_map.Clear();
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
                if (Listview_var_map.SelectedItem is TCPModbusVar)
                {
                    TCPModbusVar selected_var_map = Listview_var_map.SelectedItem as TCPModbusVar;
                    m_current_var_map = m_current_entry.List_modbus_var.First(x => x.Name == selected_var_map.Name);
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
                TCPModbusVar selected_var_map = item as TCPModbusVar;

                VarMapWindow varMapWindow = new();
                varMapWindow.Modbus_mapped = selected_var_map;
                if (varMapWindow.ShowDialog() == true)
                {
                    bool save_ok = Manage_file.Save_var_map_entries();
                    if (!save_ok)
                    {
                        MessageBox.Show("Error saving new config.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                    else
                    {
                        #region Var map list

                        m_collection_var_map.Clear();
                        m_current_entry.List_modbus_var.ForEach(var_map => m_collection_var_map.Add(var_map));

                        CollectionViewSource.GetDefaultView(Listview_var_map.ItemsSource).Refresh();
                        Listview_var_map.ItemsSource = m_collection_var_map;

                        #endregion

                        MessageBox.Show("Var. map config changed", "INFO", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }

                }
            }
        }

        #endregion

        #region New var map

        private void Button_new_var_map_Click(object sender, RoutedEventArgs e)
        {
            VarMapWindow varMapWindow = new();
            varMapWindow.Modbus_mapped = new() { Slave = m_current_entry.Name, Schema_pos = Constants.index_no_selected };
            if (varMapWindow.ShowDialog() == true)
            {
                m_current_entry.List_modbus_var.Add(varMapWindow.Modbus_mapped);
                bool save_ok = Manage_file.Save_var_map_entries();
                if (!save_ok)
                {
                    MessageBox.Show("Error saving new config.", "INFO", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                else
                {
                    m_collection_var_map.Add(varMapWindow.Modbus_mapped);
                    CollectionViewSource.GetDefaultView(Listview_var_map.ItemsSource).Refresh();
                    Listview_var_map.ItemsSource = m_collection_var_map;

                    MessageBox.Show("Parameters saved", "INFO", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
        }


        #endregion

        #region Remove var map

        private void Button_remove_var_map_Click(object sender, RoutedEventArgs e)
        {
            if (m_current_entry != null)
            {
                m_current_entry.List_modbus_var.Remove(m_current_var_map);
                bool save_ok = Manage_file.Save_var_map_entries();
                if (!save_ok)
                {
                    MessageBox.Show("Error deleting selected var.", "INFO", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                else
                {
                    m_collection_var_map.Remove(m_current_var_map);
                    Listview_var_map.Items.Refresh();

                    MessageBox.Show("Var. deleted", "INFO", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
        }

        #endregion

        #endregion
    }
}
