using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace ms
{

    public class cEvent
    {
        static string sName = Process.GetCurrentProcess().ProcessName.Replace(".vshost", "");

        public static void createEventLog()
        {
            if (!EventLog.SourceExists(sName))
            {
                EventLog.CreateEventSource(sName, "Application");
            }
        }
        public static void WriteInfo(string sMessage)
        {
            
            try
            {
                EventLog appLog = new EventLog("Application");
                appLog.Source = sName;
                appLog.WriteEntry(sMessage, EventLogEntryType.Information);
            }
            catch (Exception exp)
            { cDebugLog.Log("Error in EventWriter: WriteInfo " + exp.Message + " StackTrace: " + exp.StackTrace, true); }
        }

        public static void WriteWarning(string sMessage)
        {
            try
            {
                EventLog appLog = new EventLog("Application");
                appLog.Source = sName;
                appLog.WriteEntry(sMessage, EventLogEntryType.Warning);
            }
            catch(Exception exp)
            { cDebugLog.Log("Error in EventWriter: WriteWarning " + exp.Message + " StackTrace: " + exp.StackTrace, true); }
        }

        public static void WriteError(string sMessage)
        {
            try
            {
                EventLog appLog = new EventLog("Application");
                appLog.Source = sName;
                appLog.WriteEntry(sMessage, EventLogEntryType.Error);
            }
            catch(Exception exp)
            { cDebugLog.Log("Error in EventWriter: WriteError " + exp.Message + " StackTrace: " + exp.StackTrace, true); }
        }
    }
}