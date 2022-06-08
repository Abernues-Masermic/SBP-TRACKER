using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SBP_TRACKER
{

    public partial class BitMaskWindow : Window
    {
        public TCUCodifiedStatusEntry? TCU_codified_status_entry { get; set; }

        #region Constructor

        public BitMaskWindow()
        {
            InitializeComponent();
        }

        #endregion


        #region Loaded

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Label_var_name.Content = TCU_codified_status_entry.Name;
            Label_dir_modbus.Content = TCU_codified_status_entry.DirModbus;


            bool b_value = ushort.TryParse(TCU_codified_status_entry.Value, out ushort status_value);

            string s_value = string.Empty;
            if (b_value)
            {
                byte[] array_status_value = BitConverter.GetBytes(status_value);
                s_value = "0x" + array_status_value[1].ToString("X2") + " 0x" + array_status_value[0].ToString("X2");
            }
            Label_value.Content = s_value;


            int bit_index = 0;
            while (bit_index < 16)
            {
                bool is_enabled = (status_value & 1) == 1;
                Brush bit_state = is_enabled ? Brushes.DarkBlue : Brushes.GhostWhite;

                status_value = (ushort)(status_value >> 1);

                WrapPanel wrappanel = new()
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(5, 5, 5, 5)
                };

                Label label_index = new()
                {
                    Style = this.FindResource("Label_info") as Style,
                    Width = 20,
                    Content = bit_index
                };

                Label label_title = new()
                {
                    Style = this.FindResource("Label_info") as Style,
                    Width = 300,
                    Content = TCU_codified_status_entry.List_status_mask[bit_index]
                };

                Ellipse ellipse_state = new()
                {
                    Margin = new Thickness(5, 0, 0, 0),
                    Width = 20,
                    Height = 20,
                    Fill = bit_state,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                };

                wrappanel.Children.Add(label_index);
                wrappanel.Children.Add(label_title);
                wrappanel.Children.Add(ellipse_state);

                WrapPanel_bit_status.Children.Add(wrappanel);

                bit_index++;
            }
        }

        #endregion


        #region EXIT

        private void Button_ok_click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion
    }
}
