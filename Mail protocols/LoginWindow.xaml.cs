using System;
using System.Net.Mail;
using System.Net;
using System.Windows;

namespace SMTP_Client
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SmtpClient smtpClient = new(HostTextBox.Text, int.Parse(PortTextBox.Text))
                {
                    Credentials = new NetworkCredential(LoginTextBox.Text, PasswordTextBox.Text),
                    EnableSsl = SSLCheckBox.IsEnabled,
                };

                MainWindow mainWindow = new(smtpClient, LoginTextBox.Text);
                mainWindow.Show();
                this.Close();
            }
            catch (FormatException)
            {
                MessageBox.Show("The port provided is not-a-number value. (Should be an integer value",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (OverflowException)
            {
                MessageBox.Show("The port provided should be a positive integer value",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
