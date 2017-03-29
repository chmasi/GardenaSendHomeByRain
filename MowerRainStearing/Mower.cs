using Gardena.NET;
using Gardena.NET.Models;
using Netatmo.Net;
using Netatmo.Net.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;

using ms;
using System.Text;
using System.Threading.Tasks;

namespace MowerRainSteering
{
    public partial class Mower : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);
        private System.ComponentModel.IContainer components;
        //private System.Diagnostics.EventLog eventLogMower;
        //readonly cProtection protection = new cProtection();

        Locations myGardenaLocation = new Locations();
        Devices myGardenaDevices = new Devices();

        readonly NetatmoApi NetatmoAPI = null;
        readonly GardenaAPI GardenaApi = null;

        Boolean bemailSend30 = false;
        Boolean bemailSend60 = false;

        DateTime time30snapshot;
        DateTime time60snapshot;

       // public Mower(string[] args)
        public Mower()
        {
            InitializeComponent();
            ms.cEvent.createEventLog();
            NetatmoAPI = new NetatmoApi(Properties.Settings.Default.netatmoClientId, Properties.Settings.Default.netatmoClientSecret);
            GardenaApi = new GardenaAPI();
            NetatmoAPI.LoginSuccessful += NetatmoAPI_LoginSuccessful;
            NetatmoAPI.LoginFailed += NetatmoAPI_LoginFailed;
            GardenaApi.LoginSuccessful += GardenaApi_LoginSuccessful;
            GardenaApi.LoginFailed += GardenaApi_LoginFailed;

            Timer_Elapsed(this, null);
        }
        private async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                TimeSpan start = new TimeSpan(Properties.Settings.Default.startTimeForChecking, 0, 0);
                TimeSpan end = new TimeSpan(Properties.Settings.Default.endTimeForChecking, 0, 0);
                TimeSpan now = DateTime.Now.TimeOfDay;

                cEvent.WriteInfo("Monitoring the MowerRainSystem is active");

                if (helpers.Time.TimeBetween(DateTime.Now, start, end))
                {
                    // TODO: check for rain and send mower to parking
                    var data = await NetatmoAPI.GetStationsData();
                    if (data.ServiceError == "Please login first")
                    {
                        NetatmoAPI.Login(Properties.Settings.Default.netatmoLogin, Properties.Settings.Default.netatmoPassword, new[] { NetatmoScope.read_station });
                    }
                    else
                    {
                        NetatmoAPI_LoginSuccessful(this);
                    }

                }
            }
            catch(Exception exp)
            {
                cEvent.WriteError("Error Timeer_Elapsed : " + Environment.NewLine +  exp.Message);
                ms.cDebugLog.Log("Error Timeer_Elapsed : " + Environment.NewLine + exp.Message, true);
            }
        }
        private void GardenaApi_LoginFailed(object sender)
        {
            try
            {
                var email = Properties.Settings.Default;
                cEvent.WriteError("Gardena Login failed, see logfile for more details!");

                ms.cSendEmail.SendMail(email.emailFromEmail, email.emailForStatusMessages, "Gardena Login failed", "Garden Login failed, see logfile for more details!", email.emailAccountUser, email.emailAccountUserPassword, email.emailSMTP, email.emailSMTPPort, email.emailSSL);
            }
            catch(Exception exp)
            {
                ms.cDebugLog.Log("Error GardenaApi_LoginFailed: " + Environment.NewLine + exp.Message, true);
            }
        }
        private void GardenaApi_LoginSuccessful(object sender)
        {
            try
            {
                myGardenaLocation = GardenaApi.GetGardenaLocation(((GardenaAPI)sender).myLogin.sessions.user_id, ((GardenaAPI)sender).myLogin.sessions.token);
                myGardenaDevices = GardenaApi.GetGardenaDevices(myGardenaLocation.locations[0].id, ((GardenaAPI)sender).myLogin.sessions.token);

                GardenaApi.GardenaSendCommand(myGardenaDevices.devices[1].id, myGardenaLocation.locations[0].id, ((GardenaAPI)sender).myLogin.sessions.token, "park_until_next_timer");
            }
            catch (Exception exp)
            {
                ms.cDebugLog.Log("Error GardenaApi_LoginSuccessful: " + Environment.NewLine + exp.Message, true);
            }
        }
        private void NetatmoAPI_LoginFailed(object sender)
        {
            try
            {
                var email = Properties.Settings.Default;
                cEvent.WriteError("Netatmo Login failed, see logfile for more details!");
                ms.cSendEmail.SendMail(email.emailFromEmail, email.emailForStatusMessages, "Netatmo Login failed", "Netatmo Login failed, see logfile for more details!", email.emailAccountUser, email.emailAccountUserPassword, email.emailSMTP, email.emailSMTPPort, email.emailSSL);
            }
            catch (Exception exp)
            {
                ms.cDebugLog.Log("Error NetatmoAPI_LoginFailed: " + Environment.NewLine + exp.Message, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        private async void NetatmoAPI_LoginSuccessful(object sender)
        {
            try
            {
                var data = await NetatmoAPI.GetStationsData();
                var myDevice = data.Result.Data.Devices[0];
                foreach (var item in data.Result.Data.Devices[0].Modules)
                {
                    if (item.Type == "NAModule1")
                    {
                        var email = Properties.Settings.Default;
                        if (item.BatteryPercent <= 15 && email.sendEmailsStatus == true)
                        {
                            ms.cSendEmail.SendMail(email.emailFromEmail, email.emailForStatusMessages, "Battery Status", "Battery of " + item.ModuleName + " is less or equal to 15%", email.emailAccountUser, email.emailAccountUserPassword, email.emailSMTP, email.emailSMTPPort, email.emailSSL);
                        }
                    }

                    if (item.Type == "NAModule3")
                    {
                        var email = Properties.Settings.Default;
                        if (item.BatteryPercent <= 15)
                        {
                            ms.cSendEmail.SendMail(email.emailFromEmail, email.emailForStatusMessages, "Battery Status", "Battery of " + item.ModuleName + " is less or equal to 15%", email.emailAccountUser, email.emailAccountUserPassword, email.emailSMTP, email.emailSMTPPort, email.emailSSL);
                        }

                        string start = DateTime.Today.ToString("dd.MM.yyyy HH:mm:ss");
                        string end = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

                        // var details = item.ModuleName + " - Firmware:" + item.Firmware + " - Id:" + item.Id + " - Lastseen:" + FromUnixTime(item.LastSeen).ToString("dd.MM.yyyy HH:mm:ss") + " - Lastmessage:" + FromUnixTime(item.LastMessage).ToString("dd.MM.yyyy HH:mm:ss");
                        var rainmeasure30 = await NetatmoAPI.GetMeasure(data.Result.Data.Devices[0].Id, Scale.ThirtyMinutes, new[] { MeasurementType.sum_rain }, data.Result.Data.Devices[0].Modules[1].Id, false, Convert.ToDateTime(start), Convert.ToDateTime(end), true, 1024, true);
                        var rainmeasure60 = await NetatmoAPI.GetMeasure(data.Result.Data.Devices[0].Id, Scale.OneHour, new[] { MeasurementType.sum_rain }, data.Result.Data.Devices[0].Modules[1].Id, false, Convert.ToDateTime(start), Convert.ToDateTime(end), true, 1024, true);
                        //var rainmeasure180 = await NetatmoAPI.GetMeasure(data.Result.Data.Devices[0].Id, Scale.ThreeHours, new[] { MeasurementType.sum_rain }, data.Result.Data.Devices[0].Modules[1].Id, false, Convert.ToDateTime(start), Convert.ToDateTime(end), true, 1024, true);
                        //var rainmeasure1d = await NetatmoAPI.GetMeasure(data.Result.Data.Devices[0].Id, Scale.OneDay, new[] { MeasurementType.sum_rain }, data.Result.Data.Devices[0].Modules[1].Id, false, Convert.ToDateTime(start), Convert.ToDateTime(end), true, 1024, true);
                        //var rainmeasure1w = await NetatmoAPI.GetMeasure(data.Result.Data.Devices[0].Id, Scale.OneWeek, new[] { MeasurementType.sum_rain }, data.Result.Data.Devices[0].Modules[1].Id, false, Convert.ToDateTime(start), Convert.ToDateTime(end), true, 1024, true);
                        //var rainmeasure1m = await NetatmoAPI.GetMeasure(data.Result.Data.Devices[0].Id, Scale.OneMonth, new[] { MeasurementType.sum_rain }, data.Result.Data.Devices[0].Modules[1].Id, false, Convert.ToDateTime(start), Convert.ToDateTime(end), true, 1024, true);

                        var lastDate = rainmeasure30.Result.Measurements[rainmeasure30.Result.Measurements.Count() - 1].DateTime.ToString("dd.MM.yyyy HH:mm:ss");
                        var lastData30 = rainmeasure30.Result.Measurements[rainmeasure30.Result.Measurements.Count() - 1].MeasurementValues[0].Value;
                        var lastData60 = rainmeasure60.Result.Measurements[rainmeasure60.Result.Measurements.Count() - 1].MeasurementValues[0].Value;
                        //var lastData180 = rainmeasure180.Result.Measurements[rainmeasure180.Result.Measurements.Count() - 1].MeasurementValues[0].Value;
                        //var lastData1d = rainmeasure1d.Result.Measurements[rainmeasure1d.Result.Measurements.Count() - 1].MeasurementValues[0].Value;



                        int time30check = DateTime.Compare(DateTime.Now.AddMinutes(30), time30snapshot);
                        if (time30check < 1)
                        {
                            bemailSend30 = false;
                        }
                        int time60check = DateTime.Compare(DateTime.Now.AddHours(1), time60snapshot);
                        if (time60check < 1)
                        {
                            bemailSend60 = false;
                        }

                        if ((double)lastData30 >= email.rain && bemailSend30 == false)
                        {
                            time30snapshot = DateTime.UtcNow;
                            bemailSend30 = true;
                            if (email.sendEmailsStatus == true)
                            {
                                ms.cSendEmail.SendMail(email.emailFromEmail, email.emailForStatusMessages, "Rain Status marker reached within 30 minutes => ", "Rain measure: " + email.rain.ToString() + Environment.NewLine + "Mower will be send back home and is waiting for the next schedule!", email.emailAccountUser, email.emailAccountUserPassword, email.emailSMTP, email.emailSMTPPort, email.emailSSL);
                            }
                            GardenaApi.GardenaLogin(Properties.Settings.Default.gardenaLogin, Properties.Settings.Default.gardenaPassword);
                        }

                        if ((double)lastData60 >= email.rain && bemailSend60 == false)
                        {
                            time60snapshot = DateTime.UtcNow;
                            bemailSend60 = true;
                            if (email.sendEmailsStatus == true)
                            {
                                ms.cSendEmail.SendMail(email.emailFromEmail, email.emailForStatusMessages, "Rain Status marker reached within 1 hour => ", "Rain measure: " + email.rain.ToString() + Environment.NewLine + "Mower will be send back home and is waiting for the next schedule!", email.emailAccountUser, email.emailAccountUserPassword, email.emailSMTP, email.emailSMTPPort, email.emailSSL);
                            }
                            GardenaApi.GardenaLogin(Properties.Settings.Default.gardenaLogin, Properties.Settings.Default.gardenaPassword);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                var email = Properties.Settings.Default;
                cEvent.WriteError("Error NetatmoAPI_LoginSuccessful : " + Environment.NewLine + exp.Message);
                ms.cDebugLog.Log("Error NetatmoAPI_LoginSuccessful : " + Environment.NewLine + exp.Message, true);
                ms.cSendEmail.SendMail(email.emailFromEmail, email.emailForStatusMessages, "NetatmoAPI_LoginSuccessful", "Error: " + Environment.NewLine +  exp.Message, email.emailAccountUser, email.emailAccountUserPassword, email.emailSMTP, email.emailSMTPPort, email.emailSSL);
            }
        }
        protected override void OnStart(string[] args)
        {
            cEvent.WriteInfo("In OnStart");
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            // Set up a checkRaintimer to trigger every 10 minutes.
            System.Timers.Timer checkRaintimer = new System.Timers.Timer();
            checkRaintimer.Interval = 60000 * Properties.Settings.Default.intervalToCheckRainSensor; // 10 minutes
            //timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            checkRaintimer.Elapsed += Timer_Elapsed;
            checkRaintimer.Start();

            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            cEvent.WriteInfo("Started");
        }
        protected override void OnContinue()
        {
            cEvent.WriteInfo("In OnContinue.");
        }
        protected override void OnStop()
        {
            cEvent.WriteInfo("In OnStop");
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            cEvent.WriteInfo("Stopped");
        }
    }

    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public long dwServiceType;
        public ServiceState dwCurrentState;
        public long dwControlsAccepted;
        public long dwWin32ExitCode;
        public long dwServiceSpecificExitCode;
        public long dwCheckPoint;
        public long dwWaitHint;
    };
}
