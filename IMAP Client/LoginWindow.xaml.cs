using Mail_protocols;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IMAP_Client
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private SecureSocketOptions SmtpSelectedOptions
        {
            get => (SecureSocketOptions)SmtpSecurityCombobox.SelectedItem;
            set => SmtpSecurityCombobox.SelectedItem = value;
        }
        private SecureSocketOptions ImapSelectedOptions
        {
            get => (SecureSocketOptions)ImapSecurityCombobox.SelectedItem;
            set => ImapSecurityCombobox.SelectedItem = value;
        }

        public LoginWindow()
        {
            InitializeComponent();

            SmtpSecurityCombobox.ItemsSource = Enum.GetValues<SecureSocketOptions>();
            SmtpSelectedOptions = SecureSocketOptions.Auto;
            ImapSecurityCombobox.ItemsSource = Enum.GetValues<SecureSocketOptions>();
            ImapSelectedOptions = SecureSocketOptions.Auto;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(ImapPortTextBox.Text, out int imapport)
                    || !int.TryParse(SmtpPortTextBox.Text, out int smtpport))
                    throw new FormatException("The port provided is not valid. (Should be a positive integer value");

                ConnectionCredentials credentials = new(
                    new NetworkCredential(LoginTextBox.Text, PasswordTextBox.Text),
                    SmtpHostTextBox.Text, smtpport, SmtpSelectedOptions,
                    ImapHostTextBox.Text, imapport, ImapSelectedOptions
                    );

                Task.Run(() => TryConnect(credentials));
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void TryConnect(ConnectionCredentials Credentials)
        {
            try
            {
                ImapClient imapClient = new();
                SmtpClient smtpClient = new();

                Task ImapConnectTask = imapClient.ConnectAsync(Credentials.ImapHost, Credentials.ImapPort ?? 0, Credentials.ImapSecurityOptions ?? SecureSocketOptions.Auto);
                Task SmtpConnectTask = smtpClient.ConnectAsync(Credentials.SmtpHost, Credentials.SmtpPort ?? 0, Credentials.SmtpSecurityOptions ?? SecureSocketOptions.Auto);

                Task.WaitAll(SmtpConnectTask, ImapConnectTask);

                smtpClient.Dispose();
                imapClient.Dispose();

                this.Dispatcher.Invoke(() => 
                {
                    MainWindow mainWindow = new(Credentials);
                    mainWindow.Show();
                    this.Close();
                });
            }
            catch (Exception ex)
            {
                string msg = new StringBuilder()
                                 .AppendLine("Error! Connection couldn't be established.")
                                 .AppendLine("Maybe you have put wrong credentials or you have no internet access.\n")
                                 .Append("Details: ")
                                 .AppendLine(ex.Message)
                                 .ToString();
                MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
