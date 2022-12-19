using MailKit.Security;
using System.Net;

namespace Mail_protocols
{
    public class ConnectionCredentials
    {
        public string? SmtpHost { get; init; }
        public int? SmtpPort { get; init; }
        public SecureSocketOptions? SmtpSecurityOptions { get; init; }
        public string? ImapHost { get; init; }
        public int? ImapPort { get; init; }
        public SecureSocketOptions? ImapSecurityOptions { get; init; }
        public NetworkCredential LoginCredentials { get; init; }

        public ConnectionCredentials(NetworkCredential LoginCredentials, 
            string? SmtpHost = null, int? SmtpPort = null, SecureSocketOptions? SmtpSecurityOptions = null,
            string? ImapHost = null, int? ImapPort = null, SecureSocketOptions? ImapSecurityOptions = null)
        {
            this.SmtpHost = SmtpHost;
            this.SmtpPort = SmtpPort;
            this.SmtpSecurityOptions = SmtpSecurityOptions;

            this.ImapHost = ImapHost;
            this.ImapPort = ImapPort;
            this.ImapSecurityOptions = ImapSecurityOptions;

            this.LoginCredentials = LoginCredentials;
        }
        public ConnectionCredentials()
            : this(new()) { }
    }
}
