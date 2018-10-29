using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Hosting;

namespace ProAlfaTruckMan
{
    public interface IEmailConfiguration
    {
        string SmtpServer { get; }
        int SmtpPort { get; }
        string SmtpUsername { get; set; }
        string SmtpPassword { get; set; }

        string PopServer { get; }
        int PopPort { get; }
        string PopUsername { get; }
        string PopPassword { get; }
    }

    public class EmailConfiguration : IEmailConfiguration
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }

        public string PopServer { get; set; }
        public int PopPort { get; set; }
        public string PopUsername { get; set; }
        public string PopPassword { get; set; }
    }

    public interface IEmailService
    {
        void Send(MailMessage EmailMessage, IHostingEnvironment env);
        List<MailMessage> ReceiveEmail(int maxCount = 10);
    }

    public class EmailService : IEmailService
    {
        private readonly IEmailConfiguration _emailConfiguration;

        public EmailService(IEmailConfiguration emailConfiguration)
        {
            _emailConfiguration = emailConfiguration;
        }

        public List<MailMessage> ReceiveEmail(int maxCount = 10)
        {
            throw new NotImplementedException();
        }

        public void Send(MailMessage emailMessage, IHostingEnvironment env)
        {
            SmtpClient client = new SmtpClient(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);
            if (env.IsProduction())
            {
                client.EnableSsl = false;   // musí byť false, lebo Aspify nepodporuje SSL pripojenie k emailovému serveru
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
            }
            else
            {
                client.EnableSsl = false;   // musí byť false, lebo pri vývoji sa používa self-signed SSL certifikát, ktorý DXC emailový server vyhodnotí ako nedôveryhodný.
            }

            client.Send(emailMessage);
        }
    }
}
