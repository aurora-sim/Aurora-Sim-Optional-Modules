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
using DotNetOpenMail.Encoding;

namespace DotNetOpenMail
{
	/// <summary>
	/// This represents one of the message headers as it
	/// appears in the email.
	/// </summary>
	public class MessageHeader : IEmailStringable
	{
		private String _name;
		private String _value;
		private String _encodedvalue=null;

		/// <summary>
		/// Create a new instance of a MessageHeader
		/// </summary>
		/// <param name="name">The header name (without a colon)</param>
		/// <param name="value">The unencoded value</param>
		public MessageHeader(string name, String value)
		{
			this._name=name;
			this._value=value;
		}

		/// <summary>
		/// Create a new instance of a MessageHeader, with a mime-encoded
		/// value.
		/// </summary>
		/// <param name="name">The header name (without a colon)</param>
		/// <param name="value">The non-encoded header value</param>
		/// <param name="encodedvalue">The mime-encoded value</param>
		public MessageHeader(string name, String value, String encodedvalue)
		{
			this._name=name;
			this._value=value;
			this._encodedvalue=encodedvalue;
		}

		/// <summary>
		/// The name of the header.
		/// </summary>
		public String Name 
		{
			get {return this._name;}
		}

		/// <summary>
		/// The unencoded value of the header
		/// </summary>
		public String Value
		{
			get {return this._value;}
		}

		/// <summary>
		/// The mime-encoded value of the header
		/// </summary>
		public String EncodedValue
		{
			get {return this._encodedvalue;}
		}

		/// <summary>
		/// The string representation of the header, as it will
		/// appear in the email.
		/// </summary>
		/// <returns></returns>
		public String ToDataString() 
		{
			if (_encodedvalue!=null) 
			{
				return Name+": "+EncodedValue+SmtpProxy.ENDOFLINE;
			} 
			else 
			{
				return Name+": "+Value+SmtpProxy.ENDOFLINE;
			}				
		}

	}
}
