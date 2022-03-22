
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace SBP_TRACKER.Controls
{

    public partial class SwitchSlider : UserControl
    {

        Thickness LeftSide = new Thickness(-71, 0, 0, 0);
        Thickness RightSide = new Thickness(0, 0, -71, 0);
        SolidColorBrush Mode1 = new SolidColorBrush(Color.FromRgb(22, 105, 169));
        SolidColorBrush Mode2 = new SolidColorBrush(Color.FromRgb(169, 123, 22));
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


        public SwitchSlider()
        {
            InitializeComponent();
            Back.Fill = Mode1;
            Toggled = false;
            Dot.Margin = LeftSide;
            LabelLeft.Visibility = Visibility.Visible;
            LabelRight.Visibility = Visibility.Hidden;
        }



        public bool ToggledState { get => Toggled; set => Toggled = value; }


        public void IniState()
        {
            LabelLeft.Visibility = Visibility.Visible;
            LabelRight.Visibility = Visibility.Hidden;
            Back.Fill = Mode1;
            Toggled = false;
            Dot.Margin = LeftSide;
        }


        private void Dot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Toggled)
            {
                LabelLeft.Visibility = Visibility.Hidden;
                LabelRight.Visibility = Visibility.Visible;
                Back.Fill = Mode2;
                Toggled = true;
                Dot.Margin = RightSide;

            }
            else
            {
                LabelLeft.Visibility = Visibility.Visible;
                LabelRight.Visibility = Visibility.Hidden;
                Back.Fill = Mode1;
                Toggled = false;
                Dot.Margin = LeftSide;
            }




        }

        private void Back_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Toggled)
            {
                LabelLeft.Visibility = Visibility.Hidden;
                LabelRight.Visibility = Visibility.Visible;
                Back.Fill = Mode2;
                Toggled = true;
                Dot.Margin = RightSide;

            }
            else
            {
                LabelLeft.Visibility = Visibility.Visible;
                LabelRight.Visibility = Visibility.Hidden;
                Back.Fill = Mode1;
                Toggled = false;
                Dot.Margin = LeftSide;

            }

        }
    }
}
