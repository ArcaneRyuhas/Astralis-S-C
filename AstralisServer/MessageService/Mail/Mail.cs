
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
        private static Mail _instance;
        private readonly ILog _log = LogManager.GetLogger(typeof(UserManager));
        private readonly string _configPath = Path.Combine(Directory.GetCurrentDirectory(), "Mail", "Resources", "MailConfiguration.json");
        private const string MAIL_DISPLAY_NAME = "Astralis";
        private readonly string _fromEmail;
        private readonly string _password;
        private readonly SmtpClient smtpClient;

        public static Mail Instance()
        {
            if(_instance == null)
            {
                _instance = new Mail();
            }
            return _instance;
        }

        public Mail() 
        {
             IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile(_configPath)
            .Build();

            _fromEmail = configuration["EmailSettings:Email"];
            string smtpHost = configuration["EmailSettings:SmtpHost"];
            _password = configuration["EmailSettings:Password"];
            int _smtpPort = configuration.GetSection("EmailSettings")["SmtpPort"] != null ? int.Parse(configuration.GetSection("EmailSettings")["SmtpPort"]) : 0;
            smtpClient = new SmtpClient(smtpHost, _smtpPort)
            {
                EnableSsl = true,
            };
        }

        public string SendInvitationMail(string to, string gameId)
        {
            MailMessage mail = new MailMessage();
            string message = "Message could not be sent...";
            string htmlFilePath = "Mail/Resources/astralis.html";
            string body;

            try
            {
                using (StreamReader reader = new StreamReader(htmlFilePath))
                {
                    body = reader.ReadToEnd();
                }

                body = body.Replace("[field id = \"gameCode\"]", gameId);
                mail.From = new MailAddress(_fromEmail, MAIL_DISPLAY_NAME);
                mail.To.Add(to);

                mail.Subject = "You are invited, use this code: " + gameId + "!";
                mail.Body = body;
                mail.IsBodyHtml = true;

                smtpClient.Credentials = new NetworkCredential(_fromEmail, Decrypt(_password));

                smtpClient.Send(mail);
                message = "Mail was succesfully sent";
            }
            catch (SmtpException smtpException)
            {
                message += "SMTP error: " + smtpException.Message;
                _log.Error(message);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                message += "Invalid operation: " + invalidOperationException.Message;
                _log.Error(message);
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                _log.Error("HTML file not found: " + fileNotFoundException.Message);
            }
            catch (IOException iOException)
            {
                _log.Error("Error reading HTML file: " + iOException.Message);
            }

            return message;
        }

        private static string Decrypt(string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

            byte[] decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
