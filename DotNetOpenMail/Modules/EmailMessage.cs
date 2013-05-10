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
using System.Collections;
using System.Text;

using DotNetOpenMail.Resources;
using DotNetOpenMail.Utils;
using DotNetOpenMail.Encoding;

//using log4net;

namespace DotNetOpenMail
{
	/// <summary>
	/// An Email Message
	/// </summary>
	public class EmailMessage : ISendableMessage
	{

		#region variables
		//private static readonly ILog log = LogManager.GetLogger(typeof(EmailMessage));
		
		private String _subject=null;
		private String _organization=null;
		private String _bodytext=null;

		private EmailAddress _fromaddress=null;
		private EmailAddressCollection _toaddresses=new EmailAddressCollection();
		private EmailAddressCollection _ccaddresses=new EmailAddressCollection();
		private EmailAddressCollection _bccaddresses=new EmailAddressCollection();

		private EmailAddress _envelopefromaddress=null;

		private String _contenttype;
		private MimeBoundary _mimeboundary;

		private TextAttachment _textpart=null;
		private HtmlAttachment _htmlpart=null;

		private String _xmailer=null;
		
		private ArrayList _mixedfileattachments=new ArrayList();
		private ArrayList _relatedfileattachments=new ArrayList();
		private MessageHeaderCollection _custommessageheaders=new MessageHeaderCollection();
		private System.Text.Encoding _charset;
		private Encoding.EncodingType _encodingtype=Encoding.EncodingType.QuotedPrintable;
		#endregion 

		#region EmailMessage
		/// <summary>
		/// Create a new instance of an EmailMessage.
		/// </summary>
		public EmailMessage()
		{
			_mimeboundary=new MimeBoundary();
			_charset=System.Text.Encoding.GetEncoding(Configuration.GetInstance().GetDefaultEncoding("iso-8859-1"));
			_bodytext=GetNoMimeMessage();
		}
		#endregion

		#region Subject
		/// <summary>
		/// The subject line or subject header of the
		/// email.
		/// </summary>
		public String Subject 
		{
			get {return _subject;}
			set {_subject=value;}
		}
		#endregion

		#region FromAddress
		/// <summary>
		/// The address that appears in the From header.  It will
		/// also be used as the Envelope From address in the SMTP 
		/// negotiation, unless it is overridden by the EnvelopeFromAddress
		/// setting.
		/// </summary>
		public EmailAddress FromAddress 
		{
			get {return _fromaddress;}
			set {_fromaddress=value;}
		}
		#endregion

		#region EnvelopeFromAddress
		/// <summary>
		/// Normally the FromAddress is used as the
		/// envelope-from address, but it can be
		/// overridden here, if it is not null.
		/// </summary>
		public EmailAddress EnvelopeFromAddress 
		{
			get {return _envelopefromaddress;}
			set {_envelopefromaddress=value;}
		}
		#endregion

		#region AddToAddress
		/// <summary>
		/// Add a To address to the Headers.  This will also
		/// be used during the "RCPT TO" SMTP negotiation
		/// </summary>
		/// <param name="emailaddress">Email Address object of the recipient</param>
		public void AddToAddress(EmailAddress emailaddress) 
		{
			_toaddresses.Add(emailaddress);
		}
		/// <summary>
		/// Add a To address to the Headers.  This will also
		/// be used during the "RCPT TO" SMTP negotiation
		/// </summary>
		/// <param name="email">The plain email address (don't include the name)</param>
		public void AddToAddress(String email) 
		{
			_toaddresses.Add(new EmailAddress(email));
		}
		#endregion

		#region ToAddresses
		/// <summary>
		/// Retrieve the collection of recipients
		/// that will appear in the "To" header of the
		/// email.
		/// </summary>
		public EmailAddressCollection ToAddresses 
		{
			get {return _toaddresses;}
		}
		#endregion

		#region AddCcAddress
		/// <summary>
		/// Add a Cc address to the Headers.  This will also
		/// be used during the "RCPT TO" SMTP negotiation
		/// </summary>
		/// <param name="emailaddress">Email Address object of the recipient</param>
		public void AddCcAddress(EmailAddress emailaddress) 
		{
			_ccaddresses.Add(emailaddress);
		}
		/// <summary>
		/// Add a Cc address to the Headers.  This will also
		/// be used during the "RCPT TO" SMTP negotiation
		/// </summary>
		/// <param name="email">The plain email address (don't include the name)</param>
		public void AddCcAddress(String email) 
		{
			_ccaddresses.Add(new EmailAddress(email));
		}
		#endregion

		#region CcAddresses
		/// <summary>
		/// Retrieve the collection of recipients
		/// that will appear in the "Cc" header of the
		/// email.
		/// </summary>
		public EmailAddressCollection CcAddresses 
		{
			get {return _ccaddresses;}
		}
		#endregion

		#region AddBccAddress
		/// <summary>
		/// Add a recipient who will be "Blind Carbon Copied"
		/// as a recipient of the email.  BCC addresses are
		/// not added to the email headers, but only appear
		/// during the "RCPT TO" SMTP negotiation.
		/// </summary>
		/// <param name="emailaddress">The EmailAddress object</param>		
		public void AddBccAddress(EmailAddress emailaddress) 
		{
			_bccaddresses.Add(emailaddress);
		}

		/// <summary>
		/// Add a recipient who will be "Blind Carbon Copied"
		/// as a recipient of the email.  BCC addresses are
		/// not added to the email headers, but only appear
		/// during the "RCPT TO" SMTP negotiation.
		/// </summary>
		/// <param name="email">The plain email address (don't include the name)</param>
		public void AddBccAddress(String email) 
		{
			_bccaddresses.Add(new EmailAddress(email));
		}
		#endregion

		#region BccAddresses
		/// <summary>
		/// Get the current EmailAddressCollection of BCC addresses
		/// </summary>
		public EmailAddressCollection BccAddresses 
		{
			get {return _bccaddresses;}
		}
		#endregion

		#region Organization
		/// <summary>
		/// Get/set the organization (as it will appear in the organization
		/// header
		/// </summary>
		public String Organization 
		{
			get {return _organization;}
			set {_organization=value;}
		}
		#endregion

		#region XMailer
		/// <summary>
		/// Get the current XMailer setting for the X-Mailer header.
		/// </summary>
		public String XMailer
		{
			get {return this._xmailer;}
			set {this._xmailer=value;}
		}
		#endregion

		#region AddCustomHeader
		/// <summary>
		/// Add a custom mail header, e.g. "X-MyHeader".
		/// </summary>
		/// <param name="header">The header name (without a colon)</param>
		/// <param name="val">The value of the header.  This value will not
		/// be encoded.</param>
		public void AddCustomHeader(String header, String val) 
		{
			_custommessageheaders.Add(new MessageHeader(header, val));
		}
		#endregion

		#region HeaderCharSet
		/// <summary>
		/// The charset that is used for encoding the headers
		/// </summary>
		public System.Text.Encoding HeaderCharSet 
		{
			get {return _charset;}
			set {_charset=value;}
		}
		#endregion

		#region HeaderEncoding
		/// <summary>
		/// The mime encoding that is used for encoding the headers
		/// </summary>
		public Encoding.EncodingType HeaderEncoding 
		{
			get {return this._encodingtype;}
			set {_encodingtype=value;}
		}
		#endregion

		#region GetStandardHeaders
		/// <summary>
		/// Get the minimal header set in the specified character,
		/// and using the mime encoder provided.
		/// </summary>
		private MessageHeaderCollection GetStandardHeaders(System.Text.Encoding charset, IEncoder encoder) 
		{

			MessageHeaderCollection standardheaders=new MessageHeaderCollection();

			standardheaders.Add(new MessageHeader("From", _fromaddress.ToDataString()));
			standardheaders.Add(new MessageHeader("To", _toaddresses.ToDataString()));

			if (_ccaddresses.Count>0) 
			{
				standardheaders.Add(new MessageHeader("Cc", _ccaddresses.ToString()));
			}
			//if (_bccaddresses.Count>0) 
			//{
			//standardheaders.Add(new MessageHeader("Bcc", _bccaddresses.ToString()));
			//}

			String subject=_subject;
			if (subject==null || subject.Trim()=="") 
			{
				subject=EncodeHeaderValue("Subject", ARM.GetInstance().GetString("no_subject"));
			}
			standardheaders.Add(new MessageHeader("Subject", EncodeHeaderValue("Subject", subject)));
			standardheaders.Add(new MessageHeader("Date", new RFC2822Date(DateTime.Now, TimeZone.CurrentTimeZone).ToString()));
			if (!HasBodyTextOnly()) 
			{
				standardheaders.Add(new MessageHeader("MIME-Version", "1.0"));
			}
			#region X-Mailer Header
			if (_xmailer!=null) 
			{
				standardheaders.Add(new MessageHeader("X-Mailer", _xmailer));
			} 
			else 
			{
				String xmailer=Configuration.GetInstance().GetXSender();
				if (xmailer!=null) 
				{
					standardheaders.Add(new MessageHeader("X-Mailer", xmailer));
				} 
				else 
				{
					standardheaders.Add(new MessageHeader("X-Mailer", "DotNetOpenMail "+VersionInfo.GetInstance().ToString()));
				}
			}
			#endregion

			String contentType=null;
			if (HasBodyTextOnly()) 
			{
				// ignore the mime stuff for now
			} 
			else 
			{
				if (HasMixedContent()) 
				{
					contentType="multipart/mixed;"+SmtpProxy.ENDOFLINE+
						"        boundary=\""+_mimeboundary.BoundaryString+"\"";
				} 
				else if (HasRelatedContent()) 
				{
					contentType="multipart/related;"+SmtpProxy.ENDOFLINE+
						"        boundary=\""+_mimeboundary.BoundaryString+"\"";		
				}
				else 
				{
					contentType="multipart/alternative;"+SmtpProxy.ENDOFLINE+
						"        boundary=\""+_mimeboundary.BoundaryString+"\"";
				}

				// note: see http://www.faqs.org/rfcs/rfc2045.html , part 6.4
				//contentType+="        charset=\""+charset.BodyName+"\"";
			
				standardheaders.Add(new MessageHeader("Content-Type", contentType));
				//standardheaders.Add(new MessageHeader("Content-Transfer-Encoding", encoder.ContentTransferEncodingString));
			}
			return standardheaders;
		}
		#endregion

		#region EncodeHeaderValue
		/// <summary>
		/// Encode a header value using the current header encoder and charset.
		/// </summary>
		/// <param name="name">The header name</param>
		/// <param name="val">The header value</param>
		/// <returns></returns>
		private String EncodeHeaderValue(String name, String val) 
		{
			IEncoder encoder=Encoding.EncoderFactory.GetEncoder(this.HeaderEncoding);
			return encoder.EncodeHeaderString(name, val, this.HeaderCharSet, false);
		}
		#endregion

		#region HasMixedContent
		/// <summary>
		/// Returns true if there is content that should be "multipart/mixed"
		/// </summary>
		/// <returns>true if the content is multipart/mixed.</returns>
		private bool HasMixedContent()
		{
			return (_mixedfileattachments.Count > 0);
		}
		#endregion

		#region HasRelatedContent
		/// <summary>
		/// Returns true if there is content that should be "multipart/related"
		/// </summary>
		/// <returns>true if the content is multipart/related.</returns>
		private bool HasRelatedContent()
		{
			return (_relatedfileattachments.Count > 0);
		}
		#endregion

		#region ToDataStringHeaders
		/// <summary>
		/// Encode the headers as they will appear in the email
		/// </summary>
		/// <param name="charset">the charset encoding for the string</param>
		/// <param name="encoder">the mime encoding</param>
		/// <returns>the encoded string</returns>
		internal String ToDataStringHeaders(System.Text.Encoding charset, IEncoder encoder) 
		{
			StringBuilder sb=new StringBuilder();
			MessageHeaderCollection standardheaders=GetStandardHeaders(charset, encoder);			
			sb.Append(standardheaders.ToDataString());
			
			if (_custommessageheaders.Count > 0) 
			{
				sb.Append(_custommessageheaders.ToDataString());
			}

			return sb.ToString();
		}
		#endregion

		#region HasBodyTextOnly
		private bool HasBodyTextOnly()
		{
			return (_textpart==null 
				&& _htmlpart==null 
				&& !HasMixedContent()
				&& !HasRelatedContent());
		}
		#endregion

		#region ToDataStringBody
		/// <summary>
		/// Encode the email body as it will appear in the email
		/// </summary>
		/// <returns>the encoded body</returns>
		internal String ToDataStringBody() 
		{
			bool mixedistop=false;
			bool relatedistop=false;
			bool alternativeistop=false;

			if (HasBodyTextOnly()) 
			{
				return FormatBodyText(_bodytext);		
			}

			MimeBoundary relatedcontentmimeboundary=null;
			MimeBoundary mixedcontentmimeboundary=null;
			MimeBoundary altertnativecontentmimeboundary=null;

			#region Determine which uses the top-level boundary
			if (HasMixedContent()) 
			{
				mixedistop=true;
				mixedcontentmimeboundary=_mimeboundary;
				relatedcontentmimeboundary=new MimeBoundary();
				altertnativecontentmimeboundary=new MimeBoundary();
			} 
			else if (HasRelatedContent())
			{
				relatedistop=true;
				relatedcontentmimeboundary=_mimeboundary;
				altertnativecontentmimeboundary=new MimeBoundary();				
			} 
			else 
			{
				alternativeistop=true;
				altertnativecontentmimeboundary=_mimeboundary;
			}
			#endregion

			MimeContainer alternativemimecontainer=new MimeContainer(altertnativecontentmimeboundary, "multipart/alternative", alternativeistop);
			MimeContainer topcontainer=alternativemimecontainer;

			#region Text & HTML Parts
			if (_textpart!=null) 
			{
				alternativemimecontainer.AddAttachment(_textpart);
			}

			if (_htmlpart!=null) 
			{
				alternativemimecontainer.AddAttachment(_htmlpart);
			}
			#endregion

			#region Related Content
			if (HasRelatedContent()) 
			{
				//topcontainer=new MimeContainer(relatedcontentmimeboundary, "multipart/related", relatedistop);
				topcontainer=new MimeContainer(relatedcontentmimeboundary, "multipart/related;\r\n       type=\"multipart/alternative\"", relatedistop);
				topcontainer.AddMimeContainer(alternativemimecontainer);
				foreach (FileAttachment attachment in _relatedfileattachments) 
				{
					topcontainer.AddAttachment(attachment);
				}
			}
			#endregion
			
			#region Mixed Content
			if (HasMixedContent()) 
			{
	
				MimeContainer oldtopcontainer=topcontainer;
				topcontainer=new MimeContainer(mixedcontentmimeboundary, "multipart/mixed", mixedistop);
				topcontainer.AddMimeContainer(oldtopcontainer);

				foreach (FileAttachment attachment in _mixedfileattachments) 
				{
					topcontainer.AddAttachment(attachment);
				}

			}
			#endregion

			if (_bodytext==null) 
			{
				return topcontainer.ToDataString();
			} 
			else 
			{
				return FormatBodyText(_bodytext)+".\r\n\r\n"
					+topcontainer.ToDataString();
			}
		
		}
		#endregion

		#region FormatBodyText
		/// <summary>
		/// format the body text---not implemented yet.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		private String FormatBodyText(String text) 
		{
			return text;
		}
		#endregion

		#region GetNoMimeMessage
		private String GetNoMimeMessage() 
		{			
			return ARM.GetInstance().GetString("this_is_mime");
		}
		#endregion

		/// <summary>
		/// The text that occurs within the body.  By default,
		/// it is a MIME notice, but it can be overridden here.
		/// NOTE: This is only useful for 7 bit us-ascii currently.
		/// </summary>
		public String BodyText 
		{
			get { return _bodytext;}
			set { _bodytext=value;}
		}

		#region ToDataString
		/// <summary>
		/// Render the encoded message for smtp "DATA" transmission.
		/// This is the final version that will get sent to the 
		/// </summary>
		/// <returns></returns>
		public String ToDataString() 
		{	
			IEncoder encoder=EncoderFactory.GetEncoder(HeaderEncoding);
			return ToDataStringHeaders(this.HeaderCharSet, encoder)+SmtpProxy.ENDOFLINE+ToDataStringBody();

		}
		#endregion

		#region Send
		/// <summary>
		/// Send out the email via the smtp server given.  If 
		/// the SMTP server throws an error, an SmtpException will
		/// be thrown.  All other exceptions will be MailExceptions
		/// </summary>
		/// <param name="smtpserver">The outgoing SMTP server.</param>
		/// <returns>true if sent successfully.  (note that this
		/// will not currently return false, but in the future
		/// a false value may be used)</returns>
		public bool Send(SmtpServer smtpserver) 
		{
			//SmtpProxy smtpproxy=smtpserver.GetSmtpProxy();
			if(_fromaddress == null)
			{
				throw new MailException(ARM.GetInstance().GetString("error_no_from"));
			}

			EmailAddress envelopefrom=_envelopefromaddress;
			if (envelopefrom==null) 
			{
				envelopefrom=_fromaddress;
			}
			EmailAddressCollection allrecipients=new EmailAddressCollection();
			allrecipients.AddCollection(_toaddresses);
			allrecipients.AddCollection(_ccaddresses);
			allrecipients.AddCollection(_bccaddresses);
			return smtpserver.Send(this, allrecipients, envelopefrom);

		}
		#endregion

		#region ContentType
		/// <summary>
		/// Set the content type string in the mime header
		/// </summary>
		public String ContentType 
		{
			get {return _contenttype;}
			set {_contenttype=value;}
		}
		#endregion

		#region TextPart
		/// <summary>
		/// Set the plain text part of the email.  This is optional
		/// </summary>
		public TextAttachment TextPart 
		{
			set {_textpart=value;}
			get {return _textpart;}
		}
		#endregion

		#region HtmlPart
		/// <summary>
		/// Set the html part of the email.  This is optional
		/// </summary>
		public HtmlAttachment HtmlPart 
		{
			set {_htmlpart=value;}
			get {return _htmlpart;}
		}
		#endregion

		#region AddMixedAttachment
		/// <summary>
		/// Add an attachment which will appear to the user as
		/// a separate file.  (It is not referred to in the email itself.)
		/// </summary>
		/// <param name="fileattachment">The file attachment</param>
		public void AddMixedAttachment(FileAttachment fileattachment) 
		{
			_mixedfileattachments.Add(fileattachment);
		}
		#endregion

		#region AddRelatedAttachment
		/// <summary>
		/// Add an image which is referred to from another part of
		/// the email (probably the HTML attachment).  You should
		/// set the ContentID of the file attachment before passing
		/// it in.
		/// </summary>
		/// <param name="fileattachment">The file attachment</param>
		public void AddRelatedAttachment(FileAttachment fileattachment) 
		{	
			_relatedfileattachments.Add(fileattachment);
		}
		#endregion
	}
}


