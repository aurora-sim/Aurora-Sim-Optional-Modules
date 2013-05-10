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

using DotNetOpenMail.Encoding;

namespace DotNetOpenMail
{
	/// <summary>
	/// A mime representation of a file which can be transferred
	/// via email.
	/// </summary>
	public class FileAttachment : AbstractEmailAttachment
	{
		
		//private static readonly ILog log = LogManager.GetLogger(typeof(FileAttachment));

		/// <summary>
		/// Create a new File attachment from a FileInfo object
		/// </summary>
		/// <param name="fileinfo">A fileinfo object which points to a local file.</param>
		public FileAttachment(FileInfo fileinfo) : this(fileinfo, null)
		{			
		}

		/// <summary>
		/// Create a new text file attachment from a StreamReader.
		/// </summary>
		/// <param name="streamreader">An open streamreader</param>
		public FileAttachment(StreamReader streamreader) : this(streamreader, null)
		{
		}

		/// <summary>
		/// Create a new binary file attachment from a BinaryReader.
		/// </summary>
		/// <param name="binaryreader">An open binaryreader</param>
		public FileAttachment(BinaryReader binaryreader) : this(binaryreader, null)
		{
		}

		/// <summary>
		/// Create a new binary file attachment from an array of bytes.
		/// </summary>
		/// <param name="bytes">An array of bytes</param>
		public FileAttachment(byte[] bytes) : this(bytes, null)
		{
		}

		/// <summary>
		/// Create a new file attachment from a String.
		/// </summary>
		/// <param name="content">The contents of the file</param>
		public FileAttachment(String content) : this(content, null)
		{
		}

		#region FileAttachment
		/// <summary>
		/// Create a new File attachment with a contentid from a FileInfo object.
		/// The contentid will be used to refer to this attachment in 
		/// another mime part of the email.
		/// </summary>
		/// <param name="fileinfo">A fileinfo object which points to a local file.</param>
		/// <param name="contentid">the content id value</param>
		public FileAttachment(FileInfo fileinfo, String contentid) 
		{
			this.ContentType="binary/octet-stream";
			if (FileName==null) 
			{
				FileName=fileinfo.Name;
			}
			this._fileinfo=fileinfo;
			this._contentid=contentid;
			this.CharSet=null;
			//this.Contents=contents;
			this.Encoding=EncodingType.Base64;
			//this.Encoder=EncoderFactory.GetEncoder(EncoderFactory.GetEncoder(EncodingType);
			
		}
		#endregion
		
		#region FileAttachment
		/// <summary>
		/// Create a new file attachment from a StreamReader.
		/// The contentid will be used to refer to this attachment in 
		/// another mime part of the email. 
		/// </summary>
		/// <param name="streamreader">An open streamreader</param>
		/// <param name="contentid">the content id value</param>
		public FileAttachment(StreamReader streamreader, String contentid) 
		{
			if (this.ContentType==null) 
			{
				this.ContentType="binary/octet-stream";
			}
			String tmp=streamreader.ReadToEnd();
			streamreader.Close();
			this.Contents=tmp;
			this.CharSet=null;
			this._contentid=contentid;
			this.Encoding=EncodingType.Base64;			
		}
		#endregion

		#region FileAttachment
		/// <summary>
		/// Create a new file attachment from a BinaryReader.
		/// The contentid will be used to refer to this attachment in 
		/// another mime part of the email. 
		/// </summary>
		/// <param name="binaryreader">An open binary readerr</param>
		/// <param name="contentid">the content id value</param>
		public FileAttachment(BinaryReader binaryreader, String contentid) 
		{
			if (this.ContentType==null) 
			{
				this.ContentType="binary/octet-stream";
			}
			
			this._contentbytes=Utils.BinaryReaderUtil.ReadIntoByteArray(binaryreader);
			binaryreader.Close();
			this.CharSet=null;
			this._contentid=contentid;
			this.Encoding=EncodingType.Base64;			
		}
		#endregion

		#region FileAttachment
		/// <summary>
		/// Create a new file attachment from a byte array.
		/// The contentid will be used to refer to this attachment in 
		/// another mime part of the email. 
		/// </summary>
		/// <param name="bytes">An array of bytes</param>
		/// <param name="contentid">the content id value</param>
		public FileAttachment(byte[] bytes, String contentid) 
		{
			if (this.ContentType==null) 
			{
				this.ContentType="binary/octet-stream";
			}
			this.ContentBytes=bytes;
			this.CharSet=null;
			this.Encoding=EncodingType.Base64;			
		}
		#endregion

		/// <summary>
		/// Create a new text file attachment from a String.
		/// The contentid will be used to refer to this attachment in 
		/// another mime part of the email.
		/// </summary>
		/// <param name="content">The contents of the file</param>
		/// <param name="contentid">the content id value</param>
		public FileAttachment(String content, String contentid) 
		{
			if (this.ContentType==null) 
			{
				this.ContentType="binary/octet-stream";
			}
			this.Contents=content;
			this.CharSet=null;			
			this.Encoding=EncodingType.Base64;	
			this._contentid=contentid;
		}
	
	}
}
