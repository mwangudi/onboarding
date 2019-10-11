using OnBoarding.Models;
using System;

namespace OnBoarding.Services
{
    public class LogNotification
    {
        //Add Email Sent Success Notification
        public static void AddSucsessNotification(string from, string message, string to, string _action)
        {
            try
            {
                using (DBModel db = new DBModel())
                {
                    var Notification = db.Notifications.Create();
                    Notification.From = from;
                    Notification.MessageBody = message;
                    Notification.To = to;
                    Notification.Sent = true;
                    Notification.Type = "Email";
                    Notification.DateCreated = DateTime.Now;
                    Notification.Action = _action;
                    db.Notifications.Add(Notification);
                    db.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //Add Email not sent (Failure) notification
        public static void AddFailureNotification(string from, string message, string to, string _action)
        {
            try
            {
                using (DBModel db = new DBModel())
                {
                    var Notification = db.Notifications.Create();
                    Notification.From = from;
                    Notification.MessageBody = message;
                    Notification.To = to;
                    Notification.Sent = false;
                    Notification.Type = "Email";
                    Notification.DateCreated = DateTime.Now;
                    Notification.Action = _action;
                    db.Notifications.Add(Notification);
                    db.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
    }
}