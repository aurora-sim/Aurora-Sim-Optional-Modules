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
	/// Summary description for SmtpAuthHelper.
	/// </summary>
	internal class SmtpAuthFactory
	{
		//private static readonly ILog log = LogManager.GetLogger(typeof(SmtpAuthFactory));

		internal SmtpAuthFactory()
		{
		}

		/// <summary>
		/// Get the Auth type from the SMTP EHLO response.
		/// If it is unrecognized or unimplemented, 
		/// return null.  Case doesn't matter.
		/// </summary>
		/// <param name="authType">The string as returned in EHLO negotiation.</param>
		/// <param name="username">The user's login</param>
		/// <param name="password">The User's password</param>
		internal static ISmtpAuthToken GetAuthTokenFromString(String authType, String username, String password) 
		{
			authType=authType.ToLower().Trim();
			
			if (authType.Equals("login")) 
			{
				return new LoginAuthToken(username, password);
			}
			else 
			{
				return null;
			}
		}
	}
}
