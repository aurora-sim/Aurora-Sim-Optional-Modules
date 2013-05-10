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

//using log4net;

using DotNetOpenMail;

namespace DotNetOpenMail.Encoding
{
	/// <summary>
	/// See: http://www.freesoft.org/CIE/RFC/1521/6.htm
	/// </summary>
	public class QPEncoder : IEncoder
	{
		/// <summary>
		/// Logger
		/// </summary>
		//private static readonly ILog log = LogManager.GetLogger(typeof(QPEncoder));

		/// <summary>
		/// Maximum characters per line, before the end-of-line character
		/// </summary>
		public static readonly int MAX_CHARS_PER_LINE=76;

		/// <summary>
		/// The end-of-line character(s).
		/// </summary>
		public static readonly String END_OF_LINE="\r\n";
	

		#region QPEncoder
		/// <summary>
		/// Empty Constructor
		/// </summary>
		private QPEncoder()
		{
		}
		#endregion

		#region GetInstance
		/// <summary>
		/// Create an instance of the encoder
		/// </summary>
		/// <returns>The instantiated Quoted-printable encoder</returns>
		public static QPEncoder GetInstance() 
		{
			return new QPEncoder();
		}
		#endregion

		#region Encode
		/// <summary>
		/// Encode the incoming stream in quoted-printable encoding.
		/// </summary>
		/// <param name="binaryreader">The incoming binary reader</param>
		/// <param name="stringwriter">The outgoing string writer</param>
		public void Encode(BinaryReader binaryreader, StringWriter stringwriter) 
		{
			throw new ApplicationException("Not Implemented Yet; use a base64 encoder instead.");
		}
		#endregion

		#region Encode
		/// <summary>
		/// Encode the incoming stream in quoted-printable encoding.
		/// </summary>
		/// <param name="filestream">The incoming file stream</param>
		/// <param name="stringwriter">The outgoing string writer</param>
		/// <param name="charset">The charset to write the outgoing string in</param>
		public void Encode(FileStream filestream, StringWriter stringwriter, System.Text.Encoding charset) 
		{
			throw new ApplicationException("Not Implemented Yet; use a base64 encoder instead.");
		}
		#endregion

		#region Encode
		/// <summary>
		/// Encode the string read by the stringreader into the 
		/// stringwriter.  This implementation does not encode
		/// the characters where encoding is optional, and does
		/// not encode spaces or tabs (except at the end of a line).
		/// 
		/// Line breaks are converted to the RFC line break (CRLF).
		//  The last linebreak is not included, if any, but this 
		/// </summary>
		/// <param name="stringreader">The incoming string reader</param>
		/// <param name="stringwriter">The outgoing stringwriter</param>
		/// <param name="charset">The outgoing charset for the string</param>
		public void Encode(StringReader stringreader, StringWriter stringwriter, System.Text.Encoding charset) 
		{
			Encode(stringreader, stringwriter, charset, false, 0);
		}
		#endregion

		#region EncodeString
		/// <summary>
		/// Encode the string in quoted printable format
		/// </summary>
		/// <param name="str">The source string</param>
		/// <param name="charset">The outgoing charset</param>
		/// <returns>the encoded string</returns>
		public String EncodeString(String str, System.Text.Encoding charset) 
		{
			return EncodeString(str, charset, false, 0);
		}
		#endregion 

		#region EncodeString
		/// <summary>
		/// Encode the string in quoted printable format
		/// </summary>
		/// <param name="sourceString">The source string</param>
		/// <param name="charset">The outgoing charset</param>
		/// <param name="forceRFC2047">Force encoding, even if not required by qp RFC</param>
		/// <param name="offset">The total characters outside the encoding.  This is used to figure
		/// out how long the line will be after encoding.</param>
		/// <returns>the encoded string</returns>
		public String EncodeString(String sourceString, System.Text.Encoding charset, bool forceRFC2047, int offset) 
		{

			StringReader sr=new StringReader(sourceString);			
			StringBuilder sb=new StringBuilder();
			StringWriter sw=new StringWriter(sb);			
			Encode(sr, sw, charset, forceRFC2047, offset);
			return sb.ToString();
			
		}
		#endregion

		#region EncodeHeaderString
		/// <summary>
		/// Encode header as per RFC 2047:
		/// http://www.faqs.org/rfcs/rfc2047.html
		/// 
		/// </summary>
		/// <remarks>This doesn't split long lines yet</remarks>
		/// <param name="name">the header name</param>
		/// <param name="val">the string to encode</param>
		/// <param name="charset">The charset to encode to</param>
		/// <param name="forceencoding">Force encoding, even if not required by qp RFC</param>
		/// <returns>the encoded string, suitable for use in a header</returns>
		public String EncodeHeaderString(String name, String val, System.Text.Encoding charset, bool forceencoding) 
		{
			if (forceencoding || IsNonAscii(val)) 
			{
				
				String start="=?"+charset.HeaderName+"?Q?";
				String end="?=";
				
				int offset=name.Length+start.Length+end.Length+2; // the 2 is for the colon and space

				String encodedtext=EncodeString(val, charset, true, offset);
				//log.Debug("LINE BEFORE SPLIT IS "+encodedtext);
				if (encodedtext.Length + offset > QPEncoder.MAX_CHARS_PER_LINE) 
				{
					StringBuilder sb=new StringBuilder("");
					foreach (String line in encodedtext.Split('\n')) 
					{
						String tmp=line;
						if (sb.Length>0) 
						{
							sb.Append(QPEncoder.END_OF_LINE+"	");
						}
						tmp=line.TrimEnd(new char[] {'\r', '='});
						//if (line.LastIndexOf("\r") == line.Length-1) 
						//{
							//tmp=line.Substring(0, line.Length-1);
						//}
						sb.Append(start+tmp+end);
					}
					return sb.ToString();
				} 
				else 
				{	
					return start+encodedtext+end;
				}
			}
			else 
			{
				return val;
			}
		}
		#endregion

		#region EncodeChar
		/// <summary>
		/// Encode a char according to quoted-printable
		/// standard
		/// </summary>
		/// <param name="ch">the char to encode</param>
		/// <returns>the encoded char representation</returns>
		private String EncodeChar(char ch) 
		{
			int intch=(int) ch;
			if (intch > 255) 
			{
				//log.Debug("encoding qp: "+String.Format("0x{0:X4}", intch));
				int ch1=intch >> 8;
				int ch2=intch & 0xff;
				return String.Format("={0:X2}={1:X2}", ch1, ch2);
			} 
			else 
			{
				return String.Format("={0:X2}",(int) ch);
			}
		}
		#endregion

		#region EncodeByte
		/// <summary>
		/// Encode a byte according to quoted-printable
		/// standard
		/// </summary>
		/// <param name="ch">the byte to encode</param>
		/// <returns>the encoded byte representation</returns>
		private String EncodeByte(byte ch) 
		{
			return String.Format("={0:X2}",(int) ch);
		}
		#endregion

		#region ContentTransferEncodingString
		/// <summary>
		/// The String that goes in the content transfer encoding header
		/// </summary>
		public String ContentTransferEncodingString
		{
			get {return "quoted-printable";}
		}
		#endregion

		#region NeedsEncoding
		/// <summary>
		/// Return true if the char needs to be encoded.
		/// </summary>
		/// <param name="ch"></param>
		/// <param name="forceRFC2047"></param>
		/// <returns></returns>
		internal bool NeedsEncoding(char ch, bool forceRFC2047) 
		{
			if ((ch <= 0x1f && ch != 0x09) || // less than space (except tab)
				ch == 0x3d || // equal sign
				ch>= 0x7f    // above 7bit
				|| (forceRFC2047 && (ch==0x20 || ch==0x09 || ch==0x3f))
				) 
			{
				return true;
			}
			else 
			{
				return false;
			}

		}
		#endregion

		#region NeedsEncoding
		/// <summary>
		/// Return true if the byte needs to be encoded.
		/// </summary>
		/// <param name="ch"></param>
		/// <param name="forceRFC2047"></param>
		/// <returns></returns>
		internal bool NeedsEncoding(byte ch, bool forceRFC2047) 
		{
			if ((ch <= 0x1f && ch != 0x09) || // less than space (except tab)
				ch == 0x3d || // equal sign
				ch>= 0x7f    // above 7bit
				|| (forceRFC2047 && (ch==0x20 || ch==0x09 || ch==0x3f))
				) 
			{
				return true;
			}
			else 
			{
				return false;
			}

		}
		#endregion

		#region IsNonAscii
		/// <summary>
		/// Return true if the string needs to be encoded.
		/// </summary>
		/// <param name="str">The string to check</param>
		/// <returns>true if outside 127-bit, or one of the
		/// quoted-printable special characters.</returns>
		internal bool IsNonAscii(String str) 
		{
			for (int i=0; i<str.Length; i++) 
			{
				if (IsNonAscii(str[i])) 
				{
					return true;
				}
			}
			return false;
		}
		#endregion

		#region IsNonAscii
		/// <summary>
		/// Check if the character is one of the non-qp 
		/// characters
		/// </summary>
		/// <param name="ch">the character to check</param>
		/// <returns>true if outside the acceptable qp range</returns>
		internal bool IsNonAscii(char ch) 
		{
			if ((ch <= 0x1f && ch != 0x09) || // less than space (except tab)
				ch == 0x3d || // equal sign
				ch>= 0x7f    // above 7bit
				) 
			{
				return true;
			}
			else 
			{
				return false;
			}
		}
		#endregion

		#region Encode
		/// <summary>
		/// Encode the string read by the stringreader into the 
		/// stringwriter.  This implementation does not encode
		/// the characters where encoding is optional, and does
		/// not encode spaces or tabs (except at the end of a line).
		///
		/// Line breaks are converted to the RFC line break (CRLF).
		//  The last linebreak is not included, if any---although this 
		/// may change in the future.
		/// </summary>
		/// <param name="stringreader">The incoming string reader</param>
		/// <param name="stringwriter">The outgoing stringwriter</param>
		/// <param name="charset">The outgoing charset for the string</param>
		/// <param name="forceRFC2047">If true, force the encoding of all the
		/// <param name="offset">These are the number of characters that 
		/// will appear outside this string, e.g. the header name, etc.</param>
		/// characters (not just those that are given in RFC2027).</param>
		public void Encode(StringReader stringreader, StringWriter stringwriter, System.Text.Encoding charset, bool forceRFC2047, int offset) 
		{
			if (offset > QPEncoder.MAX_CHARS_PER_LINE - 15) 
			{
				throw new MailException("Invalid offset (header name is too long): "+offset);
			}

			String line=null;
			
			bool forceencoding=false;
			if (charset==null) 
			{
				charset=Utils.Configuration.GetInstance().GetDefaultCharset();
			}
			while((line=stringreader.ReadLine())!=null) 
			{
				int columnposition=0;
				
				byte[] bytes=charset.GetBytes(line);

				// see Rule #3 (spaces and tabs at end of line are encoded)
				StringBuilder blankendchars=new StringBuilder("");
				
				int endpos=bytes.Length-1;

				while (endpos >=0 && (bytes[endpos]==0x20 || bytes[endpos]==0x09) ) 
				{
					blankendchars.Insert(blankendchars.Length, EncodeByte(bytes[endpos]));
					endpos--;
				}


				for (int i=0; i<=endpos; i++) 
				{
					String towrite="";
					if (forceencoding || NeedsEncoding(bytes[i], forceRFC2047))
					{						
						columnposition+=3;
						towrite=EncodeByte(bytes[i]);						
					} 
					else 
					{
						columnposition+=1;
						// this is a single byte, so multibyte chars won't
						// be affected by this.
						towrite=charset.GetString(new byte[]{bytes[i]});
					}

					if (offset + columnposition > QPEncoder.MAX_CHARS_PER_LINE ) 
					{
						stringwriter.Write("="+QPEncoder.END_OF_LINE);
						columnposition=0;
					}
					stringwriter.Write(towrite);					
				}

				if (blankendchars.Length > 0) 
				{
					if (offset+columnposition+blankendchars.Length > QPEncoder.MAX_CHARS_PER_LINE) 
					{
						stringwriter.Write("="+QPEncoder.END_OF_LINE);
					}
					stringwriter.Write(blankendchars);
				}
				if (stringreader.Peek() >= 0) 
				{
					stringwriter.Write("\r\n");
				}
			}
		}
		#endregion


	}
}
