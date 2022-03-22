using System;
using System.Windows;


namespace SBP_TRACKER
{
    /// <summary>
    /// Lógica de interacción para Report.xaml
    /// </summary>
    public partial class VarMapWindow : Window
    {
        public TCPModbusVarEntry Var_entry { get; set;}

        public TCPModbusSlaveEntry Slave_entry { get; set; }

        public int Schema_pos { get; set; }




        #region Constructor

        public VarMapWindow()
        {
            InitializeComponent();

            Combobox_emet_watchdog.ItemsSource = Enum.GetNames(typeof(EMET_WATCHDOG_ASSOC));
        }

        #endregion


        #region Loaded

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Textbox_var_name.Text = Var_entry.Name;
            Textbox_var_desc.Text = Var_entry.Description;
            DecimalUpDown_dir_var.Value = Var_entry.DirModbus;
            Combobox_var_type.Text = DataConverter.Type_code_to_string (Var_entry.TypeVar);
            Schema_pos = Var_entry.Schema_pos;
            DecimalUpDown_read_range_min.Value = Var_entry.Read_range_min;
            DecimalUpDown_read_range_max.Value = Var_entry.Read_range_max;
            DecimalUpDown_scaled_range_min.Value = (decimal)Var_entry.Scaled_range_min;
            DecimalUpDown_scaled_range_max.Value = (decimal)Var_entry.Scaled_range_max;
            Textbox_unit.Text = Var_entry.Unit;
            Combobox_emet_watchdog.SelectedIndex = (int)Var_entry.Watchdog_assoc;
        }

        #endregion

        #region Save var map

        private void Button_save_var_entry_Click(object sender, RoutedEventArgs e)
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
                Globals.GetTheInstance().List_modbus_slave_entry.ForEach(entry =>
                {
                    if (!name_duplicated)
                        name_duplicated = entry.List_modbus_var.Exists(modbus_var => modbus_var.Name.Equals(Textbox_var_name.Text) && !modbus_var.Name.Equals(Var_entry.Name));
                });

                if (name_duplicated)
                {
                    save_ok = false;
                    MessageBox.Show("Var name is already defined in other var.", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
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
                if (Slave_entry.List_modbus_var.Exists(modbus_var => modbus_var.DirModbus == (int)DecimalUpDown_dir_var.Value && Var_entry.DirModbus != (int)DecimalUpDown_dir_var.Value))
                {
                    save_ok = false;
                    MessageBox.Show("@ Modbus already selected in other var ", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }

            if (save_ok)
            {
                Var_entry.Name = Textbox_var_name.Text;
                Var_entry.Description = Textbox_var_desc.Text;
                Var_entry.DirModbus = (int)DecimalUpDown_dir_var.Value;
                Var_entry.TypeVar = DataConverter.String_to_type_code(Combobox_var_type.Text);
                Var_entry.Schema_pos = Schema_pos;
                Var_entry.Read_range_min = (int)DecimalUpDown_read_range_min.Value;
                Var_entry.Read_range_max = (int)DecimalUpDown_read_range_max.Value;
                Var_entry.Read_range_grid = DecimalUpDown_read_range_min.Value.ToString() + "  -  " + DecimalUpDown_read_range_max.Value.ToString();
                Var_entry.Scaled_range_min = (float)DecimalUpDown_scaled_range_min.Value;
                Var_entry.Scaled_range_max = (float)DecimalUpDown_scaled_range_max.Value;
                Var_entry.Scaled_range_grid = DecimalUpDown_scaled_range_min.Value.ToString() + "  -  " + DecimalUpDown_scaled_range_max.Value.ToString();
                Var_entry.Unit = Textbox_unit.Text;
                Var_entry.Watchdog_assoc = Combobox_emet_watchdog.SelectedIndex;

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
