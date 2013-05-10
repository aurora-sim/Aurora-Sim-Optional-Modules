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
	/// An SmtpException represents an SMTP response
	/// code and an error message.  Network and connection
	/// errors are represented by a MailException instead.
	/// </summary>
	public class SmtpException : Exception
	{
		private int _errorcode;

		/// <summary>
		/// Create a new instance of the SmtpException
		/// </summary>
		/// <param name="errorcode">The SMTP error code</param>
		public SmtpException(int errorcode)
		{
			this._errorcode=errorcode;
		}

		/// <summary>
		/// Create a new instance of the SmtpException
		/// </summary>
		/// <param name="errorcode">The SMTP error code</param>
		/// <param name="message">The SMTP error message</param>
		public SmtpException(int errorcode, string message) : base(message) 
		{
			this._errorcode=errorcode;
		}

		/// <summary>
		/// Create a new instance of the SmtpException
		/// </summary>
		/// <param name="errorcode">The SMTP error code</param>
		/// <param name="message">The SMTP error message</param>
		/// <param name="inner">The inner exception</param>
		public SmtpException(int errorcode, string message, Exception inner) : base(message, inner)
		{
			this._errorcode=errorcode;
		}

		/// <summary>
		/// The SMTP error code.
		/// </summary>
		public int ErrorCode 
		{
			get {return _errorcode;}
			set {_errorcode=value;}
		}

		/// <summary>
		/// Convert to a string
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return _errorcode+" "+this.Message;
		}


	}
}
