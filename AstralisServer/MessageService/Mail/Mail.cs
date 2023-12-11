
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace MessageService.Mail
{
    public class Mail
    {
        private static Mail instance;
        private readonly ILog log = LogManager.GetLogger(typeof(UserManager));
        private readonly IConfiguration configuration;
        private const string MAIL_DISPLAY_NAME = "Astralis";
        private readonly string fromEmail;
        private readonly string smtpHost;
        private readonly string password;
        private readonly int smtpPort;
        private readonly SmtpClient smtpClient;
        private MailMessage mail;

        public static Mail Instance()
        {
            if(instance == null)
            {
                instance = new Mail();
            }
            return instance;
        }

        public Mail() 
        {
            configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("MailConfiguration.json")
            .Build();
            fromEmail = configuration["EmailSettings:Email"];
            smtpHost = configuration["EmailSettings:SmtpHost"];
            password = configuration["password"];
            smtpPort = configuration.GetSection("EmailSettings")["SmtpPort"] != null ? int.Parse(configuration.GetSection("EmailSettings")["SmtpPort"]) : 0;
            smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
            };
        }

        public string sendInvitationMail(string to, string gameId)
        {
            string message = "Message could not be sent...";
            string htmlFilePath = "Resources/astralis.html";
            string body;

            try
            {
                using (StreamReader reader = new StreamReader(htmlFilePath))
                {
                    body = reader.ReadToEnd();
                }

                body.Replace("[field id = \"gameCode\"]", gameId);

                mail.From = new MailAddress(fromEmail, MAIL_DISPLAY_NAME);
                mail.To.Add(to);

                mail.Subject = "You are invited, use this code: " + gameId + "!";
                mail.Body = body;
                mail.IsBodyHtml = true;

                smtpClient.Credentials = new NetworkCredential(fromEmail, Decrypt(password));

                smtpClient.Send(mail);
                message = "Mail was succesfully sent";
            }
            catch (SmtpException smtpException)
            {
                message += "SMTP error: " + smtpException.Message;
                log.Error(message);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                message += "Invalid operation: " + invalidOperationException.Message;
                log.Error(message);
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                log.Error("HTML file not found: " + fileNotFoundException.Message);
            }
            catch (IOException iOException)
            {
                log.Error("Error reading HTML file: " + iOException.Message);
            }

            return message;
        }

        private string Decrypt(string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
