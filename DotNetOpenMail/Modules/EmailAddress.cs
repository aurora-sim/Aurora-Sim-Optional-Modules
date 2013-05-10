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
using DotNetOpenMail.Encoding;

namespace DotNetOpenMail
{
	/// <summary>
	/// An email address.
	/// The two parts of the email address object
	/// are the email address itself and the
	/// name.  The name may be encoded if desired.
	/// </summary>
	public class EmailAddress : IEmailStringable
	{
		private String _email=null;
		private String _name=null;
		private System.Text.Encoding _charset;
		private Encoding.EncodingType _encodingtype=Encoding.EncodingType.QuotedPrintable;

		#region EmailAddress
		/// <summary>
		/// Create an instance of an email address object.
		/// </summary>
		/// <param name="email">The address portion of the email</param>
		/// <param name="name">The name portion of the email.</param>
		public EmailAddress(String email, String name)
		{
			if (email!=null) 
			{
				email=email.Trim();
			}
			this._email=email;
			this._name=name;
		}
		#endregion

		#region EmailAddress
		/// <summary>
		/// Create an instance of an email address object.
		/// </summary>
		/// <param name="email">The address portion of the email</param>
		public EmailAddress(String email)
		{
			if (email!=null) 
			{
				email=email.Trim();
			} 
			else 
			{
				email="";
			}
			this._email=email;
		}
		#endregion

		#region EmailAddress
		/// <summary>
		/// Create an character-encoded instance of an email address object.
		/// </summary>
		/// <param name="email">The address portion of the email</param>
		/// <param name="name">The name portion of the email.</param>
		/// <param name="charset">The character set to encode the name portion of the email address.</param>
		/// <param name="encodingtype">The encoding type to use to encode the name portion of the mail address.</param>
		public EmailAddress(String email, String name, Encoding.EncodingType encodingtype, System.Text.Encoding charset)
		{

			if (email!=null) 
			{
				email=email.Trim();
			}
			this._email=email;
			this._name=name;
			this._charset=charset;
			this._encodingtype=encodingtype;

		}
		#endregion		

		#region Email
		/// <summary>
		/// The email address portion of the email address object
		/// </summary>
		public String Email
		{
			get {return _email;}
		}
		#endregion

		#region Name
		/// <summary>
		/// The name portion of the email address object
		/// </summary>
		public String Name 
		{
			get {return _name;}
		}
		#endregion

		#region QuoteSpecials
		private String QuoteSpecials(String str) 
		{
			StringBuilder sb=new StringBuilder();
			bool needsQuotes=false;
			for (int i=0; i< str.Length; i++) 
			{
				char ch=str[i];
				if (ch=='\"') 
				{
					needsQuotes=true;
					sb.Append('\\');
				}
				else if (ch=='(' || ch==')' ||
					ch=='<' || ch=='>' ||
					ch==']' || ch=='[' ||
					ch==':' || ch==';' ||
					ch=='@' || ch=='\\' ||
					ch==',' || ch=='.')
				{
					needsQuotes=true;
					
				}
				sb.Append(ch);

			}
			if (needsQuotes) 
			{
				return "\""+sb.ToString()+"\"";
			} 
			else 
			{
				return sb.ToString();
			}

		}
		#endregion

		#region ToDataString
		/// <summary>
		/// Create the string representation of this email address
		/// (encoded or otherwise) as it will appear in the email
		/// </summary>
		/// <returns></returns>
		public String ToDataString() 
		{
			if (_name!=null && _name.Trim()!="") 
			{
				
				IEncoder encoder=Encoding.EncoderFactory.GetEncoder(_encodingtype);
				System.Text.Encoding thecharset=_charset;
				if (thecharset==null) 
				{
					thecharset=Utils.Configuration.GetInstance().GetDefaultCharset();
				}
				String encodedname=encoder.EncodeHeaderString("", QuoteSpecials(Name.Trim()), thecharset, false);
				
				return encodedname+" <"+Email+">";
			} 
			else 
			{
				return "<"+Email+">";
			}
		}
		#endregion

		#region ToString
		/// <summary>
		/// Calls ToDataString.
		/// </summary>
		/// <returns></returns>
		public override String ToString() 
		{
			return ToDataString();
		}
		#endregion

	}
}
