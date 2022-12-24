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
        private ImapFolder? _selectedfolder;
        private ImapFolder SelectedFolder
        {
            get => _selectedfolder!;
            set
            {
                CancelLoadingSource?.Cancel();

                Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    _selectedfolder?.Close();
                    _selectedfolder = value;
                    _selectedfolder.Open(FolderAccess.ReadWrite);

                    await this.Dispatcher.InvokeAsync(Messages.Clear);
                    ExecuteSearch(SearchQuery.All);
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
        private CancellationToken CancelLoadingToken => CancelLoadingSource?.Token ?? default;

        private readonly SearchQuery[] Queries;

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


            Queries = new[] 
            { 
                SearchQuery.All, SearchQuery.Flagged, SearchQuery.NotFlagged, 
                SearchQuery.Deleted, SearchQuery.Answered,
                SearchQuery.Seen, SearchQuery.NotSeen, 
                SearchQuery.Old, SearchQuery.Recent
            };

            var filternames = new[] { "All", "Flagged", "Not flagged", "Deleted", "Answered", "Seen", "Not seen", "Old", "Recent" };
            QueriesComboBox.ItemsSource = filternames;
            QueriesComboBox.SelectedIndex = 0;
        }

        private void FoldersBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string? selectedname = FoldersBox.SelectedItem.ToString();
            SelectedFolder = (ImapFolder)Folders.Where(x => x.Name == selectedname).First();
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CancelLoadingSource?.Cancel();
            DirectoryInfo tmpdir = new("Temp");
            if (tmpdir.Exists) tmpdir.Delete(true);
        }

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

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string key = SearchTextBox.Text;

            if (string.IsNullOrWhiteSpace(key)) MessageBox.Show("No search key provided");
            else
            {
                Messages.Clear();
                Task.Run(() => ExecuteSearch(AggregateQueriesByTerm(key)));
            }
        }

        private void QueriesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchQuery query = Queries[QueriesComboBox.SelectedIndex];
            Messages.Clear();
            Task.Run(() => ExecuteSearch(query));
        }

        private static SearchQuery AggregateQueriesByTerm(string term)
        {
            SearchQuery[] queries = new[]
            {
                SearchQuery.BccContains(term),
                SearchQuery.CcContains(term),
                SearchQuery.ToContains(term),
                SearchQuery.FromContains(term),
                SearchQuery.BodyContains(term),
                SearchQuery.SubjectContains(term),
                SearchQuery.MessageContains(term),
            };

            SearchQuery result = queries[0];
            foreach (SearchQuery query in queries)
                result = result.Or(query);

            return result;
        }

        private void ExecuteSearch(SearchQuery query)
        {
            CancelLoadingSource?.Cancel();

            lock (Client.SyncRoot)
            {
                if (SelectedFolder is null) return;

                CancelLoadingSource = new();

                var collection = SelectedFolder.Search(query).Reverse().ToList();

                if (collection.Count > 0)
                {
                    MimeMessage msg;
                    foreach (var id in collection)
                    {
                        if (CancelLoadingToken.IsCancellationRequested)
                            return;

                        msg = SelectedFolder.GetMessage(id);
                        this.Dispatcher.InvokeAsync(() => Messages.Add(msg));
                    }
                }
                else MessageBox.Show("No items found >.<", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            CancelLoadingSource = new();
        }
    }
}