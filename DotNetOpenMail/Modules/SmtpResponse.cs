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
	/// The response from the SMTP server
	/// </summary>
	public class SmtpResponse
	{
		private int _responseCode;
		private String _message;
		
		/// <summary>
		/// Create an instance of the SmtpResponse
		/// </summary>
		/// <param name="responseCode"></param>
		/// <param name="message"></param>
		public SmtpResponse(int responseCode, String message)
		{
			this._responseCode=responseCode;
			this._message=message;
		}

		/// <summary>
		/// The SMTP Reponse code
		/// </summary>
		public int ResponseCode
		{
			get { return _responseCode; }
		}

		/// <summary>
		/// The SMTP Response String
		/// </summary>

		public String Message 
		{
			get { return _message; }
		}

		/// <summary>
		/// Was the response a fatal error?
		/// </summary>
		public bool IsFatalError 
		{
			get {
				return (this._responseCode>=500 && this._responseCode <= 599);
			}
		}

		/// <summary>
		/// Was the response an OK message?
		/// </summary>
		public bool IsOk 
		{
			get 
			{
				return (this._responseCode>=200 && this._responseCode <= 299);
			}
		}

		/// <summary>
		/// Convert this response to an exception
		/// </summary>
		/// <returns>The SmtpException corresponding to this response</returns>
		public SmtpException GetException() 
		{
			return new SmtpException(this._responseCode, this._message);
		}

	}
}
