using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.IO;
using System.Windows.Forms;
using System.Configuration;

namespace ms
{
    public static class cSendEmail
    {

        public static string AppDataPath()
        {
            string appPath = Path.GetDirectoryName(Application.ExecutablePath);
            return appPath;
        }
        /// <summary>
        /// Send Email with the following parameters
        /// </summary>
        /// <param name="fromEmail"></param>
        /// <param name="toEmail"></param>
        /// <param name="Subject"></param>
        /// <param name="Message"></param>
        /// <param name="accountUsername"></param>
        /// <param name="accountPassword"></param>
        /// <param name="smtp"></param>
        /// <param name="port"></param>
        /// <param name="ssl"></param>
        public static void SendMail(string fromEmail, string toEmail, string Subject, string Message, string accountUsername, string accountPassword, string smtp, string port, Boolean ssl)
        {
            cProtection protection = new cProtection();
            try
            {
                MailMessage mail = new MailMessage();
                //SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                string sSmtp = smtp;
                SmtpClient SmtpServer = new SmtpClient(sSmtp);

                mail.From = new MailAddress(fromEmail);
                string[] toSendTo = toEmail.Split(';');
                foreach (var item in toSendTo)
                {
                    mail.To.Add(item);
                }
               // mail.To.Add(toEmail);
                mail.Subject = Subject;
                mail.Body = Message;
                SmtpServer.Host = smtp;
                SmtpServer.Port = Convert.ToInt32(port);
                // SmtpServer.Port = 587; //Google
                //SmtpServer.Port = 25;
                SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                SmtpServer.Credentials = new System.Net.NetworkCredential(accountUsername, protection.Decrypt(accountPassword));
                SmtpServer.EnableSsl = ssl;
                SmtpServer.Timeout = 20000;
                SmtpServer.Send(mail);
                cDebugLog.Log( Application.ProductName + " sucsessfully send", true);
                
            }
            catch (Exception exp)
            {
                cDebugLog.Log(Application.ProductName + "Mail failed to send\n" + exp.Message,true);
                cEvent.WriteError("See logfile for more details");
            }
        }
    }
}
