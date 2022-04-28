using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace SBP_TRACKER
{

    public partial class SelectVarMapWindow : Window
    {
        public int Selected_index { get; set; }
        public string Selected_var { get; set; }
        private List<string> m_list_var_map_schema = new();

        #region Constructor

        public SelectVarMapWindow()
        {
            InitializeComponent();
        }

        #endregion


        #region Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Label_selected_var.Content = Selected_var;

            Globals.GetTheInstance().List_modbus_slave_entry.ForEach(modbus_slave_entry =>
            {
                modbus_slave_entry.List_modbus_var.ForEach(modbus_slave_var => m_list_var_map_schema.Add(modbus_slave_var.Name));
            });

            Listview_schema_var_map.ItemsSource = m_list_var_map_schema;
            Listview_schema_var_map.Items.Refresh();


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


        #region Listview selection changed

        private void Listview_schema_var_map_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                if (Listview_schema_var_map.SelectedItems.Count > 0)
                    if (Listview_schema_var_map.SelectedItem is string mySelectedItem)
                        Label_selected_var.Content = mySelectedItem;
        }

        #endregion


        #region SAVE
        private void Button_save_Click(object sender, RoutedEventArgs e)
        {

            Selected_var = Label_selected_var.Content != null ? Label_selected_var.Content.ToString() : string.Empty;

            DialogResult = true;
        }

        #endregion

        #region Remove

        private void Button_remove_Click(object sender, RoutedEventArgs e)
        {
            Label_selected_var.Content = string.Empty;
        }

        #endregion




        #region EXIT

        private void Button_exit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }


        #endregion


    }
}
