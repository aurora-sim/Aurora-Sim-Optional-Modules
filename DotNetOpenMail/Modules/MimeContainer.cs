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
using System.Collections;

namespace DotNetOpenMail
{
	/// <summary>
	/// A MIME container for content or other mime containers.
	/// </summary>
	internal class MimeContainer : IEmailStringable
	{
		private MimeBoundary _mimeboundary=null;
		private ArrayList _attachments=new ArrayList();
		private ArrayList _mimecontainers=new ArrayList();
		private String _contenttype=null;
		private bool _istoplevel=true;

		public MimeContainer(MimeBoundary mimeboundary, String contenttype, bool istoplevel)
		{
			this._mimeboundary=mimeboundary;
			this._contenttype=contenttype;
			this._istoplevel=istoplevel;
		}

		public void AddAttachment(AbstractEmailAttachment attachment) 
		{	
			_attachments.Add(attachment);
		}

		public void AddMimeContainer(MimeContainer mimecontainer) 
		{
			_mimecontainers.Add(mimecontainer);
		}

		private void WriteHeader(StringBuilder sb) 
		{
			sb.Append("Content-Type: "+_contenttype+";"+SmtpProxy.ENDOFLINE);
			sb.Append("       boundary=\""+_mimeboundary.BoundaryString+"\""+SmtpProxy.ENDOFLINE+SmtpProxy.ENDOFLINE);
			//sb.Append(_mimeboundary.BoundaryStringStart+SmtpProxy.ENDOFLINE);

		}

		public String ToDataString() 
		{
			StringBuilder sb=new StringBuilder();
			bool hasheader=false;
			if (!_istoplevel) 
			{
				WriteHeader(sb);
			}
			foreach (MimeContainer mimecontainer in _mimecontainers) 
			{
				hasheader=true;
				sb.Append(_mimeboundary.BoundaryStringStart+SmtpProxy.ENDOFLINE);
				sb.Append(mimecontainer.ToDataString());
				//sb.Append(_mimeboundary.BoundaryStringEnd);
			}
			foreach (AbstractEmailAttachment attachment in _attachments) 
			{
				hasheader=true;
				sb.Append(_mimeboundary.BoundaryStringStart+SmtpProxy.ENDOFLINE);
				sb.Append(attachment.ToDataString());
				//sb.Append(_mimeboundary.BoundaryStringEnd);
			}
			if (hasheader) 
			{
				sb.Append(_mimeboundary.BoundaryStringEnd+SmtpProxy.ENDOFLINE+SmtpProxy.ENDOFLINE);
			}
			return sb.ToString();
		}

	}
}
