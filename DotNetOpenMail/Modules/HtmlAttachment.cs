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
using DotNetOpenMail.Utils;

namespace DotNetOpenMail
{
	/// <summary>
	/// An HTML email attachment
	/// </summary>
	public class HtmlAttachment : AbstractEmailAttachment
	{

		/// <summary>
		/// Create a new HTML Attachment.  It flags the content/type
		/// as "text/html", uses Quoted Printable encoding by
		/// default, and uses the default character set, which is
		/// ISO-8859-1 unless otherwise specified in the .config file.
		/// </summary>
		/// <param name="contents">The HTML content of the attachment</param>
		public HtmlAttachment(String contents)
		{
			this.ContentType="text/html";
			this.Contents=contents;
			this.CharSet=System.Text.Encoding.GetEncoding(Configuration.GetInstance().GetDefaultEncoding("iso-8859-1"));
			this.Encoding=EncodingType.QuotedPrintable;			
		}
	}
}
