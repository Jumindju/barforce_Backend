using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Barforce_Backend.Interface.Helper;
using Barforce_Backend.Model.Configuration;
using Barforce_Backend.Model.Helper.Middleware;
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

        public async Task SendVerifyMail(string receiverAddress, int verifyNum)
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

            try
            {
                using var msg = new MailMessage(fromAddress, toAddress)
                {
                    Subject = "Anmeldung bei barfoce",
                    IsBodyHtml = true,
                    // ReSharper disable once StringLiteralTypo
                    Body = $@"
<html lang=""de"">
    <head>
        <title>Barforce Anmeldung</title>
    </head>
    <body>
        <h3>
            Herzlichen Glückwunsch zur Anmeldung bei barforce
        </h3>
        <p>
           Gib den folgenden Code einfach ein, nachdem du dich bei der App angemeldet hast.
        </p>
        <p style=""text-align:center;font-size:36px;font-color:#ff0000;"">
            <b>{verifyNum}</b>
        </p>
        <footer>
            Viel Spaß!
        </footer>
    </body>
</html>"
                };
                await smtpClient.SendMailAsync(msg);
            }
            catch (Exception e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.FailedDependency, "Error while sending mail", e);
            }
        }
    }
}