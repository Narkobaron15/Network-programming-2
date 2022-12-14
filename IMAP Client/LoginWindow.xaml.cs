using MailKit.Net.Imap;
using MailKit.Security;
using System;
using System.Net;
using System.Windows;

namespace IMAP_Client
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // REMINDER: REWORK SECURITY OPTIONS CHANGER
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(PortTextBox.Text, out int port))
                    throw new FormatException("The port provided is not valid. (Should be a positive integer value");

                ImapClient client = new();
                client.Connect(HostTextBox.Text, port, SecureSocketOptions.SslOnConnect);

                MainWindow mainWindow = new(HostTextBox.Text, port,
                    new NetworkCredential(LoginTextBox.Text, PasswordTextBox.Text), SecureSocketOptions.SslOnConnect);
                mainWindow.Show();
                this.Close();
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error! Connection couldn't be established.\n\nDetails: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
