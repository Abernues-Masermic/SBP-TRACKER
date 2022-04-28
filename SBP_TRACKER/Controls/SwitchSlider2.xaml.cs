
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace SBP_TRACKER.Controls
{

    public partial class SwitchSlider2 : UserControl
    {
        private bool Toggled = false;



        public static DependencyProperty LabelContentLeftProperty = DependencyProperty.Register("LabelContentLeft", typeof(string), typeof(SwitchSlider));

        public string LabelContentLeft
        {
            get { return (string)GetValue(LabelContentLeftProperty); }
            set {  SetValue(LabelContentLeftProperty, value); }
        }

        public static DependencyProperty LabelContentRightProperty = DependencyProperty.Register("LabelContentRight", typeof(string), typeof(SwitchSlider));

        public string LabelContentRight
        {
            get { return (string)GetValue(LabelContentRightProperty); }
            set { SetValue(LabelContentRightProperty, value); }
        }


        public SwitchSlider2()
        {
            InitializeComponent();
            Toggled = false;
            Switch_dot.HorizontalAlignment = HorizontalAlignment.Left;
            LabelLeft.Visibility = Visibility.Visible;
            LabelRight.Visibility = Visibility.Hidden;

        }



        public bool ToggledState { get => Toggled; set => Toggled = value; }


        public void IniState()
        {
            Toggled = false;
        }


        private void Switch_dot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Toggled)
            {
                Toggled = true;
                Switch_dot.HorizontalAlignment = HorizontalAlignment.Right;
                LabelLeft.Visibility = Visibility.Hidden;
                LabelRight.Visibility = Visibility.Visible;
            }
            else
            {
                Toggled = false;
                Switch_dot.HorizontalAlignment = HorizontalAlignment.Left;
                LabelLeft.Visibility = Visibility.Visible;
                LabelRight.Visibility = Visibility.Hidden;
            }
        }
    }
}
