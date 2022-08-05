using Mailjet.Client;
using Mailjet.Client.Resources;
using Mailjet.Client.TransactionalEmails;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ASP.NET_Core_MVC_Playground.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }

    public class AuthMessageSenderOptionsMailjet
    {
        public string? ApiKey { get; set; }
        public string? SecretKey { get; set; }
    }

    public class AuthMessageSenderOptionsSendgrid
    {
        public string? ApiKey { get; set; }
    }

    public class AuthMessageSenderOptionsTwilio
    {
        public string? AccountSID { get; set; }
        public string? AuthToken { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        private readonly ILogger _logger;
        public AuthMessageSenderOptionsMailjet OptionsMailjet { get; }
        public AuthMessageSenderOptionsSendgrid OptionsSendgrid { get; }
        public AuthMessageSenderOptionsTwilio OptionsTwilio { get; }

        // Currently using Sendgrid
        // Can be changed so Mailjet
        public AuthMessageSender(IOptions<AuthMessageSenderOptionsSendgrid> optionsAccessorSendgrid,
                                 IOptions<AuthMessageSenderOptionsTwilio> optionsAccessorTwilio,
                                 ILogger<AuthMessageSender> logger)
        {
            OptionsSendgrid = optionsAccessorSendgrid.Value;
            OptionsTwilio = optionsAccessorTwilio.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            if (string.IsNullOrEmpty(OptionsSendgrid.ApiKey))
            {
                throw new Exception("Null or Invalid API Keys");
            }
            await ExecuteEmail(OptionsSendgrid.ApiKey, email, subject, message);
        }

        public async Task ExecuteEmail(string apiKey, string email, string subject, string message)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("jogeo98@hotmail.com"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            // Disable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(false, false);
            var response = await client.SendEmailAsync(msg);

            _logger.LogInformation(response.IsSuccessStatusCode
                ? $"Email to {email} queued successfully!"
                : $"Failure Email to {email}");

            //MailjetClient client = new MailjetClient(apiKey, secretKey);

            //MailjetRequest request = new MailjetRequest
            //{
            //    Resource = Send.Resource,
            //};

            //var emailObject = new TransactionalEmailBuilder()
            //    .WithFrom(new SendContact("george@summercliff.com"))
            //    .WithSubject($"{subject}")
            //    .WithHtmlPart($"{htmlMessage}")
            //    .WithTo(new SendContact($"{email}"))
            //    .Build();

            //var response = await client.SendTransactionalEmailAsync(emailObject);
        }

        public async Task SendSmsAsync(string number, string message)
        {
            if ((string.IsNullOrEmpty(OptionsTwilio.AccountSID)) || (string.IsNullOrEmpty(OptionsTwilio.AuthToken)) )
            {
                throw new Exception("Null or Invalid API Keys");
            }
            await ExecuteSms(OptionsTwilio.AccountSID, OptionsTwilio.AuthToken, number, message);
        }

        public Task ExecuteSms(string AccountSID, string AuthToken, string number, string message)
        {
            TwilioClient.Init(AccountSID, AuthToken);

            return MessageResource.CreateAsync(
              to: new PhoneNumber(number),
              from: new PhoneNumber(OptionsTwilio.PhoneNumber),
              body: message);
        }
    }
}
