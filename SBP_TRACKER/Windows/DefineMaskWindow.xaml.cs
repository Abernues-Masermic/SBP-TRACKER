using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SBP_TRACKER
{

    public partial class DefineMaskWindow : Window
    {
        #region Array controles

        private List<TextBox> m_list_bit_mask_textbox = new();
        public List<string> List_bit_mask_value { get; set; }

        #endregion


        #region Constructor

        public DefineMaskWindow(string codifiedVarName, string modbusDir, List<string> list_bit_mask_value)
        {
            InitializeComponent();

            Label_name_value.Content = codifiedVarName;
            Label_dir_modbus_value.Content = modbusDir;

            List_bit_mask_value = new List<string>();
            m_list_bit_mask_textbox = new List<TextBox> 
            {
                Textbox_bit0,
                Textbox_bit1,
                Textbox_bit2,
                Textbox_bit3,
                Textbox_bit4,
                Textbox_bit5,
                Textbox_bit6,
                Textbox_bit7,
                Textbox_bit8,
                Textbox_bit9,
                Textbox_bit10,
                Textbox_bit11,
                Textbox_bit12,
                Textbox_bit13,
                Textbox_bit14,
                Textbox_bit15,
            };

            if (list_bit_mask_value.Count != 0)
            {
                list_bit_mask_value.Select((value, index) => new { Value = value, Position = index }).ToList()
                    .ForEach(bit_mask => m_list_bit_mask_textbox[bit_mask.Position].Text = bit_mask.Value);
            }
            else
            {
                m_list_bit_mask_textbox.ForEach(bit_textbox => bit_textbox.Text = "NOT USED");
            }
        }

        #endregion


        #region Loaded

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

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



        #region OK

        private void Button_ok_click(object sender, RoutedEventArgs e)
        {
            m_list_bit_mask_textbox.Select((item, index) => new { Item = item, Position = index }).ToList()
                .ForEach(bit_textbox => List_bit_mask_value.Add(bit_textbox.Item.Text));

            this.DialogResult = true;
            this.Close();
        }

        #endregion

        #region Cancel

        private void Button_cancel_click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        #endregion
    }
}
