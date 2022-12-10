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

        private readonly List<Attachment> AttachmentsList;
        private readonly OpenFileDialog OpenFileDialog;

        private readonly SmtpClient smtpClient;
        private readonly string MailAddress;

        public MainWindow(SmtpClient client, string mailAddress)
        {
            InitializeComponent();

            AttachmentsList = new();
            Dialog = new()
            {
                Filter = "All files (*.*)|*.*",
                FilterIndex = 0,
                Multiselect = true,
            };

            smtpClient = client;
            MailAddress = mailAddress;
        }

        private void AddAttachmentBtn_Click(object sender, RoutedEventArgs e)
        {
            Dialog.FileName = string.Empty;

            if (Dialog.ShowDialog() == true)
            {
                foreach (var path in Dialog.FileNames)
                    AttachmentsList.Add(new Attachment(path));
            }
        }

        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ToTextBox.Text)) return;

            MailMessage msg = new(MailAddress, ToTextBox.Text)
            {
                Priority = _MailPriority,
                Subject = SubjectTextBox.Text,
                Body = $"<h1>Hello from C#</h1><p>{BodyTextBox.Text}</p>",
                IsBodyHtml = true,
            };
            AttachmentsList.ForEach(x => msg.Attachments.Add(x));

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
