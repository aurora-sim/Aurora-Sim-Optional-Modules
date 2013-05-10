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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using DotNetOpenMail.SmtpAuth;

//using log4net;

namespace DotNetOpenMail
{
	/// <summary>
	/// A proxy to access an SMTP server.  This drives 
	/// the protocol interaction.
	/// </summary>
	public class SmtpProxy : TcpClient, ISmtpProxy
	{		
		//private static readonly ILog log = LogManager.GetLogger(typeof(SmtpProxy));
		private SmtpServer _smtpserver=null;
		internal static readonly String ENDOFLINE="\r\n";
		private bool _isConnected=false;
		private bool _captureConversation=false;
		private static readonly String SERVER_LOG_PROMPT="READ> ";
		private static readonly String CLIENT_LOG_PROMPT="SENT> ";

		//private StringBuilder _conversation=new StringBuilder();
		//private IPEndPoint _iEndPoint=null;
		
		/*
		#region SmtpProxy
		private  SmtpProxy(System.Net.IPAddress ipaddress, int portno)
		{
			_iendpoint = new IPEndPoint (ipaddress, portno);
		}
		#endregion
		*/

		#region SmtpProxy
		private SmtpProxy(SmtpServer smtpserver)
		{
			_smtpserver=smtpserver;			
		}
		#endregion

		#region GetInstance
		/// <summary>
		/// Get an instance of the SMTP Proxy
		/// </summary>
		/// <param name="smtpserver"></param>
		/// <returns></returns>
		public static SmtpProxy GetInstance(SmtpServer smtpserver) 
		{
			return new SmtpProxy(smtpserver);
		}
		#endregion

		#region Open
		/// <summary>
		/// Connect to the server and return the initial 
		/// welcome string. Throw a MailException if we 
		/// can't connect.
		/// </summary>
		/// <returns></returns>
		public SmtpResponse Open() 
		{
			this.ReceiveTimeout=_smtpserver.ServerTimeout;

			IPEndPoint ipendpoint=_smtpserver.GetIPEndPoint();
			try 
			{

				Connect(ipendpoint);
			}
			catch (Exception ex)
			{
				//throw new MailException("Could not connect to "+ipendpoint+":"+ipendpoint.Port, ex);
				throw new MailException("Could not connect to "+ipendpoint, ex);
			} 
			_isConnected=true;
	
			return ReadSmtpResponse();
		}
		#endregion

		#region Helo
		/// <summary>
		/// Send the HELO string.
		/// Throw a MailException if we can't connect.
		/// </summary>
		/// <returns>the SMTP response</returns>
		public SmtpResponse Helo(String localHostName) 
		{
			if (!_isConnected) 
			{
				throw new MailException("The connection is closed.");
			}
			String message = "HELO "+localHostName+SmtpProxy.ENDOFLINE;
			//LogDebug("SENDING: "+message);
				
			Write(message);

			return ReadSmtpResponse();
		}
		#endregion

		#region Ehlo
		/// <summary>
		/// Send the EHLO string.
		/// Throw a MailException if we can't connect.
		/// </summary>
		/// <returns>the SMTP response</returns>
		public EhloSmtpResponse Ehlo(String localHostName) 
		{
			if (!_isConnected) 
			{
				throw new MailException("The connection is closed.");
			}
			String message = "EHLO "+localHostName+SmtpProxy.ENDOFLINE;
			//LogDebug("SENDING: "+message);
				
			Write(message);

			return ReadEhloSmtpResponse();
		}
		#endregion

		#region MailFrom
		/// <summary>
		/// Send the MAIL FROM command
		/// Throw a MailException if we can't connect.
		/// </summary>
		/// <param name="mailfrom">The Envelope-From address</param>
		/// <returns>the SMTP response</returns>
		public SmtpResponse MailFrom(EmailAddress mailfrom) 
		{
			if (!_isConnected) 
			{
				throw new MailException("The connection is closed.");
			}
			String message = "MAIL FROM: <"+mailfrom.Email+">"+SmtpProxy.ENDOFLINE;
			Write(message);
			return ReadSmtpResponse();
		}
		#endregion

		#region RcptTo
		/// <summary>
		/// Send the MAIL FROM command
		/// Throw a MailException if we can't connect.
		/// </summary>
		/// <param name="rcpttoaddress">A recipient's address</param>
		/// <returns>the SMTP response</returns>
		public SmtpResponse RcptTo(EmailAddress rcpttoaddress) 
		{
			if (!_isConnected) 
			{
				throw new MailException("The connection is closed.");
			}

			String message= "RCPT TO: <"+rcpttoaddress.Email+">"+SmtpProxy.ENDOFLINE;
			//LogDebug("SENDING: "+message);
			//OnLogWriteSmtp
			Write(message);
			return ReadSmtpResponse();
		}
		#endregion

		#region Data
		/// <summary>
		/// Send the DATA string (without the data)
		/// Throw a MailException if we can't connect.
		/// </summary>
		/// <returns>the SMTP response</returns>
		public SmtpResponse Data() 
		{
			if (!_isConnected) 
			{
				throw new MailException("The connection is closed.");
			}
			String message = "DATA"+SmtpProxy.ENDOFLINE;
			//LogDebug("SENDING: "+message);
				
			Write(message);

			return ReadSmtpResponse();
		}
		#endregion

		#region WriteData
		/// <summary>
		/// Send the message content string
		/// Throw a MailException if we can't 
		/// connect.
		/// </summary>
		/// <returns>the SMTP response</returns>
		public SmtpResponse WriteData(String message) 
		{
			if (!_isConnected) 
			{
				throw new MailException("The connection is closed.");
			}
			
			StringReader reader=new StringReader(message);
			String line=null;

			while ((line=reader.ReadLine())!=null) 				
			{
				// checking for dot at the beginning of the
				// line (RFC821 sec. 4.5.2)
					
				if (line.Length > 0 && line[0]=='.') 
				{
					Write("."+line+SmtpProxy.ENDOFLINE);
				} 
				else 
				{
					Write(line+SmtpProxy.ENDOFLINE);
				}
			}
			Write(SmtpProxy.ENDOFLINE+"."+SmtpProxy.ENDOFLINE);

			return ReadSmtpResponse();
		}
		#endregion

		#region ReadSmtpResponse
		private SmtpResponse ReadSmtpResponse() 
		{
			String response=ReadResponse();
			if (response.Length < 3) 
			{
				throw new MailException("Invalid response from server: \""+response+"\"");				
			}
			String responseCodeStr=response.Substring(0, 3);
			String responseMessage="";
			if (response.Length > 4) 
			{				
				responseMessage=response.Substring(4);
			}
			try 
			{
				int responseCode=Convert.ToInt32(responseCodeStr);
				return new SmtpResponse(responseCode, responseMessage);
			}
			catch 
			{
				throw new MailException("Could not understand response from server: "+response);
			}
			
		}
		#endregion

		#region ReadEhloSmtpResponse
		/// <summary>
		/// Parse the response from the EHLO into an 
		/// EhloSmtpResponse.  Returns 250 "OK" if successful.
		/// </summary>
		/// <returns></returns>
		private EhloSmtpResponse ReadEhloSmtpResponse() 
		{
			//log.Debug("READ THE RESPONSE");
			EhloSmtpResponse ehloResponse=new EhloSmtpResponse();
			String multiLineResponse=ReadResponse();
			StringReader sr=new StringReader(multiLineResponse);
			
			String line=null;
			//log.Debug("READING...");
			while ((line=sr.ReadLine()) !=null) 
			{
				try 
				{
					String responseMessage=String.Empty;
					if (line.Length > 4) 
					{				
						responseMessage=line.Substring(4).Trim();
					}

					//log.Debug("Reading "+line);
					int responseCode=Convert.ToInt32(line.Substring(0, 3));
					if (responseCode==250) 
					{
						if (responseMessage.ToLower().IndexOf("auth")==0) 
						{
							// parse the auth types from the response
							if (responseMessage.Length > 4) 
							{
								// RFC 2554 SMTP Authentication:
								// (3) The AUTH EHLO keyword contains as a parameter a space separated
								// list of the names of supported SASL mechanisms.
								foreach (String authtype in responseMessage.Substring(5).Split(' '))
								{
                                    ehloResponse.AddAvailableAuthType(authtype.ToLower());						
								}
							}
							// return new SmtpResponse(responseCode, responseMessage);
						}
					}
					else  
					{
						ehloResponse.ResponseCode=responseCode;
						ehloResponse.Message=responseMessage;
						return ehloResponse;
					}
					
				}
				catch 
				{
					throw new MailException("Could not understand response from server: "+multiLineResponse);
				}
			}
			ehloResponse.ResponseCode=250;
			ehloResponse.Message="OK";
			return ehloResponse;
		}
		#endregion

		#region Auth
		/// <summary>
		/// Send the AUTH command
		/// Throw a MailException if we can't connect.
		/// </summary>
		/// <returns>the SMTP response</returns>
		public SmtpResponse Auth(String authtype) 
		{
			if (!_isConnected) 
			{
				throw new MailException("The connection is closed.");
			}
			Write("AUTH "+authtype+SmtpProxy.ENDOFLINE);
			return ReadSmtpResponse();
		}
		#endregion

		#region SendString
		/// <summary>
		/// Send any old string to the proxy
		/// </summary>
		/// <returns>the SMTP response</returns>
		public SmtpResponse SendString(String str) 
		{
			if (!_isConnected) 
			{
				throw new MailException("The connection is closed.");
			}
			Write(str+SmtpProxy.ENDOFLINE);
			return ReadSmtpResponse();
		}
		#endregion

		#region Write
		/// <summary>
		/// Write a string to the current connection.
		/// </summary>
		/// <param name="message"></param>
		private void Write(string message)
		{
			try 
			{
				//log.Debug(message);
				_smtpserver.OnLogWriteSmtp(this, message);
				if (_captureConversation) 
				{
					
					_smtpserver.AppendConversation(SmtpProxy.CLIENT_LOG_PROMPT+message);
				}

				System.Text.ASCIIEncoding en = new System.Text.ASCIIEncoding() ;
				byte[] WriteBuffer = new byte[1024] ;
				WriteBuffer = en.GetBytes(message) ;
				
				NetworkStream stream = GetStream() ;
				if (!stream.CanWrite) 
				{
					throw new MailException("Stream could not be opened for writing.");
				}
				stream.Write(WriteBuffer,0,WriteBuffer.Length);
			}
			catch (Exception ex)
			{
				//LogError(ex.Message);
				throw new MailException("Error while sending data to the server: "+ex.Message, ex);
			}

		}
		#endregion

		#region Quit
		/// <summary>
		/// Send the QUIT command
		/// Throw a MailException if we can't connect.
		/// </summary>
		/// <returns>the SMTP response</returns>
		public SmtpResponse Quit() 
		{
			if (!_isConnected) 
			{
				throw new MailException("The connection is closed.");
			}
			Write("QUIT"+SmtpProxy.ENDOFLINE);
			return ReadSmtpResponse();
		}
		#endregion

		#region ReadResponse
		private string ReadResponse()
		{
			try 
			{
				System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\d{3}\s.+");
				string response = String.Empty;
				System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
				byte[] serverbuff = new Byte[1024];
				NetworkStream stream = GetStream();

				if (!stream.CanRead) 
				{
					throw new MailException("Stream could not be read.");
				}
				
				int count = 0;
				// read until last response has been received
				// (indicated by whitespace after the numerical response code)
				do
				{
					int LoopTimeout = 0;
					if (stream.DataAvailable)
					{
						count = stream.Read(serverbuff, 0, serverbuff.Length);  
						response = String.Concat(response, enc.GetString(serverbuff, 0, count));  
					}

					if ((LoopTimeout += 100) > this._smtpserver.ServerTimeout)
					{
						throw new MailException("Multiline server response timed out");
					}
					Thread.Sleep(100);
				}
				while(! regex.IsMatch(response));

				_smtpserver.OnLogReceiveSmtp(this, response);
				if (_captureConversation) 
				{
					this._smtpserver.AppendConversation(SmtpProxy.SERVER_LOG_PROMPT+response);
				}
				return response;
			}
			catch (Exception ex)
			{
				//LogError(ex.Message);
				throw new MailException("Error while receiving data from server: "+ex.Message, ex);
			}
		}
		#endregion

		#region CaptureSmtpConversation
		/// <summary>
		/// Set this to "true" if you want to capture the SMTP negotiation.
		/// Once the conversation has finished, use "GetConversation" to
		/// view it.
		/// </summary>
		public bool CaptureSmtpConversation 
		{
			get {return this._captureConversation;}
			set {this._captureConversation=value;}
		}
		#endregion

		/*
		#region LogError
		private void LogError(String message) 
		{
			//System.Console.Error.WriteLine(message);
			//log.Error(message);
		}
		#endregion

		#region LogDebug
		private void LogDebug(String message) 
		{
			//System.Console.Out.WriteLine(message);
			//log.Debug(message);
		}
		#endregion
		*/
	}
}
