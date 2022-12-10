using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows;

namespace Mail_protocols
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string MailAddress = "oleksii120@gmail.com",
                             pwd = "jylrwfhbqdcqewfi";

        private readonly List<Attachment> AttachmentsList;
        private OpenFileDialog OpenFileDialog;

        public MainWindow()
        {
            InitializeComponent();
            AttachmentsList = new();
            OpenFileDialog = new()
            {
                Filter = "All files (*.*)|*.*",
                FilterIndex = 0,
                Multiselect = true,
            };
        }

        private void AddAttachmentBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog.FileName = string.Empty;

            if (OpenFileDialog.ShowDialog() == true)
            {
                foreach (var path in OpenFileDialog.FileNames)
                    AttachmentsList.Add(new Attachment(path));
            }
        }

        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ToTextBox.Text)) return;

            MailMessage msg = new(MailAddress, ToTextBox.Text)
            {
                Priority = MailPriority.Normal,
                Subject = SubjectTextBox.Text,
                Body = $"<h1>Hello from C#</h1><p>{BodyTextBox.Text}</p>",
                IsBodyHtml = true,
            };
            AttachmentsList.ForEach(x => msg.Attachments.Add(x));

            SmtpClient smtpClient = new("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(MailAddress, pwd),
                EnableSsl = true
            };

            Task.Factory.StartNew(() =>
            {
                try
                {
                    smtpClient.Send(msg);
                    MessageBox.Show("The message was sent successfully!");

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.GetType().FullName);
                }
                finally
                {
                    AttachmentsList.Clear();
                }
            });
        }
    }
}
