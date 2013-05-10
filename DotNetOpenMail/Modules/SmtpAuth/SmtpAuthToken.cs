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
//using log4net;

namespace DotNetOpenMail.SmtpAuth
{
	/// <summary>
	/// Token which will negotiate an authentication type
	/// with the server.
	/// </summary>
	public class SmtpAuthToken : ISmtpAuthToken
	{
		//private static readonly ILog log = LogManager.GetLogger(typeof(SmtpAuthToken));
		private String _userName;
		private String _password;
		private System.Text.Encoding _charEncoding=System.Text.Encoding.GetEncoding("iso-8859-1");

		/// <summary>
		/// Create a new instance of the LOCAL SMTP
		/// AUTH token
		/// </summary>
		public SmtpAuthToken(String userName, String password)
		{
			this._userName=userName;
			this._password=password;
		}

		/// <summary>
		/// The User's password
		/// </summary>
		public String Password
		{
			get {return _password;}
			set {_password=value;}
		}

		/// <summary>
		/// The User's login
		/// </summary>
		public String UserName
		{
			get {return _userName;}
			set {_userName=value;}
		}

		/// <summary>
		/// Return the 235 response code if valid, otherwise
		/// return the error.
		/// </summary>
		/// <param name="smtpProxy">The SmtpProxy being used</param>
		/// <param name="supportedAuthTypes">String array of EHLO-specified auth types</param>
		/// <returns></returns>
		public SmtpResponse Negotiate(ISmtpProxy smtpProxy, String[] supportedAuthTypes) 
		{
			ISmtpAuthToken token=null;
			foreach (String authType in supportedAuthTypes) 
			{

				if (authType.Equals("login")) 
				{
					token=new LoginAuthToken(_userName, _password);
					break;
				}
			}
			if (token==null) 
			{
				return new SmtpResponse(504, DotNetOpenMail.Resources.ARM.GetInstance().GetString("unrecognized_auth_type"));
			}
			
			return token.Negotiate(smtpProxy, supportedAuthTypes);
		}

	}
}
