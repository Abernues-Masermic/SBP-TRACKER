using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SBP_TRACKER
{

    public partial class FieldSafetyAnalisisWindow : Window
    {
        private List<KeyValuePair<LINK_TO_AVG, Label>> m_list_keyValuePair_link_to_avg = new();
        private List<KeyValuePair<LINK_TO_AVG, Label>> m_list_keyValuePair_dyn_read= new();

        private List<LINK_TO_AVG> List_inc = new();
        private List<LINK_TO_AVG> List_dyn = new();

        #region Constructor

        public FieldSafetyAnalisisWindow()
        {
            InitializeComponent();

            #region Control array

            m_list_keyValuePair_link_to_avg.Add(new KeyValuePair<LINK_TO_AVG, Label>(LINK_TO_AVG.INC1_SLOPE_LAT_AVG_SBPT, Label_inc1_value));
            m_list_keyValuePair_link_to_avg.Add(new KeyValuePair<LINK_TO_AVG, Label>(LINK_TO_AVG.INC2_SLOPE_LAT_AVG_SBPT, Label_inc2_value));
            m_list_keyValuePair_link_to_avg.Add(new KeyValuePair<LINK_TO_AVG, Label>(LINK_TO_AVG.INC3_SLOPE_LAT_AVG_SBPT, Label_inc3_value));
            m_list_keyValuePair_link_to_avg.Add(new KeyValuePair<LINK_TO_AVG, Label>(LINK_TO_AVG.INC4_SLOPE_LAT_AVG_SBPT, Label_inc4_value));
            m_list_keyValuePair_link_to_avg.Add(new KeyValuePair<LINK_TO_AVG, Label>(LINK_TO_AVG.INC5_SLOPE_LAT_AVG_SBPT, Label_inc5_value));
            m_list_keyValuePair_link_to_avg.Add(new KeyValuePair<LINK_TO_AVG, Label>(LINK_TO_AVG.INC_TCU_SLOPE_LAT_AVG_SBPT, Label_tcu_pos_value));
            m_list_keyValuePair_link_to_avg.Add(new KeyValuePair<LINK_TO_AVG, Label>(LINK_TO_AVG.DYN1_AVG_SBPT, Label_dyn1_factorized_value));
            m_list_keyValuePair_link_to_avg.Add(new KeyValuePair<LINK_TO_AVG, Label>(LINK_TO_AVG.DYN2_AVG_SBPT, Label_dyn2_factorized_value));
            m_list_keyValuePair_link_to_avg.Add(new KeyValuePair<LINK_TO_AVG, Label>(LINK_TO_AVG.DYN3_AVG_SBPT, Label_dyn3_factorized_value));

            m_list_keyValuePair_link_to_avg.ForEach(keyValuPair => keyValuPair.Value.Content = Constants.Error_code);


            m_list_keyValuePair_dyn_read.Add(new KeyValuePair<LINK_TO_AVG, Label>(LINK_TO_AVG.DYN1_AVG_SBPT, Label_dyn1_read_value));
            m_list_keyValuePair_dyn_read.Add(new KeyValuePair<LINK_TO_AVG, Label>(LINK_TO_AVG.DYN2_AVG_SBPT, Label_dyn2_read_value));
            m_list_keyValuePair_dyn_read.Add(new KeyValuePair<LINK_TO_AVG, Label>(LINK_TO_AVG.DYN3_AVG_SBPT, Label_dyn3_read_value));

            m_list_keyValuePair_dyn_read.ForEach(keyValuPair => keyValuPair.Value.Content = Constants.Error_code);

            #endregion

            List_inc = new List<LINK_TO_AVG> { LINK_TO_AVG.INC1_SLOPE_LAT_AVG_SBPT, LINK_TO_AVG.INC2_SLOPE_LAT_AVG_SBPT, LINK_TO_AVG.INC3_SLOPE_LAT_AVG_SBPT, LINK_TO_AVG.INC4_SLOPE_LAT_AVG_SBPT, LINK_TO_AVG.INC5_SLOPE_LAT_AVG_SBPT };
            List_dyn = new List<LINK_TO_AVG> { LINK_TO_AVG.DYN1_AVG_SBPT, LINK_TO_AVG.DYN2_AVG_SBPT, LINK_TO_AVG.DYN3_AVG_SBPT };
        }

        #endregion


        #region Loaded

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Label_diff_inc_tcu_emerg_stow_value.Content = Globals.GetTheInstance().Max_diff_tcu_inc_emergency_stow;
            Label_diff_inc_tcu_alarm_value.Content = Globals.GetTheInstance().Max_diff_tcu_inc_alarm;

            Label_max_moving_emerg_stow_value.Content = Globals.GetTheInstance().SBPT_dyn_max_mov_emerg_stow;
            Label_max_moving_alarm_value.Content = Globals.GetTheInstance().SBPT_dyn_max_mov_alarm;
            Label_max_static_alarm_value.Content = Globals.GetTheInstance().SBPT_dyn_max_static_alarm;
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


        #region Show value

        public void ShowValue(LINK_TO_AVG link_to_avg, string s_value)
        {
            KeyValuePair<LINK_TO_AVG, Label> keyValuePair_link_to_avg = m_list_keyValuePair_link_to_avg.Find(key_value => key_value.Key == link_to_avg);
            if (keyValuePair_link_to_avg.Value != null)
            {
                keyValuePair_link_to_avg.Value.Content = s_value;

                //Check inclinometer
                if (List_inc.Exists(x => x == link_to_avg))
                {
                    if
                    (double.TryParse(keyValuePair_link_to_avg.Value.Content.ToString(), NumberStyles.Any, Globals.GetTheInstance().nfi, out double value_inc) &&
                    double.TryParse(Label_tcu_pos_value.Content.ToString(), NumberStyles.Any, Globals.GetTheInstance().nfi, out double value_tcu) && value_tcu != Constants.Error_code)
                    {
                        keyValuePair_link_to_avg.Value.Foreground = value_inc - value_tcu > Globals.GetTheInstance().Max_diff_tcu_inc_emergency_stow ? Brushes.Orange : Brushes.Black;
                        keyValuePair_link_to_avg.Value.Foreground = value_inc - value_tcu > Globals.GetTheInstance().Max_diff_tcu_inc_alarm ? Brushes.Red : keyValuePair_link_to_avg.Value.Foreground;
                    }
                }
            }

            //Check dynamometer
            if (List_dyn.Exists(x => x == link_to_avg))
            {
                if (double.TryParse(keyValuePair_link_to_avg.Value.Content.ToString(), NumberStyles.Any, Globals.GetTheInstance().nfi, out double value_dyn))
                {
                    keyValuePair_link_to_avg.Value.Foreground = (value_dyn > Globals.GetTheInstance().SBPT_dyn_max_mov_emerg_stow) && (Globals.GetTheInstance().SBPT_dyn_max_mov_emerg_stow != 0) ? Brushes.Orange : Brushes.Black;
                    keyValuePair_link_to_avg.Value.Foreground = (value_dyn > Globals.GetTheInstance().SBPT_dyn_max_mov_alarm) && (Globals.GetTheInstance().SBPT_dyn_max_mov_alarm != 0) ? Brushes.Red : keyValuePair_link_to_avg.Value.Foreground;
                    keyValuePair_link_to_avg.Value.Foreground = (value_dyn > Globals.GetTheInstance().SBPT_dyn_max_static_alarm) && (Globals.GetTheInstance().SBPT_dyn_max_static_alarm != 0) ? Brushes.Red : keyValuePair_link_to_avg.Value.Foreground;
                }
            }
        }

        #endregion

        #region Show correction load PIN factor
        public void ShowCorrectionLoadPinFactor(double factor)
        {
            Label_factor_value.Content = Math.Round( factor,8);
        }

        #endregion

        #region Show DYN read value
        public void ShowDynReadValue(LINK_TO_AVG link_to_avg, double value)
        {
            KeyValuePair<LINK_TO_AVG, Label> keyValuePair_dyn = m_list_keyValuePair_dyn_read.Find(key_value => key_value.Key == link_to_avg);
            keyValuePair_dyn.Value.Content = value;
        }

        #endregion


        #region Close

        private void Button_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion
    }
}
