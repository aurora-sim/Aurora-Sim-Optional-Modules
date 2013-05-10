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

namespace DotNetOpenMail.Encoding
{
	/// <summary>
	/// An interface for a string encoder
	/// </summary>
	public interface IEncoder
	{

		/// <summary>
		/// Encode from the stringreader to the stringwriter.
		/// </summary>
		/// <param name="source">an open stringreader to the source string</param>
		/// <param name="destination">where the output is written</param>
		/// <param name="charset">the outgoing charset</param>
		void Encode(StringReader source, StringWriter destination, System.Text.Encoding charset);

		/// <summary>
		/// Encode the file stream into the stringwriter.
		/// </summary>
		/// <param name="filestream">an open stream to file to be read</param>
		/// <param name="destination">where the output is written</param>
		/// <param name="charset">the outgoing charset</param>
		void Encode(FileStream filestream, StringWriter destination, System.Text.Encoding charset);

		/// <summary>
		/// Encode the binary reader into the stringwriter.
		/// </summary>
		/// <param name="binaryreader">an open binary reader for the data to be encoded.</param>
		/// <param name="destination">where the output is written</param>
		void Encode(BinaryReader binaryreader, StringWriter destination);


		/// <summary>
		/// Encode the string according to rfc-2047 if some of it
		/// falls outside the 7bit ASCII charset.
		/// </summary>
		/// <param name="name">The header key</param>
		/// <param name="val">The header value string</param>
		/// <param name="charset">Charset for the encoded string</param>
		/// <param name="forceencoding">Force encoding, even if in ascii-only.</param>
		/// <returns>The encoded string</returns>
		String EncodeHeaderString(String name, String val, System.Text.Encoding charset, bool forceencoding); 

		/// <summary>
		/// The String that goes in the content transfer encoding header
		/// </summary>
		String ContentTransferEncodingString {get;}


	}
}
