using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SBP_TRACKER
{
    public class Manage_web_api
    {
        private bool m_tracker_state_registered;
        private bool m_tracker_data_registered;

        public bool m_API_data_started { get; set; }
        private DispatcherTimer DispatcherTimer_modbus_api_data = new();
        private DispatcherTimer DispatcherTimer_modbus_api_state = new();


        #region Constructor

        public Manage_web_api()
        {
            m_tracker_state_registered = false;
            m_tracker_data_registered = false;
            m_API_data_started = false;

            DispatcherTimer_modbus_api_data = new DispatcherTimer();
            DispatcherTimer_modbus_api_data.Tick += new EventHandler(SendModbusAPIData_timer_Tick);

            DispatcherTimer_modbus_api_state = new DispatcherTimer();
            DispatcherTimer_modbus_api_state.Tick += new EventHandler(SendModbusAPIState_timer_Tick);
        }

        #endregion





        #region Modbus API state

        public void Start_timer_modbus_API_state(BIT_STATE bit_state, int interval)
        {
            if (bit_state == BIT_STATE.ON)
            {
                DispatcherTimer_modbus_api_state.Interval = new TimeSpan(0, 0, 0, 0, interval);
                DispatcherTimer_modbus_api_state.Start();
            }
            else
                DispatcherTimer_modbus_api_state.Stop();
        }

        private async void SendModbusAPIState_timer_Tick(object sender, EventArgs e)
        {
            Start_timer_modbus_API_state(BIT_STATE.OFF, 0);
            if (m_API_data_started)
            {
                bool send_ok = false;
                try
                {
                    send_ok = await SendModbusAPIState();
                }
                catch (TaskCanceledException ex)
                {
                    Manage_logs.SaveAPIValue($"Error send API STATE / CODE : {HttpStatusCode.NotFound} / MESSAGE : { ex.Message}");
                }

                int interval = send_ok ? (int)Globals.GetTheInstance().Send_state_interval_web_API : (int)Globals.GetTheInstance().Wait_error_conn_interval_web_API;
                Start_timer_modbus_API_state(BIT_STATE.ON, interval);
            }
        }

        public async Task<bool> SendModbusAPIState()
        {
            bool send_ok = true;

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"{Globals.GetTheInstance().API_root}"),
                Timeout= TimeSpan.FromMilliseconds((double)Globals.GetTheInstance().HTTP_timeout)
            };
            try
            {
                var tracker_item = new TrackerState
                {
                    ID =(int)Globals.GetTheInstance().Tracker_ID,
                    Name = Globals.GetTheInstance().Tracker_name,
                    StateUpdated = true,
                };

                string json_item = await Task.Run(() => JsonConvert.SerializeObject(tracker_item));
                var content = new StringContent(json_item, Encoding.UTF8, "application/json");

                using var response =
                    m_tracker_state_registered ?
                    await httpClient.PutAsync($"{Globals.GetTheInstance().State_controller_route}/{tracker_item.ID}", content) :
                    await httpClient.PostAsync(Globals.GetTheInstance().State_controller_route, content);

                string s_request_type = m_tracker_state_registered ? "PUT" : "POST";
                string s_log = $"SEND MODBUS API STATE /  REQUEST TYPE : { s_request_type } / STATUS CODE : { response.StatusCode }";

                string result_value = string.Empty;
                if (response.IsSuccessStatusCode)
                    m_tracker_state_registered = true;

                else
                {
                    result_value = response.Content.ReadAsStringAsync().Result;
                    if (response.StatusCode == HttpStatusCode.BadRequest && !string.IsNullOrEmpty(result_value))
                        if (result_value.Contains("Tracker ID already exist"))
                            m_tracker_state_registered = true;
                }

                s_log += $"/ RETURN VALUE: { response.StatusCode } / SUCCESS : { response.IsSuccessStatusCode } / RESULT : { result_value }";
                Manage_logs.SaveAPIValue(s_log);
            }
            catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
            {
                // handle 404 exceptions
                send_ok = false;
                Manage_logs.SaveAPIValue($"Error send API STATE / CODE : {HttpStatusCode.NotFound} / MESSAGE : { ex.Response}");
            }
            catch (HttpRequestException ex)
            {
                // Handle exception.
                send_ok = false;
                Manage_logs.SaveAPIValue($"Error send API STATE / CODE : {ex.StatusCode} / MESSAGE : { ex.Message}");

                //Tras un reseteo del server reiniciar las peticiones a POST
                if (m_tracker_state_registered && ex.StatusCode == HttpStatusCode.NotFound)
                    m_tracker_state_registered = false;
            }
            catch (WebException ex)
            {
                // handle other web exceptions
                send_ok = false;
                Manage_logs.SaveAPIValue($"Error send API STATE / CODE : {ex.Status} / MESSAGE : { ex.Message}");
            }
            catch (TaskCanceledException ex) {
                send_ok = false;
                Manage_logs.SaveAPIValue($"Error send API STATE / CODE : {ex.Message} / MESSAGE : { ex.Message}");
            }
            catch (Exception ex)
            {
                send_ok = false;
                Manage_logs.SaveAPIValue($"Error send API STATE / CODE : {ex.Message} / MESSAGE : { ex.Message}");
            }


            return send_ok;
        }

        #endregion


        #region Modbus API data

        public void Start_timer_modbus_API_data(BIT_STATE bit_state, int interval)
        {
            if (bit_state == BIT_STATE.ON)
            {
                DispatcherTimer_modbus_api_data.Interval = new TimeSpan(0, 0, 0, 0, interval);
                DispatcherTimer_modbus_api_data.Start();
            }
            else
                DispatcherTimer_modbus_api_data.Stop();
        }


        private async void SendModbusAPIData_timer_Tick(object sender, EventArgs e)
        {
            Start_timer_modbus_API_data(BIT_STATE.OFF, 0);

            bool send_ok = false;
            if (m_API_data_started)
            {
                try
                {
                    send_ok = await SendModbusAPIData();
                }
                catch (TaskCanceledException ex)
                {
                    Manage_logs.SaveAPIValue($"Error send API DATA / CODE : {HttpStatusCode.NotFound} / MESSAGE : { ex.Message}");
                }

                int interval = send_ok ? (int)Globals.GetTheInstance().Send_data_interval_web_API : (int)Globals.GetTheInstance().Wait_error_conn_interval_web_API;
                Start_timer_modbus_API_data(BIT_STATE.ON, interval);
            }
        }

        public async Task<bool> SendModbusAPIData()
        {
            bool send_ok = true;
            if (!string.IsNullOrEmpty(Globals.GetTheInstance().API_root))
            {
                List<TrackerVar>? current_list_var = new();
                Globals.GetTheInstance().List_slave_entry.ForEach(slave_entry => {
                    slave_entry.List_var_entry.ForEach(var_entry => {
                        if (!string.IsNullOrEmpty(var_entry.Value))
                        {
                            if (double.TryParse(var_entry.Value, NumberStyles.Any, Globals.GetTheInstance().nfi, out double val))
                                current_list_var.Add(new TrackerVar { Slave = var_entry.Slave, Name = var_entry.Name, Value = val, Unit= var_entry.Unit });
                        }
                    });
                });

                TCPModbusSlaveEntry? slave_entry = Globals.GetTheInstance().List_slave_entry.FirstOrDefault(slave_entry => slave_entry.Slave_type == SLAVE_TYPE.TCU);
                if (slave_entry != null)
                {
                    Globals.GetTheInstance().List_tcu_codified_status.ForEach(codified_status =>
                    {
                        if (!string.IsNullOrEmpty(codified_status.Value))
                        {
                            if (double.TryParse(codified_status.Value, NumberStyles.Any, Globals.GetTheInstance().nfi, out double val))
                                current_list_var.Add(new TrackerVar { Slave = slave_entry.Name, Name = codified_status.Name, Value = val, Unit = codified_status.Unit });
                        }
                    });
                }


                var update_item = new TrackerData
                {
                    ID = (int)Globals.GetTheInstance().Tracker_ID,
                    Lastregister = DateTime.Now,
                    Listvar = current_list_var,
                };

                string json_item = await Task.Run(() => JsonConvert.SerializeObject(update_item));
                var content = new StringContent(json_item, Encoding.UTF8, "application/json");

                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri($"{Globals.GetTheInstance().API_root}"),
                    Timeout = TimeSpan.FromMilliseconds((double)Globals.GetTheInstance().HTTP_timeout)
                };
                try
                {
                    using var response =
                        m_tracker_data_registered ?
                        await httpClient.PutAsync($"{Globals.GetTheInstance().Data_controller_route}/{update_item.ID}", content) :
                        await httpClient.PostAsync(Globals.GetTheInstance().Data_controller_route, content);

                    string s_request_type = m_tracker_data_registered ? "PUT" : "POST";
                    string s_log = $"SEND MODBUS API DATA.  REQUEST TYPE : { s_request_type } / STATUS CODE : { response.StatusCode }";

                    string result_value = string.Empty;
                    if (response.IsSuccessStatusCode)
                        m_tracker_data_registered = true;

                    else
                    {
                        result_value = response.Content.ReadAsStringAsync().Result;

                        if (response.StatusCode == HttpStatusCode.BadRequest && !string.IsNullOrEmpty(result_value))
                            if (result_value.Contains("Tracker ID already exist"))
                                m_tracker_data_registered = true;
                    }

                    s_log += $"/ RETURN VALUE: { response.StatusCode } / SUCCESS : { response.IsSuccessStatusCode } / RESULT : { result_value }";
                    Manage_logs.SaveAPIValue(s_log);
                }
                catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
                {
                    // handle 404 exceptions
                    send_ok = false;
                    Manage_logs.SaveAPIValue($"Error send API DATA / CODE : {HttpStatusCode.NotFound} / MESSAGE : { ex.Response}");
                }
                catch (HttpRequestException ex)
                {
                    // Handle exception.
                    send_ok = false;
                    Manage_logs.SaveAPIValue($"Error send API DATA / CODE : {ex.StatusCode} / MESSAGE : { ex.Message}");

                    //Tras un reseteo del server reiniciar las peticiones a POST
                    if (m_tracker_data_registered && ex.StatusCode == HttpStatusCode.NotFound)
                        m_tracker_data_registered = false;
                }
                catch (WebException ex)
                {
                    //Other exceptions
                    send_ok = false;
                    Manage_logs.SaveAPIValue($"Error send API DATA / CODE : {ex.Status} / MESSAGE : { ex.Message}");
                }
                catch (TaskCanceledException ex)
                {
                    send_ok = false;
                    Manage_logs.SaveAPIValue($"Error send API DATA / CODE : {ex.Message} / MESSAGE : { ex.Message}");
                }
                catch (Exception ex)
                {
                    send_ok = false;
                    Manage_logs.SaveAPIValue($"Error send API STATE / CODE : {ex.Message} / MESSAGE : { ex.Message}");
                }
            }

            return send_ok;
        }

        #endregion


    }
}
