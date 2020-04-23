using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using Trinbago_MVC5.Areas.ClassifiedAd.Models;
using Trinbago_MVC5.Controllers;
using Trinbago_MVC5.Models;

namespace Trinbago_MVC5.Extensions
{
    public static class EmailMessenger
    {
        public static bool SendMessage(ClassifiedAdQ msg)
        {
            var message = new MailMessage();
            try
            {
                message.To.Add(new MailAddress(msg.To));
                message.From = new MailAddress("DoNotReply@trinbagohotspot.com", "TrinbagoHotspot.com");
                message.ReplyToList.Add(new MailAddress(msg.From));
                message.Subject = "Message from " + msg.Name + " via 'TrinbagoHotSpot.com'";
                message.Body = MessageConfig.ReplyToAdMessage(msg);
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient(ConfigurationManager.AppSettings["defaultSMTP"], 587))
                {
                    var credentials = new NetworkCredential(ConfigurationManager.AppSettings["mailAccount"], ConfigurationManager.AppSettings["mailPassword"]);
                    smtp.Credentials = credentials;
                    smtp.EnableSsl = false;
                    smtp.Timeout = 5000;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool SendMessage(ClassifiedAdEmailUserPost msg)
        {
            var message = new MailMessage();
            try
            {
                message.To.Add(new MailAddress(msg.To));
                message.From = new MailAddress("DoNotReply@trinbagohotspot.com", "TrinbagoHotspot.com");
                message.ReplyToList.Add(new MailAddress(msg.From));
                message.Subject = "Message from " + msg.Name + " via 'TrinbagoHotSpot.com'";
                message.Body = MessageConfig.ReplyToAdMessage(msg);
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient(ConfigurationManager.AppSettings["defaultSMTP"], 587))
                {
                    var credentials = new NetworkCredential(ConfigurationManager.AppSettings["mailAccount"], ConfigurationManager.AppSettings["mailPassword"]);
                    smtp.Credentials = credentials;
                    smtp.EnableSsl = false;
                    smtp.Timeout = 5000;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        // Sends a contact message (msg) to the recipient (sendTo)
        public static bool SendContactUsMessage(ContactUs msg, string sendTo)
        {
            var message = new MailMessage();
            try
            {
                message.To.Add(new MailAddress(sendTo));
                message.From = new MailAddress("DoNotReply@trinbagohotspot.com", "TrinbagoHotspot.com");
                message.ReplyToList.Add(new MailAddress(msg.ReturnTo));
                message.Subject = "Contact/Us message from " + msg.ReturnTo + " via 'TrinbagoHotSpot.com'";
                message.Body = MessageConfig.ContactUsMessage(msg.Description);
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient(ConfigurationManager.AppSettings["defaultSMTP"], 587))
                {
                    var credentials = new NetworkCredential(ConfigurationManager.AppSettings["mailAccount"], ConfigurationManager.AppSettings["mailPassword"]);
                    smtp.Credentials = credentials;
                    smtp.EnableSsl = false;
                    smtp.Timeout = 5000;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool SendMessage(HangfireMessage msg)
        {
            var message = new MailMessage();
            try
            {
                message.To.Add(new MailAddress(msg.To));
                message.From = new MailAddress("DoNotReply@trinbagohotspot.com", "TrinbagoHotspot.com");
                message.ReplyToList.Add(new MailAddress("DoNotReply@trinbagohotspot.com"));
                message.Subject = "Message from TrinbagoHotspot Admin via 'TrinbagoHotSpot.com'";
                message.Body = MessageConfig.ClassifiedAdExpireMessage(msg);
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient(ConfigurationManager.AppSettings["defaultSMTP"], 587))
                {
                    var credentials = new NetworkCredential(ConfigurationManager.AppSettings["mailAccount"], ConfigurationManager.AppSettings["mailPassword"]);
                    smtp.Credentials = credentials;
                    smtp.EnableSsl = false;
                    smtp.Timeout = 5000;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}