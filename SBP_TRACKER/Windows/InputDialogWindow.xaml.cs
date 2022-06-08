using System.Windows;

namespace SBP_TRACKER
{

    public partial class InputDialogWindow : Window
    {
        public string Input_info { get; set; }
        public string Input_value { get; set; }

        #region Constructor

        public InputDialogWindow()
        {
            InitializeComponent();
        }

        #endregion


        #region Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Label_input.Content = Input_info;
        }

        #endregion

        #region Save

        private void Button_save_Click(object sender, RoutedEventArgs e)
        {
            Input_value = Textbox_input.Text;
            DialogResult = true;
        }

        #endregion

        #region Cancel

        private void Button_cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion



    }
}
