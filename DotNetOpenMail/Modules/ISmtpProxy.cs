using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

//using log4net;

namespace DotNetOpenMail
{
	/// <summary>
	/// Interact directly with the SMTP server.
	/// </summary>
	public interface ISmtpProxy
	{
		/// <summary>
		/// Open the connection
		/// </summary>
		/// <returns>The SMTP response</returns>
		SmtpResponse Open();

		/// <summary>
		/// Send the HELO command
		/// </summary>
		/// <param name="localHostName"></param>
		/// <returns></returns>
		SmtpResponse Helo(String localHostName);

		/// <summary>
		/// Send the EHLO command
		/// </summary>
		/// <param name="localHostName"></param>
		/// <returns></returns>
		EhloSmtpResponse Ehlo(String localHostName);

		/// <summary>
		/// Send the MAIL FROM command
		/// </summary>
		/// <param name="mailfrom"></param>
		/// <returns></returns>
		SmtpResponse MailFrom(EmailAddress mailfrom);

		/// <summary>
		/// Send one RCPT TO command
		/// </summary>
		/// <param name="rcptto"></param>
		/// <returns></returns>
		SmtpResponse RcptTo(EmailAddress rcptto);

		/// <summary>
		/// Send the DATA command
		/// </summary>
		/// <returns></returns>
		SmtpResponse Data();

		/// <summary>
		/// Write the message content
		/// </summary>
		/// <returns></returns>
		SmtpResponse WriteData(String str);

		/// <summary>
		/// Send the QUIT message to the server
		/// </summary>
		/// <returns></returns>
		SmtpResponse Quit();

		/// <summary>
		/// Close the connection
		/// </summary>
		/// <returns>The SMTP response</returns>
		void Close();

		/// <summary>
		/// Send the AUTH command
		/// </summary>
		/// <returns>the SMTP response</returns>
		SmtpResponse Auth(String authtype);

		/// <summary>
		/// Send any old string to the proxy
		/// </summary>
		/// <returns>the SMTP response</returns>
		SmtpResponse SendString(String str);

		/// <summary>
		/// Turn the SMTP Conversation Capture off/on.
		/// If on, GetSmtpConversation will contain the
		/// most recent conversation.
		/// </summary>
		/// <returns>the on/off value</returns>
		bool CaptureSmtpConversation {get; set;}


	}
}
