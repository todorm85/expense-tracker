﻿using System.Linq;
using MailKit;
using MailKit.Net.Imap;

namespace ExpenseTracker.Allianz.Gmail
{
    public class GmailClient : IMailClient
    {
        private IMailFolder expenses;
        private ImapClient client;
        private readonly string user;
        private readonly string pass;

        public GmailClient(string user, string pass)
        {
            this.user = user;
            this.pass = pass;
        }

        public int Count => this.Expenses.Count;

        public IMailFolder Expenses
        {
            get
            {
                if (this.expenses == null)
                {
                    this.expenses = this.Client.GetFolder(this.Client.PersonalNamespaces[0])
                        .GetSubfolders(false).First(x => x.FullName == "expenses");
                    this.expenses.Open(FolderAccess.ReadWrite);
                }

                return this.expenses;
            }
        }

        public ImapClient Client
        {
            get
            {
                if (this.client == null)
                {
                    this.client = new ImapClient();
                    this.client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    this.client.Connect("imap.gmail.com", 993, true);
                    this.client.Authenticate(this.user, this.pass);
                }

                return this.client;
            }
        }

        public ExpenseMessage GetMessage(int i)
        {
            var serverMsg = this.Expenses.GetMessage(i);
            return new ExpenseMessage()
            {
                Body = serverMsg.HtmlBody ?? serverMsg.TextBody,
                Subject = serverMsg.Subject,
                EmailDate = serverMsg.Date.UtcDateTime
            };
        }

        public void Dispose()
        {
            this.client?.Dispose();
        }

        public void MarkRead(int i)
        {
            this.Expenses.AddFlags(i, MessageFlags.Seen, true);
        }

        public void Delete(int i)
        {
            this.Expenses.MoveTo(i, this.Client.GetFolder(SpecialFolder.Trash));
        }
    }
}