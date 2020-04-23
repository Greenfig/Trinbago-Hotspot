using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Net.Mail;
using System.Net;
using System.Configuration;
using Trinbago_MVC5.Controllers;

namespace Trinbago_MVC5
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            var mail = new MailMessage();
            try
            {
                mail.To.Add(new MailAddress(message.Destination));
                mail.From = new MailAddress("DoNotReply@trinbagohotspot.com", "TrinbagoHotspot.com");
                mail.Subject = message.Subject;
                mail.Body = MessageConfig.VerifyEmailMessage(message.Body);
                mail.IsBodyHtml = true;

                var smtp = new SmtpClient(ConfigurationManager.AppSettings["defaultSMTP"], 587);
                var credentials = new NetworkCredential(ConfigurationManager.AppSettings["mailAccount"], ConfigurationManager.AppSettings["mailPassword"]);
                smtp.Credentials = credentials;
                smtp.EnableSsl = false;
                smtp.Timeout = 5000;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                var t = Task.Run(() => smtp.SendAsync(mail, null));
                return t;                
            }
            catch (Exception)
            {
                return Task.FromResult(0);
            }
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }    
}
