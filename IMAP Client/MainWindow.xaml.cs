using Mail_protocols;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace IMAP_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ImapClient Client { get; }
        private ConnectionCredentials ConnectionCredentials { get; }
        private ItemCollection Messages => EmailBox.Items;
        private MimeMessage SelectedMessage => (MimeMessage)EmailBox.SelectedItem;

        private IList<IMailFolder> Folders { get; }
        private IMailFolder? _selectedfolder;
        private IMailFolder SelectedFolder
        {
            get => _selectedfolder!;
            set
            {
                this.Dispatcher.Invoke(() => Messages.Clear());
                _selectedfolder?.CloseAsync(true);
                CancelLoadingSource?.Cancel();

                _selectedfolder = value;

                Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    LoadEmails();
                });
            }
        }

        private CancellationTokenSource? _cancelloadingsource;
        private CancellationTokenSource? CancelLoadingSource
        {
            get => _cancelloadingsource;
            set
            {
                _cancelloadingsource?.Dispose();
                _cancelloadingsource = value;
            }
        }
        private CancellationToken CancelLoadingToken => (CancelLoadingSource?.Token)!.Value;

        public MainWindow(ConnectionCredentials ConnectionCredentials)
        {
            InitializeComponent();

            this.ConnectionCredentials = ConnectionCredentials;

            Client = new();
            Client.Connect(ConnectionCredentials.ImapHost, ConnectionCredentials.ImapPort!.Value, ConnectionCredentials.ImapSecurityOptions!.Value);
            Client.Authenticate(ConnectionCredentials.LoginCredentials);

            Folders = Client.GetFolders(Client.PersonalNamespaces[0]);
            FoldersBox.ItemsSource = Folders
                                     .Where(x => (x.Attributes & FolderAttributes.NonExistent) != FolderAttributes.NonExistent)
                                     .Select(x => x.Name);
            FoldersBox.SelectedItem = Client.Inbox.Name;
        }

        private void LoadEmails()
        {
            lock (Client)
            {
                CancelLoadingSource = new();

                SelectedFolder.Open(FolderAccess.ReadWrite);
                var emails = SelectedFolder.Search(SearchQuery.All);

                if (emails.Count == 0)
                {
                    MessageBox.Show("No emails in this folder >.<", "Information", MessageBoxButton.OK, MessageBoxImage.Information); 
                    return;
                }

                for (int i = 1; i <= Math.Min(emails.Count, 50); i++)
                {
                    if (CancelLoadingToken.IsCancellationRequested)
                    {
                        this.Dispatcher.InvokeAsync(() => Messages.Clear());
                        return;
                    }

                    var email = SelectedFolder.GetMessage(emails[^i]);
                    this.Dispatcher.Invoke(() => Messages.Add(email));
                }
            }
        }

        private void FoldersBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string? selectedname = FoldersBox.SelectedItem.ToString();
            SelectedFolder = Folders.Where(x => x.Name == selectedname).First();
        }

        private void EmailBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                EmailViewWindow emailView = new(ConnectionCredentials, SelectedMessage);
                emailView.ShowDialog();
            }
        }

        private void FoldersBox_Drop(object sender, DragEventArgs e)
        {
            // move emails between folders
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => Directory.Delete("Temp", true);

        private void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            System.Net.Mail.SmtpClient client = new(ConnectionCredentials.SmtpHost, ConnectionCredentials.SmtpPort ?? 0)
            {
                EnableSsl = true,
                Credentials = ConnectionCredentials.LoginCredentials
            };
            SMTP_Client.MainWindow wnd = new(client, ConnectionCredentials.LoginCredentials.UserName);
            wnd.ShowDialog();
        }
    }
}
