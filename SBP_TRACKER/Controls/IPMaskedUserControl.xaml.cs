using System;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace SBP_TRACKER_Controls
{
    public partial class IPMaskedUserControl : UserControl
    {
        #region Class variables and properties

        #region public variables and properties

        public TextBox FirstBox { get { return firstBox; } }
        public TextBox SecondBox { get { return secondBox; } }
        public TextBox ThirdBox { get { return thirdBox; } }
        public TextBox FourthBox { get { return fourthBox; } }

        #endregion

        #region private variables and properties

        private const string errorMessage = "Please specify a value between 0 and 255.";

        #endregion

        #endregion


        #region Constructor

        public IPMaskedUserControl()
        {
            InitializeComponent();
        }

        public IPMaskedUserControl(byte[] bytesToFill)
        {
            InitializeComponent();

            firstBox.Text = Convert.ToString(bytesToFill[0]);
            secondBox.Text = Convert.ToString(bytesToFill[1]);
            thirdBox.Text = Convert.ToString(bytesToFill[2]);
            fourthBox.Text = Convert.ToString(bytesToFill[3]);
        }

        #endregion


        #region Methods

        #region public methods
        public byte[] GetByteArray()
        {
            byte[] userInput = new byte[4];

            userInput[0] = Convert.ToByte(firstBox.Text);
            userInput[1] = Convert.ToByte(secondBox.Text);
            userInput[2] = Convert.ToByte(thirdBox.Text);
            userInput[3] = Convert.ToByte(fourthBox.Text);

            return userInput;
        }
        #endregion

        #region private methods
        private void JumpRight(TextBox rightNeighborBox, KeyEventArgs e)
        {
            rightNeighborBox.Focus();
            rightNeighborBox.CaretIndex = 0;
            e.Handled = true;
        }

        private void JumpLeft(TextBox leftNeighborBox, KeyEventArgs e)
        {
            leftNeighborBox.Focus();
            if (leftNeighborBox.Text != "")
            {
                leftNeighborBox.CaretIndex = leftNeighborBox.Text.Length;
            }
            e.Handled = true;
        }

        //checks for backspace, arrow and decimal key presses and jumps boxes if needed.
        //returns true when key was matched, false if not.
        private bool CheckJumpRight(TextBox currentBox, TextBox rightNeighborBox, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Right:
                    if (currentBox.CaretIndex == currentBox.Text.Length || currentBox.Text == "")
                    {
                        JumpRight(rightNeighborBox, e);
                    }
                    return true;
                case Key.OemPeriod:
                case Key.Decimal:
                case Key.Space:
                    JumpRight(rightNeighborBox, e);
                    rightNeighborBox.SelectAll();
                    return true;
                default:
                    return false;
            }
        }

        private bool CheckJumpLeft(TextBox currentBox, TextBox leftNeighborBox, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    if (currentBox.CaretIndex == 0 || currentBox.Text == "")
                    {
                        JumpLeft(leftNeighborBox, e);
                    }
                    return true;
                case Key.Back:
                    if ((currentBox.CaretIndex == 0 || currentBox.Text == "") && currentBox.SelectionLength == 0)
                    {
                        JumpLeft(leftNeighborBox, e);
                    }
                    return true;
                default:
                    return false;
            }
        }

        //discards non digits, prepares IPMaskedBox for textchange.
        private void HandleTextInput(TextBox currentBox, TextBox rightNeighborBox, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(Convert.ToChar(e.Text)))
            {
                e.Handled = true;
                SystemSounds.Beep.Play();
                return;
            }

            if (currentBox.Text.Length == 3 && currentBox.SelectionLength == 0)
            {
                e.Handled = true;
                SystemSounds.Beep.Play();
                if (currentBox != fourthBox)
                {
                    rightNeighborBox.Focus();
                    rightNeighborBox.SelectAll();
                }
            }
        }

        //checks whether textbox content > 255 when 3 characters have been entered.
        //clears if > 255, switches to next textbox otherwise 
        private void HandleTextChange(TextBox currentBox, TextBox rightNeighborBox)
        {
            if (currentBox.Text.Length == 3)
            {
                try
                {
                    Convert.ToByte(currentBox.Text);

                }
                catch (Exception)
                {
                    currentBox.Clear();
                    currentBox.Focus();
                    SystemSounds.Beep.Play();
                    MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (currentBox.CaretIndex != 2 && currentBox != fourthBox)
                {
                    rightNeighborBox.CaretIndex = rightNeighborBox.Text.Length;
                    rightNeighborBox.SelectAll();
                    rightNeighborBox.Focus();
                }
            }
        }
        #endregion

        #endregion


        #region Events
        //jump right, left or stay. 
        private void FirstByte_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            CheckJumpRight(firstBox, secondBox, e);
        }

        private void SecondByte_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (CheckJumpRight(secondBox, thirdBox, e))
                return;

            CheckJumpLeft(secondBox, firstBox, e);
        }

        private void ThirdByte_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (CheckJumpRight(thirdBox, fourthBox, e))
                return;

            CheckJumpLeft(thirdBox, secondBox, e);
        }

        private void FourthByte_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            CheckJumpLeft(fourthBox, thirdBox, e);

            if (e.Key == Key.Space)
            {
                SystemSounds.Beep.Play();
                e.Handled = true;
            }
        }


        //discards non digits, prepares IPMaskedBox for textchange.
        private void FirstByte_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HandleTextInput(firstBox, secondBox, e);
        }

        private void SecondByte_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HandleTextInput(secondBox, thirdBox, e);
        }

        private void ThirdByte_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HandleTextInput(thirdBox, fourthBox, e);
        }

        private void FourthByte_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HandleTextInput(fourthBox, fourthBox, e); //pass fourthbyte twice because no right neighboring box.
        }


        //checks whether textbox content > 255 when 3 characters have been entered.
        //clears if > 255, switches to next textbox otherwise 
        private void FirstByte_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandleTextChange(firstBox, secondBox);
        }

        private void SecondByte_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandleTextChange(secondBox, thirdBox);
        }

        private void ThirdByte_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandleTextChange(thirdBox, fourthBox);
        }

        private void FourthByte_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandleTextChange(fourthBox, fourthBox);
        }
        #endregion
    }
}
