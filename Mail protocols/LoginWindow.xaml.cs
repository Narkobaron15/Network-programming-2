using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Mail_protocols
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
            // try - catch

            SmtpClient smtpClient = new(HostTextBox.Text, int.Parse(PortTextBox.Text))
            {
                Credentials = new NetworkCredential(LoginTextBox.Text, PasswordTextBox.Text),
                EnableSsl = SSLCheckBox.IsEnabled,
            };

            MainWindow mainWindow = new(smtpClient, LoginTextBox.Text);
            mainWindow.Show();
            this.Close();
        }
    }
}
