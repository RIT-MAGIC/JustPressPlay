using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;

namespace JustPressPlay.Utilities
{
	public static class Email
	{
		/// <summary>
		/// Sends an email
		/// </summary>
		/// <param name="to">The list of recipients</param>
		/// <param name="subject">The subject of the email</param>
		/// <param name="body">The body of the email, which may contain HTML</param>
		/// <param name="htmlEmail">Should the email be flagged as "html"?</param>
		/// <param name="cc">A list of CC recipients</param>
		/// <param name="bcc">A list of BCC recipients</param>
		public static void Send(NetworkCredential credentials, List<String> to, String subject, String body, bool htmlEmail = false, List<String> cc = null, List<String> bcc = null)
		{
			// Need to have at least one address
			if (to == null && cc == null && bcc == null)
				throw new System.ArgumentNullException("At least one of the address parameters (to, cc and bcc) must be non-null");

			// Set up the built-in MailMessage
			MailMessage mm = new MailMessage();
			mm.From = new MailAddress(credentials.UserName, "Just Press Play");
			if (to != null) foreach (String addr in to) mm.To.Add(new MailAddress(addr, "Test"));
			if (cc != null) foreach (String addr in cc) mm.CC.Add(new MailAddress(addr));
			if (bcc != null) foreach (String addr in bcc) mm.Bcc.Add(new MailAddress(addr));
			mm.Subject = subject;
			mm.IsBodyHtml = htmlEmail;
			mm.Body = body;
			mm.Priority = MailPriority.Normal;

			// Set up the server communication
            SmtpClient client = new SmtpClient
                {
                    Host = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.SMTPServer),
                    Port = int.Parse(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.SMTPPort)),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = credentials
                };
           
			client.Send(mm);
		}

		/// <summary>
		/// Sends an email
		/// </summary>
		/// <param name="to">The single recipient</param>
		/// <param name="subject">The subject of the email</param>
		/// <param name="body">The body of the email, which may contain HTML</param>
		/// <param name="htmlEmail">Should the email be flagged as "html"?</param>
		/// <param name="cc">A list of CC recipients</param>
		/// <param name="bcc">A list of BCC recipients</param>
		public static void Send(NetworkCredential credentials, String to, String subject, String body, bool htmlEmail = false, List<String> cc = null, List<String> bcc = null)
		{
			// Pass the single "to" parameter to the other overload
			Send(credentials,new List<String>() { to }, subject, body, htmlEmail, cc, bcc);
		}
	}
}