using AMS.Profile;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBP_TRACKER
{
    internal class Manage_file
    {
        #region Create directories

        public static bool Create_directories()
        {
            bool create_ok = true;

            string[] dirs = new string[] { Constants.SettingApp_dir, Constants.SettingModbus_dir, Constants.Log_dir, Constants.Report_dir };

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
                Manage_logs.SaveErrorValue(typeof(Manage_file).Name + " ->  " + nameof(Create_directories) + " -> " + ex.Message.ToString());
            }

            return  create_ok;
        }

        #endregion

        #region Create files

        public static bool Create_files()
        {
            bool create_ok = true;

            try
            {
                string[] files = new string[] { Constants.SettingApp_dir + @"\setting.xml", Constants.SettingModbus_dir + @"\modbusSlave.csv", Constants.SettingModbus_dir + @"\modbusMapped.csv" };

                files.ToList().ForEach(file =>
                {
                    if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + file))
                    {
                        using FileStream fs = File.Create(AppDomain.CurrentDomain.BaseDirectory + file);
                        fs.Close();
                    }
                });

                //Create datalog.csv and header
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + Constants.Report_dir + @"\dataLog.csv"))
                {
                    using FileStream fs = File.Create(AppDomain.CurrentDomain.BaseDirectory + Constants.Report_dir + @"\dataLog.csv");
                    fs.Close();

                    using StreamWriter stream_writer = new(AppDomain.CurrentDomain.BaseDirectory + Constants.Report_dir + @"\dataLog.csv");
                    stream_writer.WriteLine("DateTime;Slave;Var Name;Var type;@ modbus;Value");
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
                Profile profile = new Xml(AppDomain.CurrentDomain.BaseDirectory + Constants.SettingApp_dir + @"\setting.xml");
                Globals.GetTheInstance().Record_data_interval = int.Parse(profile.GetValue("General", "Record_data_interval", 1000).ToString());
                Globals.GetTheInstance().Modbus_start_address = int.Parse(profile.GetValue("Modbus", "Start_address", 500).ToString());
                Globals.GetTheInstance().Modbus_read_interval = int.Parse(profile.GetValue("Modbus", "Read_interval", 1000).ToString());
                Globals.GetTheInstance().Modbus_timeout = int.Parse(profile.GetValue("Modbus", "Timeout", 10000).ToString());
            }
            catch (Exception ex)
            {
                load_ok = false;
                Manage_logs.SaveErrorValue(typeof(Manage_file).Name + " ->  " + nameof(Load_app_setting) + " -> " + ex.Message.ToString());
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
                profile.SetValue("General", "Record_data_interval", Globals.GetTheInstance().Record_data_interval);
                profile.SetValue("Modbus", "Start_address", Globals.GetTheInstance().Modbus_start_address);
                profile.SetValue("Modbus", "Read_interval", Globals.GetTheInstance().Modbus_read_interval);
                profile.SetValue("Modbus", "Timeout", Globals.GetTheInstance().Modbus_timeout);
            }
            catch (Exception ex)
            {
                save_ok = false;
                Manage_logs.SaveErrorValue(typeof(Manage_file).Name + " ->  " + nameof(Save_app_setting) + " -> " + ex.Message.ToString());
            }

            return save_ok;
        }

        #endregion



        #region Read modbus slave entries

        public static bool Read_modbus_slave_entries()
        {
            bool read_ok = true;

            string modbus_slave_csv = AppDomain.CurrentDomain.BaseDirectory + @"SettingModbusSlave\modbusSlave.csv";

            try
            {
                using TextReader reader = new StreamReader(modbus_slave_csv);
                var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = ";", Encoding = Encoding.UTF8, HasHeaderRecord = false, MissingFieldFound = null };
                using var csv_reader = new CsvReader(reader, config);
                csv_reader.Context.RegisterClassMap<TCPModbusSlaveMap>();

                IEnumerable<TCPModbusSlaveEntry> records = csv_reader.GetRecords<TCPModbusSlaveEntry>();
                Globals.GetTheInstance().List_modbus_slave_entry = records.ToList();

                Globals.GetTheInstance().List_modbus_slave_entry.ForEach(entry =>
                {
                    entry.Connected = false;
                    entry.List_modbus_var = new List<TCPModbusVar>();
                });
            }
            catch (Exception ex)
            {
                read_ok = false;
                Manage_logs.SaveErrorValue(typeof(Manage_file).Name + " ->  " + nameof(Read_modbus_slave_entries) + " -> " + ex.Message.ToString());
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
                foreach (TCPModbusSlaveEntry entry in Globals.GetTheInstance().List_modbus_slave_entry)
                {
                    csv_writer.WriteRecord(entry);
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


        #region Read var map entries
        public static bool Read_var_map_entries()
        {
            bool read_ok = true;

            string var_map_csv = AppDomain.CurrentDomain.BaseDirectory + Constants.SettingModbus_dir + @"\modbusMapped.csv";

            try
            {
                using TextReader reader = new StreamReader(var_map_csv);
                var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = ";", Encoding = Encoding.UTF8, HasHeaderRecord = false };
                using var csv_reader = new CsvReader(reader, config);
                csv_reader.Context.RegisterClassMap<TCPModbusVarMap>();

                IEnumerable<TCPModbusVar> records = csv_reader.GetRecords<TCPModbusVar>();
                List<TCPModbusVar> list_map = records.ToList();

                Globals.GetTheInstance().List_modbus_slave_entry.ForEach(entry =>
                {
                    entry.List_modbus_var = list_map.Where(x => x.Slave.Equals(entry.Name)).ToList();
                    entry.List_modbus_var.ForEach(var_map => var_map.SType = DataConverter.Type_code_to_string(var_map.Type));
                });
            }
            catch (Exception ex)
            {
                read_ok = false;
                Manage_logs.SaveErrorValue(typeof(Manage_file).Name + " ->  " + nameof(Read_var_map_entries) + " -> " + ex.Message.ToString());
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

                Globals.GetTheInstance().List_modbus_slave_entry.ForEach((entry) =>
                {
                    entry.List_modbus_var.ForEach(x =>
                    {
                        csv_writer.WriteRecord(x);
                        csv_writer.NextRecord();
                        writer.Flush();
                    });
                });
            }
            catch (Exception ex) {
                save_ok = false;
                Manage_logs.SaveErrorValue(typeof(Manage_file).Name + " ->  " + nameof(Save_var_map_entries) + " -> " + ex.Message.ToString());
            }

            return save_ok;
        }

        #endregion

    }
}
