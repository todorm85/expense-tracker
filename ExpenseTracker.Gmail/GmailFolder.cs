﻿using System;
using System.Linq;
using MailKit;
using MailKit.Net.Imap;
using MimeKit;

namespace ExpenseTracker.GmailConnector
{
    internal class GmailFolder : IDisposable
    {
        public GmailFolder(string user, string pass)
        {
            this.client = new ImapClient();
            this.client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            this.client.Connect("imap.gmail.com", 993, true);

            this.client.Authenticate(user, pass);

            var personal = this.client.GetFolder(this.client.PersonalNamespaces[0]);
            this.expenses = personal.GetSubfolders(false).First(x => x.FullName == "expenses");
            this.expenses.Open(FolderAccess.ReadWrite);
        }

        public int Count
        {
            get
            {
                return this.expenses.Count;
            }
        }

        public MimeMessage GetMessage(int i)
        {
            return this.expenses.GetMessage(i);
        }

        public void Dispose()
        {
            this.client.Dispose();
        }

        internal void MarkRead(int i)
        {
            this.expenses.AddFlags(i, MessageFlags.Seen, true);
        }

        private ImapClient client;
        private IMailFolder expenses;
    }
}