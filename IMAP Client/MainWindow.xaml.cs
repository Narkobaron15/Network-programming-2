using MailKit.Net.Imap;
using MailKit.Security;
using System;
using System.Net;
using System.Windows;

namespace IMAP_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private string Host;
        //private int Port;
        //private NetworkCredential Credentials;
        private ImapClient Client;

        public MainWindow(string Host, int Port, NetworkCredential Credentials, SecureSocketOptions SocketOptions)
        {
            //this.Host = Host;
            //this.Port = Port;
            //this.Credentials = Credentials;

            Client = new();
            Client.Connect(Host, Port, SocketOptions);

            Client.Authenticate(Credentials);

            InitialMailLoad();
        }

        private void InitialMailLoad()
        {
            // 
        }
    }
}
