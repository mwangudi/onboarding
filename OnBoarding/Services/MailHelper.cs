using System;
using System.Net.Mail;

namespace OnBoarding.Services
{

    public class MailHelper
    {
        //Sender Email Address
        public static string EmailFrom = OnBoarding.Properties.Settings.Default.EmailFrom;
            // System.Configuration.ConfigurationManager.AppSettings["EmailFrom"];//"eMTkenya@stanbic.com";

        public static bool SendMailMessage(string from, string to, string subject, string body)
        {
            try
            {
                // Instantiate a new instance of MailMessage
                MailMessage mMailMessage = new MailMessage();

                // Set the sender address of the mail message
                mMailMessage.From = new MailAddress(from);

                // Set the recepient address of the mail message
                mMailMessage.To.Add(new MailAddress(to));

                //// Check if the bcc value is null or an empty string
                //if ((bcc != null) && (bcc != string.Empty))
                //{
                //    // Set the Bcc address of the mail message
                //    mMailMessage.Bcc.Add(new MailAddress(bcc));
                //}
                //// Check if the cc value is null or an empty value
                //if ((cc != null) && (cc != string.Empty))
                //{
                //    // Set the CC address of the mail message
                //    mMailMessage.CC.Add(new MailAddress(cc));
                //}

                // Set the subject of the mail message
                mMailMessage.Subject = subject;
                // Set the body of the mail message
                mMailMessage.Body = body;

                // Set the format of the mail message body as HTML
                mMailMessage.IsBodyHtml = true;

                // Set the priority of the mail message to normal
                mMailMessage.Priority = MailPriority.Normal;

                // Instantiate a new instance of SmtpClient
                SmtpClient mSmtpClient = new SmtpClient();
                // Send the mail message
                mSmtpClient.Send(mMailMessage);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}