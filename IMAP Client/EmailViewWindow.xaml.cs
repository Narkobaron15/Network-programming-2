using Mail_protocols;
using MimeKit;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace IMAP_Client
{
    /// <summary>
    /// Interaction logic for EmailViewWindow.xaml
    /// </summary>
    public partial class EmailViewWindow : Window
    {
        public MimeMessage Message { get; }
        private ConnectionCredentials Credentials { get; }
        private static Regex EmailRegex => new(@"<.*>");

        public EmailViewWindow(ConnectionCredentials Credentials, MimeMessage Message)
        {
            InitializeComponent();
            Directory.CreateDirectory("Temp");

            this.Message = Message;
            this.Credentials = Credentials;

            HtmlBodyPresenter.NavigateToString(ChangeEncodingToUTF8(Message.HtmlBody));

            if (Message.Attachments.Any())
            {
                foreach (var attachment in Message.Attachments)
                {
                    string fileName = attachment.ContentType.Name;

                    using var stream = File.Create("Temp/" + fileName);
                    if (attachment is MessagePart rfc822)
                        rfc822.Message.WriteTo(stream);
                    else
                    {
                        MimePart part = (MimePart)attachment;
                        part.Content.DecodeTo(stream);
                    }

                    Label label = new() { Content = fileName, Tag = stream.Name };
                    AttachmentsListBox.Items.Add(label);
                }
            }
            else
            {
                AttachmentsTabItem.Content = new Label()
                {
                    Content = "No attachments",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
            }
        }

        private const string HTMLUTF8Encoding = "<meta charset=\"UTF-8\">";
        private static string ChangeEncodingToUTF8(string html)
        {
            string headtag = "<head>";
            int headpos = html.IndexOf(headtag);

            return (headpos == -1)
                ? HTMLUTF8Encoding + html
                : html.Insert(headpos + headtag.Length, HTMLUTF8Encoding);
        }

        private void AttachmentsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileInfo source = new(((Label)AttachmentsListBox.SelectedItem).Tag.ToString()!);
            try
            {
                Process.Start(new ProcessStartInfo(source.FullName) { UseShellExecute = true });
            }
            catch (Exception)
            {
                if (MessageBox.Show("Could not open selected file.\nShould it be copied to the download folder?", 
                    "Error!", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                    == MessageBoxResult.Yes)
                {
                    string dlfolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\",
                           dest = dlfolder + source.Name;

                    if (!File.Exists(dest))
                        File.Copy(source.FullName, dest);
                    else
                    {
                        string dest1;
                        for (int i = 0; ; i++)
                        {
                            dest1 = $"{dlfolder}{source.Name[..source.Name.LastIndexOf('.')]} - copy ({i}){source.Extension}";
                            if (!File.Exists(dest1)) break;
                        }
                        File.Copy(source.FullName, dest1);
                    }
                }
            }
        }

        private void RespondBtn_Click(object sender, RoutedEventArgs e)
        {
            string recipient = EmailRegex.Matches(Message.From.ToString() + Message.To.ToString()).First().Value[1..^1],
                   subject = Message.Subject.Trim().ToLower().StartsWith("re: ")
                             ? Message.Subject
                             : "Re: " + Message.Subject;

            System.Net.Mail.SmtpClient client = new(Credentials.SmtpHost, Credentials.SmtpPort ?? 0) 
            { 
                EnableSsl = true,
                Credentials = Credentials.LoginCredentials
            };
            SMTP_Client.MainWindow wnd = new(client, Credentials.LoginCredentials.UserName, recipient, subject);

            wnd.ShowDialog();
        }
    }
}
