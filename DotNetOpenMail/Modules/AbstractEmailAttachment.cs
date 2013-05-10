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
using System.Text;

using DotNetOpenMail.Encoding;

namespace DotNetOpenMail
{
	/// <summary>
	/// This is an abstract version of the email attachment.  It may or
	/// may not contain content, a character encoding method, a 
	/// mime-encoding method, a content-type designation, a content-id 
	/// designation, among other things
	/// </summary>
	public abstract class AbstractEmailAttachment : IEmailStringable
	{
		internal static readonly String CONTENTTYPE="Content-Type";
		internal static readonly String CONTENTTRANSFERENCODING="Content-Transfer-Encoding";
		internal static readonly String CONTENTDIPOSITION="Content-Disposition";
		internal static readonly String CONTENTID="Content-ID";
		internal static readonly String CONTENTDESCRIPTION="Content-Description";
		internal static readonly String CHARSET="charset";

		/// <summary>
		/// The mail encoding type (e.g. quoted-printable, etc)
		/// </summary>
		protected EncodingType _encodingtype;

		/// <summary>
		/// The "content type" of the attachment
		/// </summary>
		protected String _contenttype;

		/// <summary>
		/// The content-transfer encoding of the attachment
		/// </summary>
		protected String _contenttransferencoding;

		/// <summary>
		/// The content disposition of the attachment
		/// </summary>
		protected String _contentdisposition;

		/// <summary>
		/// The content description of the attachment
		/// </summary>
		protected String _contentdescription;

		/// <summary>
		/// The character set of the encoded text
		/// </summary>
		protected System.Text.Encoding _charset=null;

		/// <summary>
		/// The unencoded contents, as a string (optional)
		/// </summary>
		protected String _contents=null;

		/// <summary>
		/// An optional content id
		/// </summary>
		protected String _contentid=null;

		/// <summary>
		/// The file name to identify the content.
		/// (This can be different from the actual file name,
		/// if there was one.)
		/// </summary>
		protected String _filename=null;

		/// <summary>
		/// The file source to read from
		/// </summary>
		protected FileInfo _fileinfo=null;

		/// <summary>
		/// The binary source of the file
		/// </summary>
		protected Byte[] _contentbytes=null;

		/// <summary>
		/// Empty constructor
		/// </summary>
		public AbstractEmailAttachment()
		{		
		}
		
		#region GetEncodedContents
		/// <summary>
		/// Get the contents, encoded for shipping via SMTP
		/// </summary>
		/// <param name="encoder">The encoder to encode the contents</param>
		/// <returns>returns the encoded string, ready for SMTP transfer</returns>
		internal virtual String GetEncodedContents(IEncoder encoder) 
		{
			StringBuilder sb=new StringBuilder();
			System.Text.Encoding charset=null;
			if (_charset==null) 
			{
				charset=System.Text.Encoding.UTF8;
			} 
			else 
			{
				charset=this._charset;
			}

			if (_contents!=null) 
			{
				encoder.Encode(new StringReader(_contents), new StringWriter(sb), charset);
			} 
			else if (_fileinfo!=null)
			{
				encoder.Encode(_fileinfo.OpenRead(), new StringWriter(sb), charset);
			}
			else if (_contentbytes!=null)
			{
				encoder.Encode(new BinaryReader(new MemoryStream(_contentbytes)), new StringWriter(sb));
			}
			String encodedcontents=sb.ToString();
			
			if (encodedcontents.Length!=0 && encodedcontents[encodedcontents.Length-1]!='\n') 
			{
				return encodedcontents+SmtpProxy.ENDOFLINE+SmtpProxy.ENDOFLINE;
			} 
			else 
			{
				return encodedcontents+SmtpProxy.ENDOFLINE;
			}
		}
		#endregion

		#region ToDataString
		/// <summary>
		/// Return the encoded contents, including mime
		/// header.
		/// </summary>
		/// <returns></returns>
		public String ToDataString() 
		{
			IEncoder encoder=EncoderFactory.GetEncoder(Encoding);
			StringBuilder sb=new StringBuilder();
			sb.Append(GetInternalMimeHeader(encoder));
			sb.Append(GetEncodedContents(encoder));			
			return sb.ToString();
		}
		#endregion

		#region Encoding
		/// <summary>
		/// The encoding type for this attachment.
		/// </summary>
		public EncodingType Encoding 
		{
			get {return _encodingtype;}
			set {_encodingtype=value;}
		}
		#endregion
		
		#region ContentType
		/// <summary>
		/// The "content type" of the attachment
		/// </summary>
		public String ContentType 
		{
			get {return _contenttype;}
			set {_contenttype=value;}
		}
		#endregion

		#region CharSet
		/// <summary>
		/// The character set of the encoded text
		/// </summary>
		public System.Text.Encoding CharSet 
		{
			get {return _charset;}
			set {_charset=value;}
		}
		#endregion
	
		#region ContentId
		/// <summary>
		/// An optional content id that is used to refer to
		/// the attachment from elsewhere within a multipart/related
		/// email.
		/// </summary>
		public String ContentId 
		{
			get {return _contentid;}
			set {_contentid=value;}
		}
		#endregion

		#region FileName
		/// <summary>
		/// The file name to attach to the attachment.
		/// </summary>
		public String FileName 
		{
			get {return _filename;}
			set {_filename=value;}
		}
		#endregion

		#region Contents
		/// <summary>
		/// The unencoded contents, as a string (optional)
		/// </summary>
		public String Contents 
		{
			get {return _contents;}
			set {_contents=value;}
		}
		#endregion

		#region ContentBytes
		/// <summary>
		/// The raw bytes of the content (if this is the way
		/// it was set.)
		/// </summary>
		public byte[] ContentBytes 
		{
			get {return _contentbytes;}
			set {_contentbytes=value;}
		}
		#endregion

		#region GetInternalMimeHeader
		/// <summary>
		/// Create the internal mime header, as found on the
		/// mime-attachment itself.
		/// </summary>
		/// <param name="encoder"></param>
		/// <returns></returns>
		protected String GetInternalMimeHeader(IEncoder encoder) 
		{
			return MakeContentType()+
				MakeTransferEncoding(encoder)+
				MakeContentId()+
				MakeContentDisposition()+
				MakeContentDescription()+
				SmtpProxy.ENDOFLINE;
		}
		#endregion

		#region MakeContentType
		private String MakeContentType() 
		{
			StringBuilder sb=new StringBuilder(AbstractEmailAttachment.CONTENTTYPE+": "+ContentType+";"+SmtpProxy.ENDOFLINE);
			if (_charset!=null) 
			{
				// changed by Pietrovich to fix
				// windows-1251 .NET encoding bug.
				// windows-1251 BodyName returns "koi8-r" 
				//	instead of "windows-1251"
				// but still encodes string as windows-1251
				string bodyName = _charset.BodyName;

				//#if (FORCEWIN1251)
                if ("koi8-r" == _charset.BodyName && "windows-1251" == _charset.HeaderName) 
				{
					bodyName = _charset.HeaderName;
				}
				//#endif

				sb.Append("       "+AbstractEmailAttachment.CHARSET + "=\"" + 
					bodyName +  "\""+SmtpProxy.ENDOFLINE);
				//	_charset.BodyName +  "\""+SmtpProxy.ENDOFLINE);
			}
			if (FileName!=null) 
			{
				sb.Append("        name=\""+FileName+"\""+SmtpProxy.ENDOFLINE);
			}
			return sb.ToString();
				
				
		}
		#endregion

		#region MakeTransferEncoding
		private String MakeTransferEncoding(IEncoder encoder) 
		{
			return AbstractEmailAttachment.CONTENTTRANSFERENCODING+": "+encoder.ContentTransferEncodingString+SmtpProxy.ENDOFLINE;
		}
		#endregion 

		#region MakeContentId
		private String MakeContentId() 
		{
			if (ContentId!=null) 
			{
				return AbstractEmailAttachment.CONTENTID+": <" + ContentId + ">" + SmtpProxy.ENDOFLINE;
			} 
			return "";

		}
		#endregion 

		#region MakeContentDisposition
		private String MakeContentDisposition() 
		{
			if (ContentId!=null) 
			{
				return "";
			}
			if (ContentType!=null && FileName !=null) 
			{
				return AbstractEmailAttachment.CONTENTDIPOSITION+": attachment;"+SmtpProxy.ENDOFLINE+
					"        filename=\""+FileName+"\""+SmtpProxy.ENDOFLINE;
			} 
			return "";

		}
		#endregion 

		#region MakeContentDescription
		private String MakeContentDescription() 
		{
			return "";
		}
		#endregion

		

	}
}
