using log4net;
using MobiObmen.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MobiObmen.Services
{
    public class MailSender
    {
        private static readonly string email = ConfigurationManager.AppSettings["FromEmail"];
        private static readonly string pass = ConfigurationManager.AppSettings["FromEmailPassword"];
        //private static readonly string[] toEmail = { ConfigurationManager.AppSettings["SJumaev"], ConfigurationManager.AppSettings["AllBilling"] };
        private static readonly string[] toEmail = { ConfigurationManager.AppSettings["SJumaev"], ConfigurationManager.AppSettings["WhomSend"] };
        private static readonly string mailServer = ConfigurationManager.AppSettings["MailServer"];
        private static readonly string mailPort = ConfigurationManager.AppSettings["MailPort"];
        private static readonly ILog log = LogManager.GetLogger(typeof(MailSender));
        private static readonly Dictionary<string, string> resourcesAnalog = new Dictionary<string, string>
        {
            {"5050", "SMS"}, //SMS
            {"4500", "MB"}, //MB
            {"5001", "Min"}  //Min
        };
        public static void SendEmail(ResourceExchangeRequest request, string quantityOfExchangeResource)
        {
            try
            {
                string message = $"С абонента {request.MSISDN} c ресурсов тарифного плана снялось {request.QuantityResource} {resourcesAnalog[request.Resource]} и не перечислелось абоненту {quantityOfExchangeResource} {resourcesAnalog[request.ToResource]}. <br>ВРЕМЯ: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(email);
                foreach (string address in toEmail)
                {
                    mailMessage.To.Add(address);
                }
                mailMessage.IsBodyHtml = true;
                mailMessage.Subject = "Ошибка MobiObmen";
                mailMessage.Body = message;
                SmtpClient smtpServer = new SmtpClient(mailServer);
                smtpServer.Port = Int32.Parse(mailPort);
                smtpServer.Credentials = new NetworkCredential(email, pass);
                smtpServer.EnableSsl = true;
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                smtpServer.Send(mailMessage);
            }
            catch (Exception ex)
            {
                log.Error($"Send Email: {ex}");
            }

        }
    }
}
