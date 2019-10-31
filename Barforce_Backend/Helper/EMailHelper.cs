using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Model.Configuration;
using Microsoft.Extensions.Options;

namespace Barforce_Backend.Helper
{
    public class EMailHelper : IEmailHelper
    {
        private readonly EMailOptions _eMailOptions;

        public EMailHelper(IOptions<EMailOptions> eMailOptions)
        {
            _eMailOptions = eMailOptions.Value;
        }

        public async Task SendVerifyMail(string receiverAddress, Guid verifyGuid)
        {
            var fromAddress = new MailAddress(_eMailOptions.Address);
            var toAddress = new MailAddress(receiverAddress);

            var smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_eMailOptions.Address, _eMailOptions.Password)
            };

            using var msg = new MailMessage(fromAddress, toAddress)
            {
                Subject = "Anmeldung bei barfoce",
                Body = ""
            };

            await smtpClient.SendMailAsync(msg);
        }
    }
}