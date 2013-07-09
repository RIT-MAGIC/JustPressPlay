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

		public static void Send(List<String> to, String subject, String body, List<String> cc = null, List<String> bcc = null)
		{
			// Need to have at least one address
			if (to == null && cc == null && bcc == null)
				throw new System.ArgumentNullException("At least one of the address parameters (to, cc and bcc) must be non-null");

			// Set up the built-in MailMessage
			MailMessage mm = new MailMessage();
			mm.From = new MailAddress("jpptest@what-ev.net");
			if (to != null) foreach (String addr in to) mm.To.Add(new MailAddress(addr));
			if (cc != null) foreach (String addr in cc) mm.CC.Add(new MailAddress(addr));
			if (bcc != null) foreach (String addr in bcc) mm.Bcc.Add(new MailAddress(addr));
			mm.Subject = subject;
			mm.IsBodyHtml = false;
			mm.Body = body;
			mm.Priority = MailPriority.Normal;

			// Set up the server communication
			SmtpClient client = new SmtpClient("mail.what-ev.net");
			client.Credentials = new NetworkCredential("jpptest@what-ev.net", "skylanders69"); // TODO: Put this info into the DB

			client.Send(mm);
		}

		public static void Send(String to, String subject, String body, List<String> cc = null, List<String> bcc = null)
		{
			// Pass the single "to" parameter to the other overload
			Send(new List<String>() { to }, subject, body, cc, bcc);
		}
	}
}