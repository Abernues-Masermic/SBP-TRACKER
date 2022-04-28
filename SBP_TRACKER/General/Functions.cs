using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace SBP_TRACKER
{
    public static class Functions
    {
        #region Get subarray

        public static T[] SubArray<T>(this T[] array, int offset, int length)
        {
            T[] result = new T[length];
            Array.Copy(array, offset, result, 0, length);
            return result;
        }

        #endregion



        #region Read from array

        public static string Read_from_array(int[] read_values, int start_pos, TypeCode var_Type)
        {
            string s_value = string.Empty;

            switch (var_Type)
            {
                case TypeCode.Int16:

                    byte[] array_data = BitConverter.GetBytes(read_values[start_pos]);
                    short i16_data = BitConverter.ToInt16(array_data, 0);
                    s_value = i16_data.ToString();

                    break;

                case TypeCode.Int32:
                    byte[] array_data_1 = BitConverter.GetBytes(read_values[start_pos]);
                    byte[] array_data_2 = BitConverter.GetBytes(read_values[start_pos + 1]);

                    array_data = array_data_1.SubArray(0, 2).Concat(array_data_2.SubArray(0, 2)).ToArray();
                    int i32_data = BitConverter.ToInt32(array_data, 0);
                    s_value = i32_data.ToString();

                    break;

                case TypeCode.UInt16:
                    array_data = BitConverter.GetBytes(read_values[start_pos]);
                    ushort ui16_data = BitConverter.ToUInt16(array_data, 0);
                    s_value = ui16_data.ToString();

                    break;

                case TypeCode.UInt32:
                    array_data_1 = BitConverter.GetBytes(read_values[start_pos]);
                    array_data_2 = BitConverter.GetBytes(read_values[start_pos + 1]);

                    array_data = array_data_1.SubArray(0, 2).Concat(array_data_2.SubArray(0, 2)).ToArray();
                    uint ui32_data = BitConverter.ToUInt32(array_data, 0);
                    s_value = ui32_data.ToString();

                    break;

                case TypeCode.Single:
                    array_data_1 = BitConverter.GetBytes(read_values[start_pos]);
                    array_data_2 = BitConverter.GetBytes(read_values[start_pos + 1]);

                    array_data = array_data_1.SubArray(0, 2).Concat(array_data_2.SubArray(0, 2)).ToArray();
                    float single_data = BitConverter.ToSingle(array_data, 0);
                    s_value = single_data.ToString("0.00", Globals.GetTheInstance().nfi);

                    break;
            }

            return s_value;
        }

        #endregion


        #region Read from array and convert value

        public static string Read_from_array_convert_scale(int[] read_values, int dir_mb, string var_name, TypeCode var_type, double value_min, double scale_min, double scale_factor)
        {
            string s_value = string.Empty;

            switch (var_type)
            {
                case TypeCode.Int16:
                    {
                        byte[] array_data = BitConverter.GetBytes(read_values[dir_mb]);
                        Int16 i16_data = BitConverter.ToInt16(array_data, 0);
                        double d_value = scale_min + ((i16_data - value_min) * scale_factor);
                        s_value = d_value.ToString("0.00", Globals.GetTheInstance().nfi);

                        break;
                    }

                case TypeCode.Int32:
                    {
                        byte[] array_data_1 = BitConverter.GetBytes(read_values[dir_mb]);
                        byte[] array_data_2 = BitConverter.GetBytes(read_values[dir_mb + 1]);

                        byte[] array_data = array_data_1.SubArray(0, 2).Concat(array_data_2.SubArray(0, 2)).ToArray();
                        Int32 i32_data = BitConverter.ToInt32(array_data, 0);
                        double d_value = scale_min + ((i32_data - value_min) * scale_factor);
                        s_value = d_value.ToString("0.00", Globals.GetTheInstance().nfi);

                        break;
                    }

                case TypeCode.UInt16:
                    {
                        byte[] array_data = BitConverter.GetBytes(read_values[dir_mb]);
                        UInt16 ui16_data = BitConverter.ToUInt16(array_data, 0);
                        double d_value = scale_min + ((ui16_data - value_min) * scale_factor);
                        s_value = d_value.ToString("0.00", Globals.GetTheInstance().nfi);

                        if (Globals.GetTheInstance().Depur_enable == BIT_STATE.ON)
                        {
                            string s_values = string.Empty;
                            read_values.ToList().ForEach(x =>
                            {

                                byte[] byte_check = BitConverter.GetBytes(x);
                                UInt16 ui16_check = BitConverter.ToUInt16(byte_check, 0);

                                s_values += $"0x{ui16_check:X2} ";
                            });

                            string s_log = $"READ VALUES : {s_values} / NAME : {var_name} / POS : {dir_mb} / value : {ui16_data} / scale factor : { scale_factor.ToString("0.00000", Globals.GetTheInstance().nfi)} / conversion : {s_value}";
                            Manage_logs.SaveDepurValue(s_log);
                        }

                        break;
                    }

                case TypeCode.UInt32:
                    {
                        byte[] array_data_1 = BitConverter.GetBytes(read_values[dir_mb]);
                        byte[] array_data_2 = BitConverter.GetBytes(read_values[dir_mb + 1]);

                        byte[] array_data = array_data_1.SubArray(0, 2).Concat(array_data_2.SubArray(0, 2)).ToArray();
                        UInt32 ui32_data = BitConverter.ToUInt32(array_data, 0);
                        double d_value = scale_min + ((ui32_data - value_min) * scale_factor);
                        s_value = d_value.ToString("0.00", Globals.GetTheInstance().nfi);

                        break;
                    }

                case TypeCode.Single:
                    {
                        byte[] array_data_1 = BitConverter.GetBytes(read_values[dir_mb]);
                        byte[] array_data_2 = BitConverter.GetBytes(read_values[dir_mb + 1]);

                        byte[] array_data = array_data_1.SubArray(0, 2).Concat(array_data_2.SubArray(0, 2)).ToArray();
                        float single_data = BitConverter.ToSingle(array_data, 0);
                        double d_value = scale_min + ((single_data - value_min) * scale_factor);
                        s_value = d_value.ToString("0.00", Globals.GetTheInstance().nfi);

                        break;
                    }
            }


            return s_value;
        }

        #endregion


        #region Calculate scale factor

        public static double Calculate_scale_factor(TCPModbusVarEntry var_entry)
        {
            double read_range = var_entry.Read_range_max - var_entry.Read_range_min;
            double scale_range = var_entry.Scaled_range_max - var_entry.Scaled_range_min;
            double scale_factor = scale_range / read_range;
            scale_factor = Math.Round(scale_factor, 10);
            return scale_factor;
        }


        #endregion


        #region Redefine send mail instant

        public static int Redefine_send_mail_instant()
        {
            int interval_ms = 24 * 60 * 60 * 1000;

            try
            {
                #region Hours

                int current_hour = DateTime.Now.Hour;
                int send_hour = int.Parse(Globals.GetTheInstance().Mail_instant[..2]);

                int diff_hour;
                if (send_hour >= current_hour)
                    diff_hour = send_hour - current_hour;
                else
                    diff_hour = 24 - send_hour + current_hour;

                int hour_milliseconds = diff_hour * 60 * 60 * 1000;

                #endregion

                #region Minutes

                int current_min = DateTime.Now.Minute;
                int send_min = int.Parse(Globals.GetTheInstance().Mail_instant.Substring(3, 2));

                int diff_min;
                if (send_min >= current_min)
                    diff_min = send_min - current_min;
                else
                    diff_min = 60 - send_min + current_min;

                int min_milliseconds = diff_min * 60 * 1000;

                #endregion

                //Check if instant is the same as current time
                interval_ms = hour_milliseconds + min_milliseconds;
                if (interval_ms == 0)
                    interval_ms = 24 * 60 * 60 * 1000;

                string s_log = "REDEFINE SEND MAIL INSTANT. HOUR : " + Globals.GetTheInstance().Mail_instant + " / MILLISECONDS : " + interval_ms;
                Manage_logs.SaveLogValue(s_log);
            }
            catch (Exception ex)
            {
                Manage_logs.SaveErrorValue(typeof(Manage_file).Name + " ->  " + nameof(Redefine_send_mail_instant) + " -> " + ex.Message.ToString());
            }

            return interval_ms;
        }

        #endregion


        #region Send mail

        public static Tuple<bool, List<string>> Send_record_mail()
        {
            bool b_send = true;
            List<string> list_files = new();
            try
            {
                string s_log = "SEND RECORD MAIL -> ";

                using MailMessage msg = new();

                s_log += "FROM : " + Globals.GetTheInstance().Mail_from;

                msg.From = new MailAddress(Globals.GetTheInstance().Mail_from);

                s_log += " / TO : ";

                Globals.GetTheInstance().List_mail_to.ForEach(mailto =>
                {
                    msg.To.Add(new MailAddress(mailto));
                    s_log += mailto + "; ";
                });

                msg.Subject = "SBP TRACKER REPORT";
                msg.Body = string.Empty;
                msg.Priority = MailPriority.High;

                s_log += " / FILES : ";

                List<string> list_record_file = new() { Constants.Record_scs1, Constants.Record_scs2 };
                list_record_file.ForEach(record_file =>
                {
                    string s_file = AppDomain.CurrentDomain.BaseDirectory + Constants.Compress_dir + @"\" + record_file + DateTime.Now.Year.ToString("0000") + DateTime.Now.Month.ToString("00") + DateTime.Now.AddDays(-1).Day.ToString("00") + ".zip";
                    if (File.Exists(s_file))
                    {
                        msg.Attachments.Add(new Attachment(s_file));
                        list_files.Add(Path.GetFileNameWithoutExtension(s_file));

                        s_log += Path.GetFileNameWithoutExtension(s_file) + "; ";
                    }
                });

                SmtpClient cliente_smtp = new(Globals.GetTheInstance().Mail_smtp_client)
                {
                    Credentials = new NetworkCredential(Globals.GetTheInstance().Mail_user, Globals.GetTheInstance().Mail_pass)
                };

                cliente_smtp.Send(msg);
            }
            catch (Exception ex)
            {
                Manage_logs.SaveErrorValue($"{typeof(Manage_file).Name} -> {nameof(Send_record_mail)} -> {ex.Message}");
                b_send = false;
            }

            Tuple<bool, List<string>> tuple_values = new(b_send, list_files);

            return tuple_values;
        }

        #endregion

        #region Send alarm main

        public static bool Send_alarm_mail(string s_subject, string s_body)
        {
            bool b_send = true;

            try
            {
                Manage_logs.SaveLogValue($"Send alarm mail. SUBJECT : {s_subject} / MESSAGE : {s_body}");

                string s_log = "SEND RECORD MAIL -> ";

                using MailMessage msg = new();

                s_log += "FROM : " + Globals.GetTheInstance().Mail_from;

                msg.From = new MailAddress(Globals.GetTheInstance().Mail_from);

                s_log += " / TO : ";

                Globals.GetTheInstance().List_mail_to.ForEach(mailto =>
                {
                    msg.To.Add(new MailAddress(mailto));
                    s_log += mailto + "; ";
                });

                msg.Subject = s_subject;
                msg.Body = s_body;
                msg.Priority = MailPriority.High;

                SmtpClient cliente_smtp = new(Globals.GetTheInstance().Mail_smtp_client)
                {
                    Credentials = new NetworkCredential(Globals.GetTheInstance().Mail_user, Globals.GetTheInstance().Mail_pass)
                };

                cliente_smtp.Send(msg);
            }
            catch (Exception ex)
            {
                Manage_logs.SaveErrorValue($"{typeof(Manage_file).Name} -> {nameof(Send_alarm_mail)} -> {ex.Message}");
                b_send = false;
            }

            return b_send;
        }

        #endregion


        #region TypeCode MIN MAX VALUE

        public static Tuple<decimal, decimal> TypeCode_min_max(TypeCode typecode)
        {
            decimal min_value = 0;
            decimal max_value = 0;
            switch (typecode)
            {
                case TypeCode.UInt16:
                    {
                        min_value = UInt16.MinValue;
                        max_value = UInt16.MaxValue;
                        break;
                    }
                case TypeCode.Int16:
                    {
                        min_value = Int16.MinValue;
                        max_value = Int16.MaxValue;
                        break;
                    }

                case TypeCode.UInt32:
                    {
                        min_value = UInt32.MinValue;
                        max_value = UInt32.MaxValue;
                        break;
                    }

                case TypeCode.Int32:
                    {
                        min_value = Int32.MinValue;
                        max_value = Int32.MaxValue;
                        break;
                    }
                case TypeCode.Single:
                    {
                        min_value = decimal.MinValue;
                        max_value = decimal.MaxValue;
                        break;
                    }
            }

            return Tuple.Create(min_value, max_value);
        }

        #endregion



        #region Set check bits

        public static int SetBitTo1(this int value, int position)
        {
            // Set a bit at position to 1.
            return value |= (1 << position);
        }

        public static int SetBitTo0(this int value, int position)
        {
            // Set a bit at position to 0.
            return value & ~(1 << position);
        }

        public static bool IsBitSetTo1(this int value, int position)
        {
            // Return whether bit at position is set to 1.
            return (value & (1 << position)) != 0;
        }

        public static bool IsBitSetTo0(this int value, int position)
        {
            // If not 1, bit is 0.
            return !IsBitSetTo1(value, position);
        }

        #endregion
    }
}
