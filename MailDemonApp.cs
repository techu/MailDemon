﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using DnsClient;

using MailKit;
using MailKit.Net;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace MailDemon
{
    public class MailDemonApp
    {
        private static async Task TestClientConnectionAsync(MailDemon demon)
        {
            SmtpClient client = new SmtpClient()
            {
                SslProtocols = System.Security.Authentication.SslProtocols.None,
                Timeout = 60000 // 60 secs
            };
            await client.ConnectAsync("localhost", 25, MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable);
            await client.AuthenticateAsync(new NetworkCredential("noreply", "testpassword!!!@@@###"));
            MimeMessage msg = new MimeMessage
            {
                Body = (new BodyBuilder { HtmlBody = "<html><body><b>Test Email Bold 12345</b></body></html>" }).ToMessageBody(),
                Subject = "test subject"
            };
            msg.From.Add(new MailboxAddress("noreply@" + demon.Domain));
            msg.To.Add(new MailboxAddress("_CHANGE_TO_AN_ACTUAL_EMAIL_ADDRESS_@hotmail.com"));
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
        }

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            MailDemon demon = new MailDemon();
            demon.RunAsync(args, configuration).ConfigureAwait(false);
            Console.WriteLine("Mail demon running, press Ctrl-C to exit");
            TestClientConnectionAsync(demon).ConfigureAwait(false).GetAwaiter().GetResult();
            Console.ReadLine();
        }
    }
}