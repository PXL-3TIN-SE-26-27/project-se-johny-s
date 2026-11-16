using System.Net.Mail;

namespace CampusBook.Api.Infrastructure;

public class SmtpMailer
{
    private readonly string _host;

    public SmtpMailer(string host)
    {
        _host = host;
    }

    public virtual void Send(string to, string subject, string body)
    {
        try
        {
            var dir = AppConfig.Instance.MailPickupDirectory;
            using var client = new SmtpClient(_host);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
                client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                client.PickupDirectoryLocation = Path.GetFullPath(dir);
            }
            using var msg = new MailMessage(AppConfig.Instance.SmtpFrom, to, subject, body);
            client.Send(msg);
        }
        catch (Exception)
        {
            // mail is not important
        }
    }
}
