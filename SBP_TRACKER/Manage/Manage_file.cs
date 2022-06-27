using AMS.Profile;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace SBP_TRACKER
{
    internal class Manage_file
    {
        #region Create directories

        public static bool Create_directories()
        {
            bool create_ok = true;

            string[] dirs = new string[] { Constants.SettingApp_dir, Constants.SettingModbus_dir, Constants.Log_dir, Constants.Record_dir, Constants.Compress_dir };

            try
            {
                dirs.ToList().ForEach(dir =>
                {
                    if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + dir))
                        Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + dir);
                });
            }
            catch (Exception ex)
            {
                create_ok = false;
                Manage_logs.SaveErrorValue($"{typeof(Manage_file).Name} -> {nameof(Create_directories)} -> {ex.Message}");
            }

            return create_ok;
        }

        #endregion

        #region Create files

        public static bool Create_files()
        {
            bool create_ok = true;

            try
            {
                string[] files = new string[] { Constants.SettingModbus_dir + @"\modbusSlave.csv", Constants.SettingModbus_dir + @"\modbusMapped.csv", Constants.SettingModbus_dir + @"\codifiedStatus.csv", Constants.SettingModbus_dir + @"\statusMask.csv" };

                files.ToList().ForEach(file =>
                {
                    if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + file))
                    {
                        using FileStream fs = File.Create(AppDomain.CurrentDomain.BaseDirectory + file);
                        fs.Close();
                    }
                });

                //Creating setting xml
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + Constants.SettingApp_dir + @"\setting.xml"))
                {
                    XmlDocument xmlDoc = new();
                    XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                    XmlElement rootNode = xmlDoc.CreateElement("RAIZ");
                    xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);
                    xmlDoc.AppendChild(rootNode);
                    xmlDoc.Save(AppDomain.CurrentDomain.BaseDirectory + Constants.SettingApp_dir + @"\setting.xml");
                }

                //Creating email.txt
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + Constants.SettingApp_dir + @"\email.txt"))
                {
                    using var myFile = File.Create(AppDomain.CurrentDomain.BaseDirectory + Constants.SettingApp_dir + @"\email.txt");
                    myFile.Close();
                }

            }
            catch (Exception ex)
            {
                create_ok = false;
                Manage_logs.SaveErrorValue(typeof(Manage_file).Name + " ->  " + nameof(Create_files) + " -> " + ex.Message.ToString());
            }

            return create_ok;
        }

        #endregion


        #region Load app setting

        public static bool Load_app_setting()
        {
            bool load_ok = true;
            try
            {
                int i_default = 10;
                double d_default = 0.5;

                Profile profile = new Xml(AppDomain.CurrentDomain.BaseDirectory + Constants.SettingApp_dir + @"\setting.xml");

                Globals.GetTheInstance().Depur_enable = (BIT_STATE)int.Parse(profile.GetValue("General", "Depur_enable", 0).ToString());
                Globals.GetTheInstance().TCU_depur = (BIT_STATE)int.Parse(profile.GetValue("General", "TCU_depur", 0).ToString());
                Globals.GetTheInstance().Refresh_scada_interval = int.Parse(profile.GetValue("General", "Refresh_scada_interval", 1500).ToString());

                Globals.GetTheInstance().Modbus_read_scs_normal_interval = int.Parse(profile.GetValue("Modbus", "Read_scs_normal_interval", 1000).ToString());
                Globals.GetTheInstance().Modbus_read_scs_fast_interval = int.Parse(profile.GetValue("Modbus", "Read_scs_fast_interval", 200).ToString());
                Globals.GetTheInstance().Modbus_read_tcu_interval = int.Parse(profile.GetValue("Modbus", "Read_tcu_interval", 1000).ToString());
                Globals.GetTheInstance().Modbus_write_tcu_watchdog_interval = int.Parse(profile.GetValue("Modbus", "Write_tcu_watchdog_interval", 1000).ToString());
                Globals.GetTheInstance().Modbus_write_tcu_datetime_interval = int.Parse(profile.GetValue("Modbus", "Write_tcu_datetime_interval", 60000).ToString());
                Globals.GetTheInstance().Modbus_write_samca_interval = int.Parse(profile.GetValue("Modbus", "Write_samca_interval", 1000).ToString());
                Globals.GetTheInstance().Modbus_conn_timeout = int.Parse(profile.GetValue("Modbus", "Conn_timeout", 1000).ToString());
                Globals.GetTheInstance().Modbus_comm_timeout = int.Parse(profile.GetValue("Modbus", "Comm_timeout", 10000).ToString());
                Globals.GetTheInstance().Modbus_reconnect_interval = int.Parse(profile.GetValue("Modbus", "Reconnect_interval", 10000).ToString());
                Globals.GetTheInstance().Modbus_dir_tcu_command = ushort.Parse(profile.GetValue("Modbus", "Dir_tcu_command", 3100).ToString());
                Globals.GetTheInstance().Modbus_dir_tcu_datetime = ushort.Parse(profile.GetValue("Modbus", "Dir_tcu_datetime", 3300).ToString());
                Globals.GetTheInstance().Modbus_dir_write_samca = ushort.Parse(profile.GetValue("Modbus", "Dir_write_samca", 2000).ToString());

                Globals.GetTheInstance().Enable_write_tcu = (BIT_STATE)int.Parse(profile.GetValue("Modbus", "Enable_write_tcu", 1).ToString());
                Globals.GetTheInstance().Enable_write_samca = (BIT_STATE)int.Parse(profile.GetValue("Modbus", "Enable_write_samca", 1).ToString());


                Globals.GetTheInstance().Record_scs_normal_interval = int.Parse(profile.GetValue("Record", "Record_scs_normal_interval", 1000).ToString());
                Globals.GetTheInstance().Record_scs_fast_interval = int.Parse(profile.GetValue("Record", "Record_scs_fast_interval", 250).ToString());
                Globals.GetTheInstance().Record_tcu_interval = int.Parse(profile.GetValue("Record", "Record_tcu_interval", 1000).ToString());
                Globals.GetTheInstance().Record_samca_interval = int.Parse(profile.GetValue("Record", "Record_samca_interval", 1000).ToString());

                Globals.GetTheInstance().Decimal_sep = (DECIMAL_SEP)int.Parse(profile.GetValue("Record", "Decimal_sep", 0).ToString());
                Globals.GetTheInstance().Field_sep = (FIELD_SEP)int.Parse(profile.GetValue("Record", "Field_sep", 0).ToString());
                Globals.GetTheInstance().SField_sep = Globals.GetTheInstance().Field_sep == FIELD_SEP.COMA ? "," : ";";
                Globals.GetTheInstance().Date_format = profile.GetValue("Record", "Date_format", "yyyy/dd/MM HH:mm:ss.fff").ToString();
                Globals.GetTheInstance().Format_provider = profile.GetValue("Record", "Format_provider", "en-US").ToString();

                Globals.GetTheInstance().SBPT_trigger_3sec = profile.GetValue("WindConfig", "sbpt_trigger_3sec", d_default);
                Globals.GetTheInstance().SBPT_trigger_10min = profile.GetValue("WindConfig", "sbpt_trigger_10min", d_default);
                Globals.GetTheInstance().SBPT_wind_delay_time_3sec = profile.GetValue("WindConfig", "sbpt_wind_delay_time_3sec", i_default);
                Globals.GetTheInstance().SBPT_low_hist_10min = profile.GetValue("WindConfig", "sbpt_low_hist_10min", d_default);

                Globals.GetTheInstance().SAMCA_trigger_3sec = profile.GetValue("WindConfig", "samca_trigger_3sec", d_default);
                Globals.GetTheInstance().SAMCA_trigger_10min = profile.GetValue("WindConfig", "samca_trigger_10min", d_default);
                Globals.GetTheInstance().SAMCA_wind_delay_time_3sec = profile.GetValue("WindConfig", "samca_wind_delay_time_3sec", i_default);
                Globals.GetTheInstance().SAMCA_low_hist_10min = profile.GetValue("WindConfig", "samca_low_hist_10min", d_default);

                Globals.GetTheInstance().SBPT_inc_avg_interval = profile.GetValue("IncConfig", "sbpt_inc_avg_interval", i_default);
                Globals.GetTheInstance().Max_diff_tcu_inc_emergency_stow = profile.GetValue("IncConfig", "max_diff_tcu_inc_emergency_stow", d_default);
                Globals.GetTheInstance().Max_diff_tcu_inc_alarm = profile.GetValue("IncConfig", "max_diff_tcu_inc_alarm", d_default);

                Globals.GetTheInstance().SBPT_dyn_avg_interval = profile.GetValue("DynConfig", "sbpt_dyn_avg_interval", i_default);
                Globals.GetTheInstance().SBPT_dyn_max_mov_emerg_stow = profile.GetValue("DynConfig", "sbpt_dyn_max_mov_emerg_stow", i_default);
                Globals.GetTheInstance().SBPT_dyn_max_mov_alarm = profile.GetValue("DynConfig", "sbpt_dyn_max_mov_alarm", i_default);
                Globals.GetTheInstance().SBPT_dyn_max_static_alarm = profile.GetValue("DynConfig", "sbpt_dyn_max_static_alarm", i_default);

                Globals.GetTheInstance().Mail_data_on = (BIT_STATE)profile.GetValue("Mail", "Mail_data_enable", 0);
                Globals.GetTheInstance().Mail_alarm_on = (BIT_STATE)profile.GetValue("Mail", "Mail_alarm_enable", 0);
                Globals.GetTheInstance().Mail_instant = profile.GetValue("Mail", "Mail_instant", "00:00").ToString();
                Globals.GetTheInstance().Mail_from = profile.GetValue("Mail", "Mail_from", "").ToString();
                Globals.GetTheInstance().Mail_user = profile.GetValue("Mail", "Mail_username", "").ToString();
                Globals.GetTheInstance().Mail_pass = profile.GetValue("Mail", "Mail_pass", "").ToString();
                Globals.GetTheInstance().Mail_smtp_client = profile.GetValue("Mail", "Mail_smtp_client", "").ToString();

                Globals.GetTheInstance().Load_cloud_on = (BIT_STATE)profile.GetValue("Cloud", "Load_cloud_enable", 0);
                Globals.GetTheInstance().Cloud_check_interval = int.Parse(profile.GetValue("Cloud", "Cloud_check_interval", 60).ToString());
                Globals.GetTheInstance().Python_path = profile.GetValue("Cloud", "Python_path", "");
                Globals.GetTheInstance().Cloud_script = profile.GetValue("Cloud", "Cloud_script", "CloudDataLoad.py");

                Globals.GetTheInstance().Enable_web_api = (BIT_STATE)profile.GetValue("WebApi", "WebApiEnable", 0);
                Globals.GetTheInstance().Tracker_ID = decimal.Parse(profile.GetValue("WebApi", "TrackerID", 1).ToString());
                Globals.GetTheInstance().Tracker_name = profile.GetValue("WebApi", "TrackerName", "DEFAULT").ToString();
                Globals.GetTheInstance().API_root = profile.GetValue("WebApi", "API_root", "").ToString();
                Globals.GetTheInstance().Data_controller_route = profile.GetValue("WebApi", "Data_controller_route", "").ToString();
                Globals.GetTheInstance().State_controller_route = profile.GetValue("WebApi", "State_controller_route", "").ToString();
                Globals.GetTheInstance().Send_state_interval_web_API = profile.GetValue("WebApi", "Send_state_interval", 10000);
                Globals.GetTheInstance().Send_data_interval_web_API = profile.GetValue("WebApi", "Send_data_interval", 2000);
                Globals.GetTheInstance().Wait_error_conn_interval_web_API = profile.GetValue("WebApi", "Wait_error_conn_interval", 60000);
                Globals.GetTheInstance().HTTP_timeout = profile.GetValue("WebApi", "Http_timeout", 2000);
            }
            catch (Exception ex)
            {
                load_ok = false;
                Manage_logs.SaveErrorValue($"{typeof(Manage_file).Name} -> {nameof(Load_app_setting)} -> {ex.Message}");
            }

            return load_ok;
        }

        #endregion

        #region Save app setting

        public static bool Save_app_setting()
        {
            bool save_ok = true;
            try
            {
                Profile profile = new Xml(AppDomain.CurrentDomain.BaseDirectory + Constants.SettingApp_dir + @"\setting.xml");

                profile.SetValue("General", "Depur_enable", (int)Globals.GetTheInstance().Depur_enable);
                profile.SetValue("General", "TCU_depur", (int)Globals.GetTheInstance().TCU_depur);
                profile.SetValue("General", "Refresh_scada_interval", Globals.GetTheInstance().Refresh_scada_interval);

                profile.SetValue("Modbus", "Read_scs_normal_interval", Globals.GetTheInstance().Modbus_read_scs_normal_interval);
                profile.SetValue("Modbus", "Read_scs_fast_interval", Globals.GetTheInstance().Modbus_read_scs_fast_interval);
                profile.SetValue("Modbus", "Read_tcu_interval", Globals.GetTheInstance().Modbus_read_tcu_interval);
                profile.SetValue("Modbus", "Write_tcu_watchdog_interval", Globals.GetTheInstance().Modbus_write_tcu_watchdog_interval);
                profile.SetValue("Modbus", "Write_tcu_datetime_interval", Globals.GetTheInstance().Modbus_write_tcu_datetime_interval);
                profile.SetValue("Modbus", "Write_samca_interval", Globals.GetTheInstance().Modbus_write_samca_interval);
                profile.SetValue("Modbus", "Conn_timeout", Globals.GetTheInstance().Modbus_conn_timeout);
                profile.SetValue("Modbus", "Comm_timeout", Globals.GetTheInstance().Modbus_comm_timeout);
                profile.SetValue("Modbus", "Reconnect_interval", Globals.GetTheInstance().Modbus_reconnect_interval);
                profile.SetValue("Modbus", "Dir_tcu_command", Globals.GetTheInstance().Modbus_dir_tcu_command);
                profile.SetValue("Modbus", "Dir_tcu_datetime", Globals.GetTheInstance().Modbus_dir_tcu_datetime);
                profile.SetValue("Modbus", "Dir_write_samca", Globals.GetTheInstance().Modbus_dir_write_samca);

                profile.SetValue("Modbus", "Enable_write_tcu", (int)Globals.GetTheInstance().Enable_write_tcu);
                profile.SetValue("Modbus", "Enable_write_samca", (int)Globals.GetTheInstance().Enable_write_samca);


                profile.SetValue("Record", "Record_scs_normal_interval", Globals.GetTheInstance().Record_scs_normal_interval);
                profile.SetValue("Record", "Record_scs_fast_interval", Globals.GetTheInstance().Record_scs_fast_interval);
                profile.SetValue("Record", "Record_tcu_interval", Globals.GetTheInstance().Record_tcu_interval);
                profile.SetValue("Record", "Record_samca_interval", Globals.GetTheInstance().Record_samca_interval);

                profile.SetValue("Record", "Decimal_sep", (int)Globals.GetTheInstance().Decimal_sep);
                profile.SetValue("Record", "Field_sep", (int)Globals.GetTheInstance().Field_sep);
                profile.SetValue("Record", "Date_format", Globals.GetTheInstance().Date_format);
                profile.SetValue("Record", "Format_provider", Globals.GetTheInstance().Format_provider);

                profile.SetValue("WindConfig", "sbpt_trigger_3sec", Globals.GetTheInstance().SBPT_trigger_3sec);
                profile.SetValue("WindConfig", "sbpt_trigger_10min", Globals.GetTheInstance().SBPT_trigger_10min);
                profile.SetValue("WindConfig", "sbpt_wind_delay_time_3sec", Globals.GetTheInstance().SBPT_wind_delay_time_3sec);
                profile.SetValue("WindConfig", "sbpt_low_hist_10min", Globals.GetTheInstance().SBPT_low_hist_10min);

                profile.SetValue("WindConfig", "samca_trigger_3sec", Globals.GetTheInstance().SAMCA_trigger_3sec);
                profile.SetValue("WindConfig", "samca_trigger_10min", Globals.GetTheInstance().SAMCA_trigger_10min);
                profile.SetValue("WindConfig", "samca_wind_delay_time_3sec", Globals.GetTheInstance().SAMCA_wind_delay_time_3sec);
                profile.SetValue("WindConfig", "samca_low_hist_10min", Globals.GetTheInstance().SAMCA_low_hist_10min);

                profile.SetValue("IncConfig", "sbpt_inc_avg_interval", Globals.GetTheInstance().SBPT_inc_avg_interval);
                profile.SetValue("IncConfig", "max_diff_tcu_inc_emergency_stow", Globals.GetTheInstance().Max_diff_tcu_inc_emergency_stow);
                profile.SetValue("IncConfig", "max_diff_tcu_inc_alarm", Globals.GetTheInstance().Max_diff_tcu_inc_alarm);

                profile.SetValue("DynConfig", "sbpt_dyn_avg_interval", Globals.GetTheInstance().SBPT_dyn_avg_interval);
                profile.SetValue("DynConfig", "sbpt_dyn_max_mov_emerg_stow", Globals.GetTheInstance().SBPT_dyn_max_mov_emerg_stow);
                profile.SetValue("DynConfig", "sbpt_dyn_max_mov_alarm", Globals.GetTheInstance().SBPT_dyn_max_mov_alarm);
                profile.SetValue("DynConfig", "sbpt_dyn_max_static_alarm", Globals.GetTheInstance().SBPT_dyn_max_static_alarm);

                profile.SetValue("Mail", "Mail_data_enable", (int)Globals.GetTheInstance().Mail_data_on);
                profile.SetValue("Mail", "Mail_alarm_enable", (int)Globals.GetTheInstance().Mail_alarm_on);
                profile.SetValue("Mail", "Mail_instant", Globals.GetTheInstance().Mail_instant);
                profile.SetValue("Mail", "Mail_from", Globals.GetTheInstance().Mail_from);
                profile.SetValue("Mail", "Mail_username", Globals.GetTheInstance().Mail_user);
                profile.SetValue("Mail", "Mail_pass", Globals.GetTheInstance().Mail_pass);
                profile.SetValue("Mail", "Mail_smtp_client", Globals.GetTheInstance().Mail_smtp_client);

                profile.SetValue("Cloud", "Load_cloud_enable", (int)Globals.GetTheInstance().Load_cloud_on);
                profile.SetValue("Cloud", "Cloud_check_interval", Globals.GetTheInstance().Cloud_check_interval);
                profile.SetValue("Cloud", "Python_path", Globals.GetTheInstance().Python_path);
                profile.SetValue("Cloud", "Cloud_script", Globals.GetTheInstance().Cloud_script);

                profile.SetValue("WebApi", "WebApiEnable", (int)Globals.GetTheInstance().Enable_web_api);
                profile.SetValue("WebApi", "TrackerID", Globals.GetTheInstance().Tracker_ID);
                profile.SetValue("WebApi", "TrackerName", Globals.GetTheInstance().Tracker_name);
                profile.SetValue("WebApi", "API_root", Globals.GetTheInstance().API_root);
                profile.SetValue("WebApi", "Data_controller_route", Globals.GetTheInstance().Data_controller_route);
                profile.SetValue("WebApi", "State_controller_route", Globals.GetTheInstance().State_controller_route);
                profile.SetValue("WebApi", "Send_state_interval", Globals.GetTheInstance().Send_state_interval_web_API);
                profile.SetValue("WebApi", "Send_data_interval", Globals.GetTheInstance().Send_data_interval_web_API);
                profile.SetValue("WebApi", "Wait_error_conn_interval", Globals.GetTheInstance().Wait_error_conn_interval_web_API);
                profile.SetValue("WebApi", "Http_timeout", Globals.GetTheInstance().HTTP_timeout);
            }
            catch (Exception ex)
            {
                save_ok = false;
                Manage_logs.SaveErrorValue($"{typeof(Manage_file).Name} -> {nameof(Save_app_setting)} -> {ex.Message}");
            }

            return save_ok;
        }

        #endregion


        #region Load email list

        public static bool Load_email_to()
        {
            Globals.GetTheInstance().List_mail_to = new List<string>();

            bool load_ok = true;
            try
            {
                using StreamReader file = new(AppDomain.CurrentDomain.BaseDirectory + Constants.SettingApp_dir + @"\email.txt");
                string ln;

                while ((ln = file.ReadLine()) != null)
                {
                    Globals.GetTheInstance().List_mail_to.Add(ln);
                }
                file.Close();
            }
            catch (Exception ex)
            {
                load_ok = false;
                Manage_logs.SaveErrorValue($"{typeof(Manage_file).Name} -> {nameof(Load_email_to)} -> {ex.Message}");
            }

            return load_ok;
        }

        #endregion

        #region Save email list

        public static bool Save_email_to()
        {
            bool save_ok = true;
            try
            {
                using StreamWriter file = new(AppDomain.CurrentDomain.BaseDirectory + Constants.SettingApp_dir + @"\email.txt", false);
                Globals.GetTheInstance().List_mail_to.ForEach(mailto => file.WriteLine(mailto));
                file.Close();
            }
            catch (Exception ex)
            {
                save_ok = false;
                Manage_logs.SaveErrorValue(typeof(Manage_file).Name + " ->  " + nameof(Save_email_to) + " -> " + ex.Message.ToString());
            }

            return save_ok;
        }

        #endregion






        #region Load modbus slave entries

        public static bool Load_modbus_slave_entries()
        {
            bool read_ok = true;

            string modbus_slave_csv = AppDomain.CurrentDomain.BaseDirectory + @"SettingModbusSlave\modbusSlave.csv";

            try
            {
                using TextReader reader = new StreamReader(modbus_slave_csv);
                var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = ";", Encoding = Encoding.UTF8, HasHeaderRecord = false, MissingFieldFound = null };
                using var csv_reader = new CsvReader(reader, config);
                csv_reader.Context.RegisterClassMap<TCPModbusSlaveMap>();

                Globals.GetTheInstance().List_slave_entry = csv_reader.GetRecords<TCPModbusSlaveEntry>().ToList();

                Globals.GetTheInstance().List_slave_entry.ForEach(slave_entry =>
                {
                    slave_entry.Connected = false;
                    slave_entry.Num_communication_error = 0;
                    slave_entry.List_var_entry = new List<TCPModbusVarEntry>();
                });
            }
            catch (Exception ex)
            {
                read_ok = false;
                Manage_logs.SaveErrorValue($"{typeof(Manage_file).Name} ->  {nameof(Load_modbus_slave_entries)} -> {ex.Message}");
            }

            return read_ok;

        }

        #endregion

        #region Save modbus slave entries

        public static bool Save_modbus_slave_entries()
        {
            bool save_ok = true;

            string modbus_slave_csv = AppDomain.CurrentDomain.BaseDirectory + Constants.SettingModbus_dir + @"\modbusSlave.csv";

            try
            {
                using TextWriter writer = new StreamWriter(modbus_slave_csv, false);
                var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = ";", Encoding = Encoding.UTF8 };
                using var csv_writer = new CsvWriter(writer, config);
                csv_writer.Context.RegisterClassMap<TCPModbusSlaveMap>();
                foreach (TCPModbusSlaveEntry slave_entry in Globals.GetTheInstance().List_slave_entry)
                {
                    csv_writer.WriteRecord(slave_entry);
                    csv_writer.NextRecord();
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                save_ok = false;
                Manage_logs.SaveErrorValue(typeof(Manage_file).Name + " ->  " + nameof(Save_modbus_slave_entries) + " -> " + ex.Message.ToString());
            }

            return save_ok;
        }

        #endregion


        #region Load var map entries
        public static bool Load_var_map_entries()
        {
            bool read_ok = true;

            string var_map_csv = AppDomain.CurrentDomain.BaseDirectory + Constants.SettingModbus_dir + @"\modbusMapped.csv";

            try
            {
                using TextReader reader = new StreamReader(var_map_csv);
                var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = ";", Encoding = Encoding.UTF8, HasHeaderRecord = false };
                using var csv_reader = new CsvReader(reader, config);
                csv_reader.Context.RegisterClassMap<TCPModbusVarMap>();

                List<TCPModbusVarEntry> list_var_entry = csv_reader.GetRecords<TCPModbusVarEntry>().ToList();

                string s_log = "VAR MAP ENTRIES \r\n ----------------------\r\n";

                Globals.GetTheInstance().List_slave_entry.ForEach(slave_entry =>
                {
                    slave_entry.List_var_entry = list_var_entry.Where(var_entry => var_entry.Slave.Equals(slave_entry.Name)).ToList();
                    slave_entry.List_var_entry.ForEach(var_entry =>
                    {
                        var_entry.Value = Constants.Error_code.ToString();
                        var_entry.Read_range_grid = var_entry.Read_range_min.ToString() + "  -  " + var_entry.Read_range_max.ToString();
                        var_entry.Scaled_range_grid = $"{var_entry.Scaled_range_min.ToString("0.00", Globals.GetTheInstance().nfi)} - {var_entry.Scaled_range_max.ToString("0.00", Globals.GetTheInstance().nfi)}";
                        var_entry.Scale_factor = Functions.Calculate_scale_factor(var_entry);

                        s_log += $"SLAVE : {var_entry.Slave} / VAR : {var_entry.Name} / @MB : {var_entry.DirModbus} / READ RANGE : {var_entry.Read_range_grid } / SCALED RANGE : {var_entry.Scaled_range_grid } / SCALED FACTOR : {var_entry.Scale_factor.ToString("0.00000", Globals.GetTheInstance().nfi)} \r\n";
                    });

                    slave_entry.List_var_entry = slave_entry.List_var_entry.OrderBy(var_entry => var_entry.DirModbus).ToList();
                });

                Manage_logs.SaveLogValue(s_log);
            }
            catch (Exception ex)
            {
                read_ok = false;
                Manage_logs.SaveErrorValue($"{typeof(Manage_file).Name} -> {nameof(Load_var_map_entries)} -> {ex.Message}");
            }

            return read_ok;
        }

        #endregion

        #region Save var map entries

        public static bool Save_var_map_entries()
        {
            bool save_ok = true;

            string modbus_var_map_csv = AppDomain.CurrentDomain.BaseDirectory + Constants.SettingModbus_dir + @"\modbusMapped.csv";

            try
            {
                using TextWriter writer = new StreamWriter(modbus_var_map_csv, false);
                var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = ";", Encoding = Encoding.UTF8 };
                using var csv_writer = new CsvWriter(writer, config);
                csv_writer.Context.RegisterClassMap<TCPModbusVarMap>();

                Globals.GetTheInstance().List_slave_entry.ForEach(slave_entry =>
                {
                    slave_entry.List_var_entry.ForEach(var_entry =>
                    {
                        csv_writer.WriteRecord(var_entry);
                        csv_writer.NextRecord();
                        writer.Flush();
                    });
                });
            }
            catch (Exception ex)
            {
                save_ok = false;
                Manage_logs.SaveErrorValue($"{typeof(Manage_file).Name} -> {nameof(Save_var_map_entries)} -> {ex.Message}");
            }

            return save_ok;
        }

        #endregion




        #region Load TCU decodified entries

        public static bool Load_tcu_codified_status()
        {
            bool read_ok = true;

            try
            {
                var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = ";", Encoding = Encoding.UTF8, HasHeaderRecord = false };

                #region Codified CSV

                string codified_csv = AppDomain.CurrentDomain.BaseDirectory + Constants.SettingModbus_dir + @"\codifiedStatus.csv";
                using TextReader codified_reader = new StreamReader(codified_csv);
                using var codified_csv_reader = new CsvReader(codified_reader, config);
                codified_csv_reader.Context.RegisterClassMap<TCUCodifiedStatusMap>();

                IEnumerable<TCUCodifiedStatusEntry> records = codified_csv_reader.GetRecords<TCUCodifiedStatusEntry>();
                Globals.GetTheInstance().List_tcu_codified_status = records.ToList();
                Globals.GetTheInstance().List_tcu_codified_status = Globals.GetTheInstance().List_tcu_codified_status.ToList().OrderBy(x => x.DirModbus).ToList();

                Globals.GetTheInstance().List_tcu_codified_status.ForEach(codified_status => {
                    codified_status.List_status_mask = new List<string>();
                    codified_status.Value = Constants.Error_code.ToString();
                });

                #endregion

                #region Mask CSV

                string mask_csv = AppDomain.CurrentDomain.BaseDirectory + Constants.SettingModbus_dir + @"\statusMask.csv";
                using var mask_reader = new StreamReader(mask_csv);
                using var mask_csv_reader = new CsvReader(mask_reader, config);
                var anonymousTypeDefinition = new
                {
                    dir = default(int),
                    Status0 = string.Empty,
                    Status1 = string.Empty,
                    Status2 = string.Empty,
                    Status3 = string.Empty,
                    Status4 = string.Empty,
                    Status5 = string.Empty,
                    Status6 = string.Empty,
                    Status7 = string.Empty,
                    Status8 = string.Empty,
                    Status9 = string.Empty,
                    Status10 = string.Empty,
                    Status11 = string.Empty,
                    Status12 = string.Empty,
                    Status13 = string.Empty,
                    Status14 = string.Empty,
                    Status15 = string.Empty,
                };

                var mask_records = mask_csv_reader.GetRecords(anonymousTypeDefinition);
                foreach (var record in mask_records)
                {
                    TCUCodifiedStatusEntry? status_entry = Globals.GetTheInstance().List_tcu_codified_status.FirstOrDefault(tcu_codified => tcu_codified.DirModbus == record.dir);
                    if (status_entry != null)
                    {
                        status_entry.List_status_mask = new List<string> {
                                record.Status0,
                                record.Status1,
                                record.Status2,
                                record.Status3,
                                record.Status4,
                                record.Status5,
                                record.Status6,
                                record.Status7,
                                record.Status8,
                                record.Status9,
                                record.Status10,
                                record.Status11,
                                record.Status12,
                                record.Status13,
                                record.Status14,
                                record.Status15,
                            };
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                read_ok = false;
                Manage_logs.SaveErrorValue($"{typeof(Manage_file).Name} -> {nameof(Load_tcu_codified_status)} -> {ex.Message}");
            }

            return read_ok;
        }

        #endregion

        #region Save TCU decodified entries

        public static bool Save_tcu_decodified_entries()
        {
            bool save_ok = true;

            try
            {
                var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = ";", Encoding = Encoding.UTF8, HasHeaderRecord = false };

                //Codified var
                string codified_csv = AppDomain.CurrentDomain.BaseDirectory + Constants.SettingModbus_dir + @"\codifiedStatus.csv";
                using TextWriter codified_writer = new StreamWriter(codified_csv, false);
                using var codified_csv_writer = new CsvWriter(codified_writer, config);
                codified_csv_writer.Context.RegisterClassMap<TCUCodifiedStatusMap>();

                //Mask
                string mask_csv = AppDomain.CurrentDomain.BaseDirectory + Constants.SettingModbus_dir + @"\statusMask.csv";
                using TextWriter mask_writer = new StreamWriter(mask_csv, false);
                using var mask_csv_writer = new CsvWriter(mask_writer, config);
                List<object>? records = new();

                Globals.GetTheInstance().List_tcu_codified_status.ForEach((codified_entry) =>
                {
                    //Codified var
                    codified_csv_writer.WriteRecord(codified_entry);
                    codified_csv_writer.NextRecord();
                    codified_writer.Flush();

                    //Status MASK
                    if (codified_entry.List_status_mask.Count != 0)
                    {
                        records.Add(
                            new
                            {
                                dir = codified_entry.DirModbus,
                                Status0 = codified_entry.List_status_mask[0],
                                Status1 = codified_entry.List_status_mask[1],
                                Status2 = codified_entry.List_status_mask[2],
                                Status3 = codified_entry.List_status_mask[3],
                                Status4 = codified_entry.List_status_mask[4],
                                Status5 = codified_entry.List_status_mask[5],
                                Status6 = codified_entry.List_status_mask[6],
                                Status7 = codified_entry.List_status_mask[7],
                                Status8 = codified_entry.List_status_mask[8],
                                Status9 = codified_entry.List_status_mask[9],
                                Status10 = codified_entry.List_status_mask[10],
                                Status11 = codified_entry.List_status_mask[11],
                                Status12 = codified_entry.List_status_mask[12],
                                Status13 = codified_entry.List_status_mask[13],
                                Status14 = codified_entry.List_status_mask[14],
                                Status15 = codified_entry.List_status_mask[15],
                            });
                    }
                });

                mask_csv_writer.WriteRecords(records);
                mask_csv_writer.Flush();
            }
            catch (Exception ex)
            {
                save_ok = false;
                Manage_logs.SaveErrorValue(typeof(Manage_file).Name + " ->  " + nameof(Save_tcu_decodified_entries) + " -> " + ex.Message.ToString());
            }

            return save_ok;
        }

        #endregion




        #region Load modbus commands

        public static bool Load_modbus_commands()
        {
            bool read_ok = true;

            try
            {
                Globals.GetTheInstance().List_tcu_command = new();

                string command_file = AppDomain.CurrentDomain.BaseDirectory + Constants.SettingModbus_dir + @"\modbusCommands.txt";
                using StreamReader file = new(command_file);

                int index_numparam = 0;
                TCUCommand scscomand = new();
                COMMAND_FIELD command_field = COMMAND_FIELD.INDEX;
                string? ln;
                while ((ln = file.ReadLine()) != null)
                {
                    switch (command_field)
                    {
                        case COMMAND_FIELD.INDEX:
                            {
                                scscomand = new();
                                scscomand.Name_params = new();
                                scscomand.Type_params = new();
                                scscomand.Index = int.Parse(ln);
                                command_field = COMMAND_FIELD.NAME;
                                break;
                            }

                        case COMMAND_FIELD.NAME:
                            {
                                scscomand.Name = ln;
                                command_field = COMMAND_FIELD.NUM_PARAM;
                                break;
                            }

                        case COMMAND_FIELD.NUM_PARAM:
                            {
                                scscomand.Num_params = int.Parse(ln);
                                if (scscomand.Num_params != 0)
                                {
                                    index_numparam = 0;
                                    command_field = COMMAND_FIELD.VAR_NAME;
                                }

                                else
                                    command_field = COMMAND_FIELD.SEPARATION;


                                break;
                            }

                        case COMMAND_FIELD.VAR_NAME:
                            {
                                scscomand.Name_params.Add(ln);
                                command_field = COMMAND_FIELD.VAR_TYPE;
                                break;
                            }

                        case COMMAND_FIELD.VAR_TYPE:
                            {
                                scscomand.Type_params.Add((TypeCode)byte.Parse(ln));

                                if (++index_numparam == scscomand.Num_params)
                                    command_field = COMMAND_FIELD.SEPARATION;

                                else
                                    command_field = COMMAND_FIELD.VAR_NAME;

                                break;
                            }

                        case COMMAND_FIELD.SEPARATION:
                            {
                                Globals.GetTheInstance().List_tcu_command.Add(scscomand);
                                command_field = COMMAND_FIELD.INDEX;
                                break;
                            }

                    }
                }
                file.Close();
            }
            catch (Exception ex)
            {
                read_ok = false;
                Manage_logs.SaveErrorValue(typeof(Manage_file).Name + " ->  " + nameof(Load_modbus_commands) + " -> " + ex.Message.ToString());
            }

            return read_ok;
        }

        #endregion



        #region Load main drive slope correction

        public static  bool Load_main_drive_slope_correction()
        {
            bool read_ok = false;
            try
            {
                var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = ";", Encoding = Encoding.UTF8, HasHeaderRecord = true };

                string slope_correction_csv = AppDomain.CurrentDomain.BaseDirectory + Constants.SettingApp_dir + @"\maindrive_slope_correction.csv";
                using TextReader slope_correction_reader = new StreamReader(slope_correction_csv);
                using var slope_correction_csv_reader = new CsvReader(slope_correction_reader, config);
                slope_correction_csv_reader.Context.RegisterClassMap<SlopeCorrectionMap>();

                IEnumerable<SlopeCorrection> records = slope_correction_csv_reader.GetRecords<SlopeCorrection>();
                Globals.GetTheInstance().Dictionary_slope_correction = new();
                records.ToList().ForEach(record => Globals.GetTheInstance().Dictionary_slope_correction.TryAdd<double, double>(record.Alpha_TT, record.Factor));
   
                Globals.GetTheInstance().List_slope_correction_alphaTT = Globals.GetTheInstance().Dictionary_slope_correction.Keys.ToList();
                Globals.GetTheInstance().List_slope_correction_alphaTT.Sort();

                read_ok = true;
            }
            catch (Exception ex)
            {
                read_ok = false;
                Manage_logs.SaveErrorValue(typeof(Manage_file).Name + " ->  " + nameof(Load_main_drive_slope_correction) + " -> " + ex.Message.ToString());
            }

            return read_ok;
        }

        #endregion


    }
}
