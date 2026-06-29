using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
namespace BLL.Managers.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("منصة فايد - Fayed Platform", _configuration["EmailSettings:SenderEmail"]!));

            emailMessage.To.Add(new MailboxAddress("", toEmail));

            emailMessage.Subject = subject;
            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(
                    _configuration["EmailSettings:Server"]!,
                    int.Parse(_configuration["EmailSettings:Port"]!),
                    MailKit.Security.SecureSocketOptions.StartTls
                );

                await client.AuthenticateAsync(
                    _configuration["EmailSettings:Username"]!,
                    _configuration["EmailSettings:Password"]!
                );

                await client.SendAsync(emailMessage);
            }
            catch (Exception ex)
            {
                throw new Exception($"فشل إرسال بريد التأكيد عبر Mailtrap: {ex.Message}");
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}
