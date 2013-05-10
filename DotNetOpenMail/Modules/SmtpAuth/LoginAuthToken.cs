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
using System.Text;

//using log4net;

using DotNetOpenMail.Encoding;

namespace DotNetOpenMail.SmtpAuth
{
	/// <summary>
	/// A LOCAL SMTP AUTH token
	/// </summary>
	public class LoginAuthToken : ISmtpAuthToken
	{	
		//private static readonly ILog log = LogManager.GetLogger(typeof(LoginAuthToken));
		private String _userName;
		private String _password;
		private System.Text.Encoding _charEncoding=System.Text.Encoding.GetEncoding("iso-8859-1");

		/// <summary>
		/// Create a new instance of the LOCAL SMTP
		/// AUTH token
		/// </summary>
		public LoginAuthToken(String userName, String password)
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

		/*
		/// <summary>
		/// Return true if this is among the supported AUTH
		/// types.
		/// </summary>
		/// <param name="authtypes"></param>
		/// <returns></returns>
		public bool IsSupported(SmtpAuthType[] authtypes) 
		{
			log.Debug("CHECKING SUPPORTED TYPES");
			for (int i=0; i<authtypes.Length; i++) 			
			{
				log.Debug("CHECKING IF "+authtypes[i]+"="+SmtpAuthType.Login);
				if (authtypes[i]==SmtpAuthType.Login) 
				{
					return true;
				}
			}
			return false;						
		}
		*/

		/// <summary>
		/// Return the 235 response code if valid, otherwise
		/// return the error.
		/// </summary>
		/// <param name="smtpProxy"></param>
		/// <param name="supportedAuthTypes">the supported auth types</param>
		/// <returns></returns>
		public SmtpResponse Negotiate(ISmtpProxy smtpProxy, String[] supportedAuthTypes) 
		{
			SmtpResponse response=smtpProxy.Auth("login");
			//log.Debug("RESPONSE WAS "+response.ResponseCode+" "+response.Message);
			if (response.ResponseCode!=334) 
			{
				return response;
			}
			Base64Encoder encoder=Base64Encoder.GetInstance();
			response=smtpProxy.SendString(encoder.EncodeString(this.UserName, this._charEncoding ));
			if (response.ResponseCode!=334) 
			{
				return response;
			}
			response=smtpProxy.SendString(encoder.EncodeString(this.Password, this._charEncoding ));
			if (response.ResponseCode!=334) 
			{
				// here it's an error
				return response;
			} 
			else 
			{
				// here it's ok.
				return response;
			}
			

		}

	}
}
