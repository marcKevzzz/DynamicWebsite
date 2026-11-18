using System.Net;
using System.Net.Mail;

namespace Dcd.Web.Data;

internal sealed record RecoveryMail(Uri Origin, string Host, int Port, MailAddress From, NetworkCredential? Credentials)
{
    public static RecoveryMail? FromConfiguration(IConfiguration configuration)
    {
        if (!Uri.TryCreate(configuration["Recovery:PublicOrigin"], UriKind.Absolute, out var origin) ||
            origin.Scheme != Uri.UriSchemeHttps || origin.UserInfo.Length > 0 ||
            string.IsNullOrWhiteSpace(configuration["Smtp:Host"]) ||
            !MailAddress.TryCreate(configuration["Smtp:From"], out var from)) return null;
        var credentials = configuration["Smtp:User"] is { Length: > 0 } user
            ? new NetworkCredential(user, configuration["Smtp:Password"]) : null;
        return new(origin, configuration["Smtp:Host"]!, configuration.GetValue("Smtp:Port", 587), from, credentials);
    }

    public async Task SendAsync(string recipient, string path, string subject, string instruction, CancellationToken cancellationToken)
    {
        using var message = new MailMessage(From, new MailAddress(recipient))
        {
            Subject = subject,
            Body = $"{instruction}\n\n{new Uri(Origin, path)}\n\nIf you did not request this, you can ignore this email."
        };
        using var smtp = new SmtpClient(Host, Port) { EnableSsl = true, Credentials = Credentials };
        await smtp.SendMailAsync(message, cancellationToken);
    }
}
