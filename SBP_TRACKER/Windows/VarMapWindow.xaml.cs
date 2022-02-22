using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SBP_TRACKER
{
    /// <summary>
    /// Lógica de interacción para Report.xaml
    /// </summary>
    public partial class VarMapWindow : Window
    {
        public TCPModbusVar Modbus_mapped { get; set;}

        public int Schema_pos { get; set; }

        #region Constructor

        public VarMapWindow()
        {
            InitializeComponent();
        }

        #endregion


        #region Loaded

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Textbox_var_name.Text = Modbus_mapped.Name;
            DecimalUpDown_dir_var.Value = Modbus_mapped.Dir ;
            Combobox_var_type.Text = Modbus_mapped.SType;
            Schema_pos = Modbus_mapped.Schema_pos;
        }

        #endregion

        #region Save var map

        private void Button_save_var_map_Click(object sender, RoutedEventArgs e)
        {
            if (Textbox_var_name.Text == string.Empty || Combobox_var_type.SelectedIndex == Constants.index_no_selected)
            {
                MessageBox.Show("Check parameters", "Error save", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
            else
            {
                Modbus_mapped.Name = Textbox_var_name.Text;
                Modbus_mapped.Dir = (int)DecimalUpDown_dir_var.Value;
                Modbus_mapped.SType = Combobox_var_type.Text;
                Modbus_mapped.Type = DataConverter.String_to_type_code(Modbus_mapped.SType);
                Modbus_mapped.Schema_pos = Schema_pos;

               this.DialogResult = true;
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
