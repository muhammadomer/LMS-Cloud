using LogApp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Database.DAL
{
    public class SettingDAL
    {
        SinglePoint_CloudEntities singlePoint_CloudEntities = new SinglePoint_CloudEntities();
        public Settings GetSettings()
        {
            try
            {
                return singlePoint_CloudEntities.Settings.SingleOrDefault();
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return null;
            }
        }
        public bool SaveSettings(Settings settings)
        {
            try
            {
                Settings settings1 = singlePoint_CloudEntities.Settings.SingleOrDefault();
                settings1.EnableSSL = settings.EnableSSL;
                settings1.SMTPHost = settings.SMTPHost;
                settings1.SMTPPassword = settings.SMTPPassword;
                settings1.SMTPPort = settings.SMTPPort;
                settings1.SMTPUserName = settings.SMTPUserName;
                singlePoint_CloudEntities.Settings.AddOrUpdate(settings1);
                singlePoint_CloudEntities.SaveChanges();
                ActivityLogDAL.LogActivity("Save SMTP settings");
                return true;
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return false;
            }
        }
        public  string TestSMTPSettings(string toEmail, Settings smtpSettings)
        {

            try
            {
                string EmailTextChange = "Single Point";
                if (Convert.ToInt32(ConfigurationManager.AppSettings["OtherApplication"]) == 1)
                {
                    EmailTextChange = "Elephant-TMS";
                }

                MailMessage mail = new MailMessage();
                mail.To.Add(toEmail);
                mail.From = new MailAddress(smtpSettings.SMTPUserName);
                mail.Subject = EmailTextChange + " Cloud - SMTP Test Message";

                string Body = "This is an email message sent by "+ EmailTextChange + " Cloud application while testing the SMTP settings.";
                mail.Body = Body;

                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = smtpSettings.SMTPHost;
                smtp.Port = smtpSettings.SMTPPort;
                smtp.Credentials = new NetworkCredential(smtpSettings.SMTPUserName, smtpSettings.SMTPPassword);
                smtp.EnableSsl = true;
                smtp.Send(mail);
                ActivityLogDAL.LogActivity("Test Email");
                return "Sent email successfuly";
            }
            catch (Exception ex)
            {
                //if (ex.InnerException != null)
                //{
                //    return ex.InnerException.Message;
                //}
                //else
                //{
               

                return ex.Message;

               // }

            }
        }
        public bool EnableDisableTwoFA(bool state)
        {
            try
            {
                Settings setting = singlePoint_CloudEntities.Settings.FirstOrDefault();
                setting.TwoFAEnable = state;
                singlePoint_CloudEntities.Settings.AddOrUpdate(setting);
                singlePoint_CloudEntities.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return false;
            }
        }
        public bool IsEnabledTwoFA()
        {
            try
            {
                return singlePoint_CloudEntities.Settings.Select(x => x.TwoFAEnable).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log4Net.WriteException(ex);
                return false;
            }
        }
    }
}
