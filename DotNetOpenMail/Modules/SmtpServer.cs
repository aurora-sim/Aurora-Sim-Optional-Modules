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
using System.Net;
using System.Text;
using DotNetOpenMail.SmtpAuth;
using DotNetOpenMail.Logging;

//using DomainKeys;

namespace DotNetOpenMail
{
	/// <summary>
	/// A description of an SMTP server.
	/// </summary>
	public class SmtpServer
	{
		//public delegate void SmtpDataMessagePresendHandler(object sender, MyEventArgs e);
		
		/// <summary>
		/// Logging delegate
		/// </summary>
		public delegate void LogHandler(LogMessage logMessage);

		/// <summary>
		/// Handle a message about to be sent
		/// </summary>
		public event LogHandler LogSmtpWrite;

		/// <summary>
		/// Handle a message received
		/// </summary>
		public event LogHandler LogSmtpReceive;

		/// <summary>
		/// Log that the conversation has completed
		/// </summary>
		public event LogHandler LogSmtpCompleted;

		//private static Logger _logger=new Logger();

		#region Data Members
		private IPEndPoint	_ipendpoint;
		private IPAddress	_ipaddress;
		private String		_hostname;
		private int			_port=25;
		private String		_id; // some human readable name
		private int			_serverTimeout=30000; // the timeout in milliseconds
		private String		_helohost=null;
		private ISmtpAuthToken _authToken=null;
		private ISmtpProxy	_smtpProxy=null;
		private bool		_captureSmtpConversation=false;
		private StringBuilder		_smtpConversation=new StringBuilder();
		#endregion

		#region SmtpServer
		/// <summary>
		/// Create a new SMTP server at this hostname and
		/// port.
		/// </summary>
		/// <param name="hostname">The address or hostname of the smtp server</param>
		/// <param name="port">The port of the smtp server.</param>
		public SmtpServer(String hostname, int port)
		{
			_id=hostname+":"+port;
			_hostname=hostname;
			_port=port;
		}
		#endregion

		#region SmtpServer
		/// <summary>
		/// Create a new SMTP server at this hostname and
		/// port.
		/// </summary>
		/// <param name="hostname">The address or hostname of the smtp server</param>			
		public SmtpServer(String hostname) 
		{
			_id=hostname+":"+_port;
			_hostname=hostname;						
		}
		#endregion
		
		#region SmtpServer
		/// <summary>
		/// Create a new SMTP server at this hostname and
		/// port.
		/// </summary>
		/// <param name="ipaddress">The ip address of the SMTP server</param>
		/// <param name="port">The port number of SMTP server</param>
		public SmtpServer(IPAddress ipaddress, int port)
		{
			_id=ipaddress.ToString()+":"+port;
			_ipaddress=ipaddress;
			_port=port;			
		}
		#endregion

		#region GetIPEndPoint
		/// <summary>
		/// Get the end point for the SMTP server.  The IPEndpoint will
		/// be saved for later to prevent multiple hostname lookups.
		/// </summary>
		/// <exception>SmtpException is thrown if the host doesn't resolve.</exception>
		/// <returns>returns an IPEndPoint for the SMTPServer.</returns>
		internal IPEndPoint GetIPEndPoint()
		{
			if (_ipendpoint!=null) 
			{
				return _ipendpoint;
			}
			if (_hostname!=null) 
			{
				IPHostEntry iphostentry=System.Net.Dns.Resolve(_hostname);
				if (iphostentry.AddressList.Length==0) 
				{
					throw new MailException("unable to resolve host: "+_hostname);
				} 
				else 
				{
					_ipendpoint=new IPEndPoint(iphostentry.AddressList[0], _port);
				}
				return this._ipendpoint;
			} 
			else if (_ipaddress!=null)
			{
				_ipendpoint=new IPEndPoint(_ipaddress, _port);
				return _ipendpoint;
			}
			else throw new MailException("Invalid IPEndPoint");
		}
		#endregion

		#region ServerTimeout
		/// <summary>
		/// The timeout waiting for an SMTP command, in
		/// milliseconds
		/// </summary>
		public int ServerTimeout 
		{
			get {return _serverTimeout;}
			set {_serverTimeout=value;}
		}
		#endregion

		#region GetSmtpProxy
		/// <summary>
		/// Get an instance of an SmtpProxy
		/// </summary>
		/// <returns></returns>
		internal ISmtpProxy GetSmtpProxy() 
		{
			if (_smtpProxy!=null) 
			{
				return _smtpProxy;
			} 
			else 
			{
				return SmtpProxy.GetInstance(this);
			}
		}
		#endregion

		#region GetHeloHost
		private String GetHeloHost() 
		{
			if (_helohost!=null) 
			{
				return _helohost;
			}
			else 
			{
				//String hostname=Environment.MachineName.ToLower();
				String hostname=Dns.GetHostName();
				if (hostname!=null) 
				{
					return hostname;					
				} 
				else 
				{
					return "localhost";
				}
			} 
		}
		#endregion

		#region UseEhlo
		/// <summary>
		/// Figure out whether we need EHLO or not.
		/// Currently, this is only for SMTP AUTH
		/// </summary>
		/// <returns></returns>
		private bool UseEhlo() 
		{
			if (this._authToken!=null) 
			{
				return true;
			} 
			else 
			{
				return false;
			}
		}
		#endregion

		#region Send
		/// <summary>
		/// Send the email.
		/// </summary>
		/// <param name="emailMessage">The completed message</param>
		/// <param name="rcpttocollection">A list of email addresses which
		/// are to be used in the RCPT TO SMTP communication</param>
		/// <param name="mailfrom">An email address for the MAIL FROM 
		/// part of the SMTP protocol.</param>
		/// <exception cref="SmtpException">throws an SmtpException if there
		/// is an unexpected SMTP error number sent from the server.</exception>
		/// <exception cref="MailException">throws a MailException if there
		/// is a network problem, connection problem, or other issue.</exception>
		/// <returns></returns>
		internal bool Send(ISendableMessage emailMessage, EmailAddressCollection rcpttocollection, EmailAddress mailfrom) 
		{
			//ISmtpNegotiator negotiator=_smtpserver.GetSmtpNegotiator();
			ISmtpProxy smtpproxy=GetSmtpProxy();
			//smtpproxy.CaptureSmtpConversation=CaptureSmtpConversation;
			SmtpResponse smtpResponse=smtpproxy.Open();
			try 
			{
				#region Connect
				if (smtpResponse.ResponseCode!=220)
				{
					throw smtpResponse.GetException();
				}
				#endregion

				#region HELO / EHLO
				if (UseEhlo()) 
				{
					EhloSmtpResponse esmtpResponse=smtpproxy.Ehlo(GetHeloHost());
					if (esmtpResponse.ResponseCode!=250)
					{
						// TODO: FIX THIS
						throw new SmtpException(esmtpResponse.ResponseCode, esmtpResponse.Message);
					}

					// do SMTP AUTH
					if (this._authToken!=null) 
					{
						
						smtpResponse=_authToken.Negotiate(smtpproxy, esmtpResponse.GetAvailableAuthTypes());
						if (smtpResponse.ResponseCode!=235) 
						{
							throw smtpResponse.GetException();
						}
					}
					

				} 
				else 
				{

					smtpResponse=smtpproxy.Helo(GetHeloHost());
					if (smtpResponse.ResponseCode!=250)
					{
						throw smtpResponse.GetException();
					}
				}
				#endregion

				#region MAIL FROM	
				smtpResponse=smtpproxy.MailFrom(mailfrom);
				if (smtpResponse.ResponseCode!=250)
				{
					throw smtpResponse.GetException();
				}
				#endregion

				#region RCPT TO

				foreach ( EmailAddress rcpttoaddress in rcpttocollection)
				{
					smtpResponse=smtpproxy.RcptTo(rcpttoaddress);
					if (smtpResponse.ResponseCode!=250)
					{
						throw smtpResponse.GetException();
					}
				}
				#endregion
				
				#region DATA

				smtpResponse=smtpproxy.Data();
				if (smtpResponse.ResponseCode!=354)
				{
					throw smtpResponse.GetException();
				}
				//smtpResponse=negotiator.WriteData();
				
				String message=emailMessage.ToDataString();
				if (message==null) 
				{
					throw new MailException("The message content is null");
				}
				/*
				// START Test With Domain Keys
				// (this would appear as an event callback if it works)
				MailSigner mailsigner=new MailSigner();
				bool signed = mailsigner.signMail(message);
				if (!signed) 
				{
					throw new MailException("Error creating DomainKeys signature.");
				}
				message=mailsigner.signedHeader + message;
				// END Test With Domain Keys
				*/


				// Send the data
				smtpResponse=smtpproxy.WriteData(message);
				if (smtpResponse.ResponseCode!=250)
				{
					throw smtpResponse.GetException();
				}
				#endregion
				
				#region QUIT
				// QUIT
				smtpResponse=smtpproxy.Quit();
				if (smtpResponse.ResponseCode!=221)
				{
					throw smtpResponse.GetException();
				}
				#endregion

			}
			finally 
			{
				smtpproxy.Close();
				OnLogSmtpCompleted(this, "Connection Closed");
			}
			return true;
		}
		#endregion

		#region OverrideSmtpProxy
		/// <summary>
		/// Override the SmtpProxy.  This is only
		/// for testing smtp negotiation without an 
		/// smtp server.
		/// </summary>
		/// <param name="smtpProxy"></param>
		public void OverrideSmtpProxy(ISmtpProxy smtpProxy) 
		{
			_smtpProxy=smtpProxy;
		}
		#endregion
		
		#region SmtpAuthToken
		/// <summary>
		/// Get or set the SMTP AUTH token for the server.
		/// </summary>
		public ISmtpAuthToken SmtpAuthToken
		{
			get {return _authToken;}
			set {_authToken = value;}
		}
		#endregion		

		#region CaptureSmtpConversation
		/// <summary>
		/// If true, the following call to "Send" will capture the 
		/// most recent SMTP conversation to the SmtpServer object.
		/// This is intended for debugging only.
		/// This is deprecated in favour of the log message events.
		/// </summary>
		[ObsoleteAttribute("This property is deprecated.  You "+
			 "should use LogSmtpWrite and LogSmtpReceive instead.  "+
			 "This will be removed in a future version.", false)]
		public bool CaptureSmtpConversation 
		{
			get 
			{
				return _captureSmtpConversation;
			}
			set 
			{
				_captureSmtpConversation=value;
				if (!_captureSmtpConversation) 
				{
					this._smtpConversation=new StringBuilder();
				}
			}
		}
		#endregion

		#region AppendConversation
		/// <summary>
		/// Append this string to the conversation log.
		/// </summary>
		/// <param name="str">the string from the SMTP conversation</param>
		internal void AppendConversation(String str) 
		{
			this._smtpConversation.Append(str);
		}
		#endregion

		#region GetSmtpConversation
		/// <summary>
		/// If CaptureSmtpConversation is true, a Send() will
		/// be captured, and will be accessible here.
		/// </summary>
		/// <returns>The SMTP Conversation or null.</returns>
		[ObsoleteAttribute("This method is deprecated.  You "+
			 "should use LogSmtpWrite and LogSmtpReceive instead.  "+
			 "This will be removed in a future version.", false)]
		public String GetSmtpConversation() 
		{
			if (_smtpConversation==null || _smtpConversation.Length==0) 
			{
				return null;
			}
			else 
			{
				return _smtpConversation.ToString();
			}
		}
		#endregion

		
		#region OnLogWriteSmtp
		internal void OnLogWriteSmtp(Object sender, String message) 
		{
			if (LogSmtpWrite!=null) 
			{
				LogSmtpWrite(new LogMessage(sender, message));
			}
		}
		#endregion

		#region OnLogReceiveSmtp
		internal void OnLogReceiveSmtp(Object sender, String message) 
		{
			if (LogSmtpReceive!=null) 
			{
				LogSmtpReceive(new LogMessage(sender, message));
			}
		}
		#endregion

		#region OnLogSmtpCompleted
		internal void OnLogSmtpCompleted(Object sender, String message) 
		{
			if (LogSmtpCompleted!=null) 
			{
				LogSmtpCompleted(new LogMessage(sender, message));
			}
		}
		#endregion

		#region Hostname
		/// <summary>
		/// The server hostname
		/// </summary>
		public String Hostname 
		{
			get {return _hostname;}
		}
		#endregion

		#region Port
		/// <summary>
		/// The server port
		/// </summary>
		public int Port 
		{
			get {return _port;}
		}
		#endregion

		/// <summary>
		/// Return the host name, a colon, and a port number.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Hostname+":"+Port;
		}

		/// <summary>
		/// Set the hostname used by the HELO 
		/// command.
		/// </summary>
		public String HeloHost 
		{
			get 
			{
				return _helohost;
			}
			set 
			{
				_helohost=value;
			}
		}


	}
}
