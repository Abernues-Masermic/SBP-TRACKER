using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace SBP_TRACKER
{
    /// <summary>
    /// Lógica de interacción para Report.xaml
    /// </summary>
    public partial class SettingVarMapWindow : Window
    {
        public TCPModbusVarEntry Var_entry { get; set;}

        public TCPModbusSlaveEntry Slave_entry { get; set; }

        public int Graphic_pos { get; set; }




        #region Constructor

        public SettingVarMapWindow()
        {
            InitializeComponent();

            DecimalUpDown_send_to_samca_pos.MaxValue = Constants.WR_SAMCA_REG_SIZE -1;

            Combobox_var_linked.ItemsSource = Enum.GetNames(typeof(LINK_TO_SEND_TCU));
            Combobox_avg_linked.ItemsSource = Enum.GetNames(typeof(LINK_TO_AVG));
        }

        #endregion



        #region Using enter key

        public void Enter_executed(object sender, ExecutedRoutedEventArgs e)
        {
            Save();
        }

        public void Enter_enable(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        #endregion



        #region Loaded

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Textbox_var_name.Text = Var_entry.Name;
            Textbox_var_desc.Text = Var_entry.Description;
            DecimalUpDown_dir_modbus.Value = Var_entry.DirModbus;
            Combobox_var_type.Text = DataConverter.Type_code_to_string (Var_entry.TypeVar);
            Graphic_pos = Var_entry.Graphic_pos;
            DecimalUpDown_read_range_min.Value = Var_entry.Read_range_min;
            DecimalUpDown_read_range_max.Value = Var_entry.Read_range_max;
            DecimalUpDown_scaled_range_min.Value = (decimal)Var_entry.Scaled_range_min;
            DecimalUpDown_scaled_range_max.Value = (decimal)Var_entry.Scaled_range_max;
            DecimalUpDown_offset.Value = (decimal)Var_entry.Offset;
            Textbox_unit.Text = Var_entry.Unit;
            Combobox_var_linked.SelectedValue = Enum.GetName(typeof(LINK_TO_SEND_TCU), Var_entry.Link_to_send_tcu);
            Combobox_avg_linked.SelectedValue = Enum.GetName(typeof(LINK_TO_AVG), Var_entry.Link_to_avg);

            Checkbox_correction_load_pin.IsChecked = Var_entry.Correction_load_pin;
            Checkbox_scs_record.IsChecked = Var_entry.SCS_record;
            Checkbox_fast_mode_record.IsChecked = Var_entry.Fast_mode_record;
            Checkbox_samca_record.IsChecked = Var_entry.SAMCA_record;
            DecimalUpDown_send_to_samca_pos.Value = Var_entry.Send_to_samca_pos == String.Empty ? Constants.index_no_selected : decimal.Parse(Var_entry.Send_to_samca_pos);       
        }

        #endregion

        #region Save event

        private void Button_save_var_entry_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        #endregion


        #region Save action

        private void Save()
        {
            bool save_ok = true;

            if (Textbox_var_name.Text == string.Empty || Combobox_var_type.SelectedIndex == Constants.index_no_selected)
            {
                save_ok = false;
                MessageBox.Show("Check parameters", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
            if (save_ok)
            {
                bool name_duplicated = false;
                Globals.GetTheInstance().List_slave_entry.ForEach(slave_entry =>
                {
                    if (!name_duplicated)
                        name_duplicated = slave_entry.List_var_entry.Exists(modbus_var => modbus_var.Name.Equals(Textbox_var_name.Text) && !modbus_var.Name.Equals(Var_entry.Name));
                });

                if (name_duplicated)
                {
                    save_ok = false;
                    MessageBox.Show("Var name is already defined in other var.", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            if (save_ok)
            {
                if (DecimalUpDown_dir_modbus.Value >= Slave_entry.Read_reg)
                {
                    save_ok = false;
                    MessageBox.Show("@ Modbus cannot be higher than slave read num data", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            if (save_ok)
            {
                if (DecimalUpDown_dir_modbus.Value < 0)
                {
                    save_ok = false;
                    MessageBox.Show("@ Modbus cannot be less than 0", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            if (save_ok)
            {
                if (
                    DecimalUpDown_read_range_min.Value >= DecimalUpDown_read_range_max.Value ||
                    DecimalUpDown_scaled_range_min.Value >= DecimalUpDown_scaled_range_max.Value)
                {
                    save_ok = false;
                    MessageBox.Show("Error in range definition values", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            if (save_ok)
            {
                if (Slave_entry.List_var_entry.Exists(var_entry => (var_entry.DirModbus == (int)DecimalUpDown_dir_modbus.Value) && (Var_entry.DirModbus != (int)DecimalUpDown_dir_modbus.Value)))
                {
                    save_ok = false;
                    MessageBox.Show("@ Modbus already selected in other var associated with slave  ", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            if (save_ok)
            {
                if (DecimalUpDown_send_to_samca_pos.Value != Constants.index_no_selected)
                {
                    List<TCPModbusVarEntry> list_all_var_entry = new();
                    Globals.GetTheInstance().List_slave_entry.ForEach(slave_entry =>   list_all_var_entry.AddRange(slave_entry.List_var_entry));

                    if (list_all_var_entry.Exists(var_entry => (var_entry.Send_to_samca_pos == DecimalUpDown_send_to_samca_pos.Value.ToString()) && (var_entry.DirModbus != Var_entry.DirModbus)))
                    {
                        save_ok = false;
                        MessageBox.Show("SEND TO SAMCA @ modbus already selected in other var entry", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                    if (Globals.GetTheInstance().List_tcu_codified_status.Exists(codified_status => codified_status.Send_to_samca_pos == DecimalUpDown_send_to_samca_pos.Value.ToString()))
                    {
                        save_ok = false;
                        MessageBox.Show("SEND TO SAMCA @ modbus already selected in other codified status var", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            }
            if (save_ok)
            {
                Var_entry.Name = Textbox_var_name.Text;
                Var_entry.Description = Textbox_var_desc.Text;
                Var_entry.DirModbus = (int)DecimalUpDown_dir_modbus.Value;
                Var_entry.TypeVar = DataConverter.String_to_type_code(Combobox_var_type.Text);
                Var_entry.Graphic_pos = Graphic_pos;
                Var_entry.Read_range_min = (int)DecimalUpDown_read_range_min.Value;
                Var_entry.Read_range_max = (int)DecimalUpDown_read_range_max.Value;
                Var_entry.Read_range_grid = DecimalUpDown_read_range_min.Value.ToString() + "  -  " + DecimalUpDown_read_range_max.Value.ToString();
                Var_entry.Scaled_range_min = (double)DecimalUpDown_scaled_range_min.Value;
                Var_entry.Scaled_range_max = (double)DecimalUpDown_scaled_range_max.Value;
                Var_entry.Scaled_range_grid = DecimalUpDown_scaled_range_min.Value.ToString() + "  -  " + DecimalUpDown_scaled_range_max.Value.ToString();
                Var_entry.Offset = (double)DecimalUpDown_offset.Value;
                Var_entry.Unit = Textbox_unit.Text;

                LINK_TO_SEND_TCU link_to_send = LINK_TO_SEND_TCU.NONE;
                if (Enum.TryParse(Combobox_var_linked.SelectedValue.ToString(), out link_to_send))
                    Var_entry.Link_to_send_tcu = (int)link_to_send;

                LINK_TO_AVG link_to_avg = LINK_TO_AVG.NONE;
                if (Enum.TryParse(Combobox_avg_linked.SelectedValue.ToString(), out link_to_avg))
                    Var_entry.Link_to_avg = (int)link_to_avg;

                Var_entry.Correction_load_pin = (bool)Checkbox_correction_load_pin.IsChecked;
                Var_entry.SCS_record = (bool)Checkbox_scs_record.IsChecked;
                Var_entry.Fast_mode_record = (bool)Checkbox_fast_mode_record.IsChecked;
                Var_entry.SAMCA_record = (bool)Checkbox_samca_record.IsChecked;

                Var_entry.Send_to_samca_pos = DecimalUpDown_send_to_samca_pos.Value == Constants.index_no_selected ? String.Empty : DecimalUpDown_send_to_samca_pos.Value.ToString();

                Var_entry.Scale_factor = Functions.Calculate_scale_factor(Var_entry);

                this.DialogResult = true;
                this.Close();
            }
        }

        #endregion


        #region Exit
        private void Button_exit_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        #endregion
    }
}
