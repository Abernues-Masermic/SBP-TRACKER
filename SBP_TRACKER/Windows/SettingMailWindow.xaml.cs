using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Windows;
using System.Windows.Input;
using Ionic.Zip;




namespace SBP_TRACKER
{

    public partial class SettingMailWindow : Window
    {

        #region Constructor

        public SettingMailWindow()
        {
            InitializeComponent();

        }

        #endregion


        #region Loaded

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Check_mail_data_enable.IsChecked = Globals.GetTheInstance().Mail_data_on == BIT_STATE.ON ? true : false;
                Check_mail_alarm_enable.IsChecked = Globals.GetTheInstance().Mail_alarm_on == BIT_STATE.ON ? true : false;
                Time_sendMail.Text = Globals.GetTheInstance().Mail_instant;
                Textbox_from.Text = Globals.GetTheInstance().Mail_from;
                Textbox_username.Text = Globals.GetTheInstance().Mail_user;
                Textbox_pass.Password = Globals.GetTheInstance().Mail_pass;
                Textbox_smtp.Text = Globals.GetTheInstance().Mail_smtp_client;

                ListboxTo.Items.Clear();
                Globals.GetTheInstance().List_mail_to.ForEach(mailto => ListboxTo.Items.Add(mailto));
            }
            catch (Exception ex)
            {
                Manage_logs.SaveErrorValue($"{GetType().Name}  -> {nameof(SettingMailWindow)} -> {ex.Message}");
            }
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



        #region add delete TO address

        private void Button_addTo_Click(object sender, RoutedEventArgs e)
        {
            InputDialogWindow input_dialog = new()
            {
                Left = this.Left,
                Top = this.Top,
                Input_info = "Define new email addresse"
            };
            if (input_dialog.ShowDialog() == true)
            {
                ListboxTo.Items.Add(input_dialog.Input_value);
            }
        }

        private void Button_deleteTo_Click(object sender, RoutedEventArgs e)
        {
            if (ListboxTo.SelectedValue != null)
            {
                ListboxTo.Items.Remove(ListboxTo.SelectedItem);
            }
        }

        #endregion





        #region Time send box lost focus

        private void Time_sendMail_LostFocus(object sender, RoutedEventArgs e)
        {
            string s_value = Time_sendMail.Text;
            var vPromptCharCount = s_value.ToCharArray().Count(x => x == '_');
            if (vPromptCharCount != 4 && vPromptCharCount != 0)
            {
                MessageBox.Show("Complete a field", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                Time_sendMail.Text = "__:__";
            }
            else if (vPromptCharCount == 0)
            {
                if (int.Parse(s_value.Substring(0, 2)) > 23 || int.Parse(s_value.Substring(3, 2)) > 59)
                {
                    MessageBox.Show("Not valid value for this field", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    Time_sendMail.Text = "__:__";
                }
            }
        }

        #endregion

        #region Button save

        private void Button_save_Click(object sender, RoutedEventArgs e)
        {
            Globals.GetTheInstance().Mail_data_on = Check_mail_data_enable.IsChecked == true ? BIT_STATE.ON : BIT_STATE.OFF;
            Globals.GetTheInstance().Mail_alarm_on = Check_mail_alarm_enable.IsChecked == true ? BIT_STATE.ON : BIT_STATE.OFF;
            Globals.GetTheInstance().Mail_instant = Time_sendMail.Text;
            Globals.GetTheInstance().Mail_from = Textbox_from.Text;
            Globals.GetTheInstance().Mail_user = Textbox_username.Text;
            Globals.GetTheInstance().Mail_pass = Textbox_pass.Password;
            Globals.GetTheInstance().Mail_smtp_client = Textbox_smtp.Text;

            Globals.GetTheInstance().List_mail_to.Clear();
            foreach (var item in ListboxTo.Items)
            {
                Globals.GetTheInstance().List_mail_to.Add(item.ToString());
            }

            bool save_setting = Manage_file.Save_app_setting();
            bool save_mail = Manage_file.Save_email_to();

            if (save_setting && save_mail)
                MessageBox.Show("Config. saved", "Info", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            else
                MessageBox.Show("Error saving config.", "Info", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
        }

        #endregion



        #region Select ZIP file 

        private void Button_zip_file_Click(object sender, RoutedEventArgs e)
        {
            string? s_initial_dir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (File.Exists(Textbox_zip.Text))
                s_initial_dir = Path.GetDirectoryName(Textbox_zip.Text);

            else if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + Constants.Record_dir))
                s_initial_dir = AppDomain.CurrentDomain.BaseDirectory + Constants.Record_dir;

            var fileDialog = new OpenFileDialog
            {
                InitialDirectory = s_initial_dir,
                Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*"
            };

            if (fileDialog.ShowDialog() == true)
                Textbox_zip.Text = fileDialog.FileName;

            Activate();
        }

        #endregion

        #region Select send mail file

        private void Button_send_file_Click(object sender, RoutedEventArgs e)
        {
            string? s_initial_dir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            
            if (File.Exists(Textbox_send_mail.Text))
                s_initial_dir = Path.GetDirectoryName(Textbox_send_mail.Text);

            else if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + Constants.Compress_dir))
                s_initial_dir = AppDomain.CurrentDomain.BaseDirectory + Constants.Compress_dir;


            var fileDialog = new OpenFileDialog
            {
                InitialDirectory = s_initial_dir,
                Filter = "rar files (*.rar)|*.rar|All files (*.*)|*.*"
            };

            if (fileDialog.ShowDialog() == true)
                Textbox_send_mail.Text = fileDialog.FileName;

            Activate();

        }

        #endregion


        #region Zip file action
        private void Button_zip_Click(object sender, RoutedEventArgs e)
        {
            if (Textbox_zip.Text != string.Empty && File.Exists(Textbox_zip.Text))
            {
                try
                {
                    string s_zip_file = Path.GetFileNameWithoutExtension(Textbox_zip.Text) + Constants.Compress_extension;
                    string path_zip_file = Constants.Compress_dir + @"\" + s_zip_file;

                    if (File.Exists(path_zip_file))
                        File.Delete(path_zip_file);

                    using (ZipFile zip = new ())
                    {
                        //zip.Password = "SBPTRACKER"; 
                        zip.AddFile(Textbox_zip.Text, Path.GetFileNameWithoutExtension(Textbox_zip.Text));
                        zip.Save(path_zip_file);
                    }

                    MessageBox.Show("Compressing file completed", "INFO", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                catch (Exception ex)
                {
                    Manage_logs.SaveErrorValue(GetType().Name + " ->  " + nameof(Button_send_mail_Click) + " -> " + ex.Message.ToString());
                    MessageBox.Show("Error compressing file", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }

            }
        }

        #endregion

        #region Send mail action

        private void Button_send_mail_Click(object sender, RoutedEventArgs e)
        {
            if (Textbox_send_mail.Text != string.Empty && File.Exists(Textbox_send_mail.Text))
            {
                try
                {
                    string s_subject = Path.GetFileNameWithoutExtension(Textbox_send_mail.Text);

                    using MailMessage msg = new();

                    msg.From = new MailAddress(Globals.GetTheInstance().Mail_from);
                    Globals.GetTheInstance().List_mail_to.ForEach(mailto => msg.To.Add(new MailAddress(mailto)));             

                    msg.Subject = "SBP TRACKER. " + s_subject ;
                    msg.Body = string.Empty;
                    msg.Attachments.Add(new Attachment(Textbox_send_mail.Text));
                    msg.Priority = MailPriority.High;

                    SmtpClient cliente_smtp = new(Globals.GetTheInstance().Mail_smtp_client)
                    {
                        EnableSsl = true,
                        UseDefaultCredentials = false,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        Credentials = new NetworkCredential(Globals.GetTheInstance().Mail_user, Globals.GetTheInstance().Mail_pass)
                    };

                    cliente_smtp.Send(msg);

                    MessageBox.Show("Send file via mail completed", "INFO", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                catch (Exception ex)
                {
                    Manage_logs.SaveErrorValue(GetType().Name + " ->  " + nameof(Button_send_mail_Click) + " -> " + ex.Message.ToString());
                    MessageBox.Show("Error sending file", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }

            }
        }

        #endregion



        #region Button exit

        private void Button_exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion


    }
}
