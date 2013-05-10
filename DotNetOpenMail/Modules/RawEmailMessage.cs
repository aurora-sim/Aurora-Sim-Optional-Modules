/*
 * Copyright (c) 2005 Mike Bridge <mike@bridgecanada.com>
 * 
 * Permission is hereby granted, free of charge, to any 
 * person obtaining a copy of this software and associated 
 * documentation files (the "Software"), to deal in the 
 * Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, 
 * distribute, sublicense, and/or sell copies of the 
 * Software, and to permit persons to whom the Software 
 * is furnished to do so, subject to the following 
 * conditions:
 *
 * The above copyright notice and this permission notice 
 * shall be included in all copies or substantial portions 
 * of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF 
 * ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT 
 * SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR 
 * ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN 
 * ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
 * OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;

namespace DotNetOpenMail
{
	/// <summary>
	/// A full, preformatted email message, including headers
	/// </summary>
	public class RawEmailMessage : ISendableMessage
	{
		private String _content=null;
		private EmailAddressCollection _rcpttoaddresses=new EmailAddressCollection();
		private EmailAddress _mailfrom=null;

		/// <summary>
		/// Create an instance of the RawEmailMessage
		/// </summary>
		public RawEmailMessage()
		{
		}

		#region Content
		/// <summary>
		/// The raw content of the email
		/// </summary>
		public String Content 
		{
			get { return _content; }
			set { _content = value; }
		}
		#endregion

		#region MailFrom
		/// <summary>
		/// The address to use in the envelope-from
		/// SMTP negotiation
		/// </summary>
		public EmailAddress MailFrom 
		{
			get { return _mailfrom; }
			set { _mailfrom = value; }
		}
		#endregion

		#region RcptToAddresses
		/// <summary>
		/// The recipient addresses
		/// </summary>
		public EmailAddressCollection RcptToAddresses
		{
			get { return _rcpttoaddresses; }
		}
		#endregion

		#region AddRcptToAddress
		/// <summary>
		/// Add a recipient to the EmailAddressCollection
		/// of recipients
		/// </summary>
		/// <param name="emailaddress"></param>
		public void AddRcptToAddress(EmailAddress emailaddress) 
		{
			_rcpttoaddresses.Add(emailaddress);
		}
		#endregion

		#region Send
		/// <summary>
		/// Send the email via the specified SMTP server
		/// </summary>
		/// <param name="smtpserver">The SMTP server to use</param>
		/// <returns>true if the email was sent</returns>
		public bool Send(SmtpServer smtpserver) 
		{
			// smtpproxy=SmtpProxy.GetInstance(smtpserver);
			if (_rcpttoaddresses.Count==0)
			{
				throw new MailException("Please include a RCPT TO address");
			}
			if (_mailfrom==null) 
			{
				throw new MailException("Please include a MAIL FROM address");
			}
			return smtpserver.Send(this, _rcpttoaddresses, _mailfrom);
		}
		#endregion

		#region ToDataString
		/// <summary>
		/// Render the message for smtp "DATA" transmission.
		/// </summary>
		/// <returns>The rendered String, which in this 
		/// case is the content itself, untouched.</returns>
		public String ToDataString() 
		{	
			return _content;
		}
		#endregion

	}
}
