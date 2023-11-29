using TurboTicketsMVC.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;


namespace TurboTicketsMVC.Services
{
    //IEmailSender is by microsoft
    public class EmailService : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        //The following line gets the email settings that we set in secrets json and initialised in 
        //Program.cs in the custom configure "EmailSettings" class. 
        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {

                //We can use classes directly if they're static. we don;t need to instantiate Environment here.
                //The following line uses emailsettings locally and env vars when deployed.
                var emailAddress = _emailSettings.EmailAddress ?? Environment.GetEnvironmentVariable("EmailAddress");
                var emailPassword = _emailSettings.EmailPassword ?? Environment.GetEnvironmentVariable("EmailPassword");
                var emailHost = _emailSettings.EmailHost ?? Environment.GetEnvironmentVariable("EmailHost");
                var emailPort = _emailSettings.EmailPort != 0 ? _emailSettings.EmailPort : int.Parse(Environment.GetEnvironmentVariable("EmailPort")!);

                MimeMessage newEmail = new MimeMessage();
                newEmail.Sender = MailboxAddress.Parse(emailAddress);
                foreach (string address in email.Split(";"))
                {
                    newEmail.To.Add(MailboxAddress.Parse(address));
                    newEmail.Subject = subject;

                    BodyBuilder emailBody = new BodyBuilder();
                    emailBody.HtmlBody = htmlMessage;
                    newEmail.Body = emailBody.ToMessageBody();

                    using SmtpClient smtpClient = new SmtpClient();

                    try
                    {
                        await smtpClient.ConnectAsync(emailHost, emailPort, SecureSocketOptions.StartTls);
                        await smtpClient.AuthenticateAsync(emailAddress, emailPassword);
                        await smtpClient.SendAsync(newEmail);
                        await smtpClient.DisconnectAsync(true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("***************Error***************");
                        Console.WriteLine($"Failure sending email with google. Error: {ex.Message}");
                        Console.WriteLine("***************Error***************");
                        throw;
                    }

                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
