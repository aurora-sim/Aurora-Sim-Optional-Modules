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
using System.IO;

namespace DotNetOpenMail.Encoding
{
	/// <summary>
	/// Encode a file in Base64 Encoding.
	/// 
	/// See: http://www.freesoft.org/CIE/RFC/1521/7.htm
	/// </summary>
	public class Base64Encoder : IEncoder
	{
		/// <summary>
		/// The maximum chars per line before end of line char(s)
		/// </summary>
		public static readonly int MAX_CHARS_PER_LINE=76;

		/// <summary>
		/// The end-of-line character(s) to use
		/// </summary>
		public static readonly String END_OF_LINE="\r\n";

		#region Base64Encoder
		/// <summary>
		/// Empty Constructor
		/// </summary>
		private Base64Encoder()
		{			
		}
		#endregion

		#region GetInstance
		/// <summary>
		/// Create an instance of this class
		/// </summary>
		public static Base64Encoder GetInstance() 
		{
			return new Base64Encoder();
		}
		#endregion

		#region Encode
		/// <summary>
		/// Encode the Stringreader's data in base64.
		/// 
		/// Note: This is not particularly efficient on memory.
		/// This method should probably be improved to 
		/// take advantage of the Reader, rather than 
		/// taking the whole string into memory.
		/// </summary>
		/// <param name="stringreader">Reader for incoming string</param>
		/// <param name="stringwriter">Writer for outgoing encoded string</param>
		/// <param name="encoding">The character encoding for the encoded string.</param>
		public void Encode(StringReader stringreader, StringWriter stringwriter, System.Text.Encoding encoding) 
		{
			try
			{
				String sourceString=stringreader.ReadToEnd();
				stringwriter.Write(EncodeString(sourceString, encoding));
			}
			catch(Exception e)
			{
				throw new Exception("Error in base64Encode" + e.Message);
			}

		}
		#endregion

		#region Encode
		/// <summary>
		/// Encode the File's data in base64.
		/// </summary>
		/// <param name="filestream">Reader for incoming string</param>
		/// <param name="stringwriter">Writer for outgoing encoded string</param>
		/// <param name="charset">The character set for the encoded string</param>
		public void Encode(FileStream filestream, StringWriter stringwriter, System.Text.Encoding charset) 
		{
			//fs = finfo.OpenRead();
			byte[] buffer = new byte[filestream.Length];
			filestream.Read(buffer, 0, buffer.Length);
			filestream.Close();	
			stringwriter.Write(MakeLines(System.Convert.ToBase64String(buffer, 0, buffer.Length)));

			//Encode(new StringReader(System.Text.Encoding.ASCII.GetString(buffer)), stringwriter);
		}
		#endregion

		#region Encode
		/// <summary>
		/// Encode the File's data in base64.
		/// </summary>
		/// <param name="binaryreader">Reader for incoming string</param>
		/// <param name="stringwriter">Writer for outgoing encoded string</param>
		public void Encode(BinaryReader binaryreader, StringWriter stringwriter) 
		{
			//fs = finfo.OpenRead();
			byte[] b;
			int charstoread=Base64Encoder.MAX_CHARS_PER_LINE * 102;
			while (true) 
			{
				b = binaryreader.ReadBytes(charstoread);
				if (b.Length > 0) 
				{
					stringwriter.Write(MakeLines(Convert.ToBase64String(b)));
				}
				else 
				{
					break;
				}
			}
			
				
			//Encode(new StringReader(System.Text.Encoding.ASCII.GetString(buffer)), stringwriter);
		}
		#endregion

		#region EncodeString
		/// <summary>
		/// Encode a string in Base64 in a particular characters set
		/// </summary>
		/// <param name="sourceString">The source text</param>
		/// <param name="charset">the charset for the encoded text</param>
		/// <returns>Returns an encoded string</returns>
		public String EncodeString(String sourceString, System.Text.Encoding charset) 
		{			
			byte[] sourceBytes = new byte[sourceString.Length];
			sourceBytes = charset.GetBytes(sourceString);
			//sourceBytes = System.Text.Encoding.UTF8.GetBytes(sourceString);
			String result=Convert.ToBase64String(sourceBytes);
			return (MakeLines(result));
		}
		#endregion

		#region EncodeHeaderString
		/// <summary>
		/// Encode header as per RFC 2047:
		/// http://www.faqs.org/rfcs/rfc2047.html
		/// </summary>
		/// <param name="name">The header name</param>
		/// <param name="val">The header text to be encoded (data only)</param>
		/// <param name="charset">The charset for the encoded string</param>
		/// <param name="forceencoding">ignored for this class</param>
		/// <returns>Returns the encoded string</returns>
		public String EncodeHeaderString(String name, String val, System.Text.Encoding charset, bool forceencoding) 
		{
			String encodedtext=EncodeString(val, charset);
			String tmp="=?"+charset.HeaderName+"?B?"+encodedtext+"?=";
			return tmp;
			
		}
		#endregion


		#region MakeLines
		/// <summary>
		/// Chop the text into lines that are smaller
		/// than MAX_CHARS_PER_LINE
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		private String MakeLines(String source) 
		{
			StringBuilder sb=new StringBuilder();
			int pos=0;
			for (int i=0; i<source.Length; i++) 
			{
				sb.Append(source[i]);
				pos++;
				if (pos>=Base64Encoder.MAX_CHARS_PER_LINE) 
				{
					sb.Append(Base64Encoder.END_OF_LINE);
					pos=0;
				}				
			}
			return sb.ToString();
		}
		#endregion

		#region ContentTransferEncodingString
		/// <summary>
		/// The String that goes in the content transfer encoding header
		/// </summary>
		public String ContentTransferEncodingString
		{
			get {return "base64";}
		}
		#endregion



	}
}
