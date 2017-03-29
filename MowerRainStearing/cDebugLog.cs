using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Configuration;

namespace ms
{
    public static class cDebugLog
    {
        public static void Log(string slogMessage, /*[Optional]*/ bool bLog, [Optional] string sFileName)
        {
            if(String.IsNullOrEmpty(sFileName))
            {
                sFileName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            }

            if (bLog == true)
            {
                try
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + "Logfiles\\" + sFileName + "_" + DateTime.Now.ToString("ddMMyyyy") + ".txt";

                    // This text is added only once to the file. 
                    if (!File.Exists(path))
                    {
                        // Create a file to write to. 
                        string createText = "LogFile created" + Environment.NewLine;
                        File.WriteAllText(path, createText);
                    }

                    string appendText = DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString(); // + Environment.NewLine;
                    File.AppendAllText(path, appendText);
                    appendText = " => " + slogMessage + Environment.NewLine;
                    File.AppendAllText(path, appendText);
                    //appendText = "-----------------------------------------------------------------" + Environment.NewLine;
                    //File.AppendAllText(path, appendText);

                    // Open the file to read from. 
                    //string readText = File.ReadAllText(path);
                    //Console.WriteLine(readText);
                }
                catch (Exception)
                {

                }
            }
        }

        /// <summary>
        /// sPath = Path of Directory
        /// sType = Type of Files example: "*.log"
        /// </summary>
        /// <param name="sPath"></param>
        /// <param name="sType"></param>
        public static void DeleteLogFilesOlderThan(string sPath, string sType, int iDays)
        {
            try
            {
                var files = new DirectoryInfo(sPath).GetFiles(sType);
                foreach (var file in files.Where(file => DateTime.UtcNow - file.CreationTimeUtc > TimeSpan.FromDays(iDays)))
                {
                    file.Delete();
                }
            }
            catch(Exception exp)
            {
                ms.cEvent.WriteInfo("Path for Logfiles does not exist => " + sPath);
                ms.cDebugLog.Log("Error in cDebugLog: DeleteLogFilesOlderThan " + exp.Message + " StackTrace: " + exp.StackTrace, true);
            }
        }

        public static void DeleteOldFiles(string path, uint maximumAgeInDays, params string[] filesToExclude)
        {

            DateTime minimumDate = DateTime.Now.AddDays(-maximumAgeInDays);
            foreach (var path1 in Directory.EnumerateFiles(path))
            {
                if (IsExcluded(path1, filesToExclude))
                    continue;

                DeleteFileIfOlderThan(path1, minimumDate);
            }
        }

        private const int RetriesOnError = 3;
        private const int DelayOnRetry = 1000;

        private static bool IsExcluded(string item, string[] exclusions)
        {
            foreach (string exclusion in exclusions)
            {
                if (item.Equals(exclusion, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool DeleteFileIfOlderThan(string path, DateTime date)
        {
            for (int i = 0; i < RetriesOnError; ++i)
            {
                try
                {
                    FileInfo file = new FileInfo(path);
                    if (file.CreationTime < date)
                        file.Delete();

                    return true;
                }
                catch (IOException)
                {
                    System.Threading.Thread.Sleep(DelayOnRetry);
                }
                catch (UnauthorizedAccessException)
                {
                    System.Threading.Thread.Sleep(DelayOnRetry);
                }
            }

            return false;
        }
    }
}

