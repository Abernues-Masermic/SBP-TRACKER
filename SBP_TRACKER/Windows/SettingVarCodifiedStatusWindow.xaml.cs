using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Effects;

namespace SBP_TRACKER
{
    public partial class SettingVarCodifiedStatusWindow : Window
    {

        public TCPModbusSlaveEntry? Slave_entry { get; set; }
        public TCUCodifiedStatusEntry? TCU_codified_status_entry { get; set; }

        private List<string> m_list_bit_mask_value = new();

        #region Constructor

        public SettingVarCodifiedStatusWindow()
        {
            InitializeComponent();

            Combobox_link_to_grahic.ItemsSource = Enum.GetNames(typeof(LINK_TO_GRAPHIC));
        }

        #endregion


        #region Loaded

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Textbox_var_name.Text = TCU_codified_status_entry.Name;
            DecimalUpDown_dir_modbus.Value = TCU_codified_status_entry.DirModbus;
            Combobox_var_type.Text = DataConverter.Type_code_to_string( TCU_codified_status_entry.TypeVar);
            Textbox_unit.Text = TCU_codified_status_entry.Unit;
            Checkbox_status_mask.IsChecked = TCU_codified_status_entry.Status_mask_enable;
            Checkbox_scs_record.IsChecked = TCU_codified_status_entry.SCS_record;

            m_list_bit_mask_value = TCU_codified_status_entry.List_status_mask;

            Combobox_link_to_grahic.SelectedValue = Enum.GetName(typeof(LINK_TO_GRAPHIC), TCU_codified_status_entry.Link_to_graphic);
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



        #region Define mask

        private void Button_define_mask_Click(object sender, RoutedEventArgs e)
        {
            BlurEffect blurEffect = new()
            {
                Radius = 3
            };

            Effect = blurEffect;

            DefineMaskWindow mask_window = new(Textbox_var_name.Text, DecimalUpDown_dir_modbus.Value.ToString(), m_list_bit_mask_value);
            mask_window.ShowDialog();
            if (mask_window.DialogResult == true)
                m_list_bit_mask_value = mask_window.List_bit_mask_value;
            

            Effect = null;
        }

        #endregion


        #region Save event
        private void Button_save_state_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        #endregion

        #region Save action

        private void Save() {
            bool continue_save = true;
            if (Textbox_var_name.Text == string.Empty || Combobox_var_type.SelectedIndex == Constants.index_no_selected)
            {
                MessageBox.Show("Check parameters", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                continue_save = false;
            }

            if (continue_save)
            {
                bool name_duplicated = false;
                Globals.GetTheInstance().List_tcu_codified_status.ForEach(codified_status =>
                {
                    if (!name_duplicated)
                        name_duplicated = codified_status.Name.Equals(Textbox_var_name.Text) && !codified_status.Name.Equals(TCU_codified_status_entry.Name);
                });

                if (name_duplicated)
                {
                    MessageBox.Show("TCU encode var name is already defined in other var.", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    continue_save = false;
                }
            }

            if (continue_save)
            {
                if (Slave_entry != null)
                {
                    if (DecimalUpDown_dir_modbus.Value >= Slave_entry.Read_reg)
                    {
                        continue_save = false;
                        MessageBox.Show("@ Modbus cannot be higher than Slave read num data", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            }

            if (continue_save)
            {
                if (Slave_entry != null)
                {
                    if (Slave_entry.List_modbus_var.Exists(modbus_var => (modbus_var.DirModbus == (int)DecimalUpDown_dir_modbus.Value) && (TCU_codified_status_entry.DirModbus != (int)DecimalUpDown_dir_modbus.Value)))
                    {
                        continue_save = false;
                        MessageBox.Show("@ Modbus already selected in other var associated with slave  ", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            }
            if (continue_save)
            {
                if (DecimalUpDown_dir_modbus.Value < 0)
                {
                    continue_save = false;
                    MessageBox.Show("@ Modbus cannot be less than 0", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }


            if (continue_save)
            {
                TCU_codified_status_entry.Name = Textbox_var_name.Text;
                TCU_codified_status_entry.DirModbus = (int)DecimalUpDown_dir_modbus.Value;
                TCU_codified_status_entry.TypeVar = DataConverter.String_to_type_code(Combobox_var_type.Text);
                TCU_codified_status_entry.Unit = Textbox_unit.Text;
                TCU_codified_status_entry.Status_mask_enable = (bool)Checkbox_status_mask.IsChecked;

                TCU_codified_status_entry.List_status_mask = TCU_codified_status_entry.Status_mask_enable ? m_list_bit_mask_value : new List<string>();

                LINK_TO_GRAPHIC link_to_graphic = LINK_TO_GRAPHIC.NONE;
                if (Enum.TryParse(Combobox_link_to_grahic.SelectedValue.ToString(), out link_to_graphic))
                    TCU_codified_status_entry.Link_to_graphic = (int)link_to_graphic;

                TCU_codified_status_entry.SCS_record = (bool)Checkbox_scs_record.IsChecked;

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
