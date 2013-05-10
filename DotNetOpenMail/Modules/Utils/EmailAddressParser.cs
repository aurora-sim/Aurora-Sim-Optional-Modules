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
using System.Text.RegularExpressions;

namespace DotNetOpenMail.Utils
{
	/// <summary>
	/// Parse a email address string into an EmailAddress.
	/// * This isn't very mature yet! *
	/// </summary>
	public class EmailAddressParser
	{
		private String _lastError=null;

		/// <summary>
		/// Constructor
		/// </summary>
		public EmailAddressParser()
		{
		}

		/// <summary>
		/// If the last conversion was unsuccessful, this
		/// contains the error message.  If it was successful,
		/// it will be null.
		/// </summary>
		public String LastError
		{
			get {return _lastError;}
		}

		/// <summary>
		/// Parse a raw email address into an EmailAddress object.
		/// If it can't be parsed, return null and set LastError
		/// to the error message.
		/// </summary>
		/// <param name="rawemailaddress"></param>
		/// <returns></returns>
		public EmailAddress ParseRawEmailAddress(String rawemailaddress) 
		{
			// see: http://www.twilightsoul.com/Default.aspx?PageContentID=10&tabid=134
			String regex=@"^((?<DisplayName>([\t\x20]*[!#-'\*\+\-/-9=\?A-Z\^-~]+[\t\x20]*|"""+
				@"[\x01-\x09\x0B\x0C\x0E-\x21\x23-\x5B\x5D-\x7F]*"")+)?[\t\x20]*"+
				@"<(?<LocalPart>([\t\x20]*[!#-'\*\+\-/-9=\?A-Z\^-~]+"+
				@"(\.[!#-'\*\+\-/-9=\?A-Z\^-~]+)*|""[\x01-\x09\x0B\x0C\x0E-\x21\x23-\x5B\x5D-\x7F]*""))"+
				@"@(?<Domain>(([a-zA-Z0-9][-a-zA-Z0-9]*[a-zA-Z0-9]\.)+[a-zA-Z]{2,}|"+
				@"\[(([0-9]?[0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\.){3}"+
				@"([0-9]?[0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\]"+
				@"|[a-zA-Z0-9_-]+"+ // added to allow domains w/o dot, e.g. "localhost"
				@"))>[\t\x20]*|"+
				@"(?<LocalPart>([\t\x20]*[!#-'\*\+\-/-9=\?A-Z\^-~]+(\.[!#-'\*\+\"+
				@"-/-9=\?A-Z\^-~]+)*|"+
				@"""[\x01-\x09\x0B\x0C\x0E-\x21\x23-\x5B\x5D-\x7F]*""))"+
				@"@(?<Domain>(([a-zA-Z0-9][-a-zA-Z0-9]*[a-zA-Z0-9]\.)+[a-zA-Z]{2,}|"+
				@"\[(([0-9]?[0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\.){3}"+ // IP line 1
				@"([0-9]?[0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\]"+ // IP line 2				
				@")))$";
			
			Regex re = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture); 
			Match m = re.Match(rawemailaddress);

			String email="";
			String name="";

			if (!m.Success) 
			{
				_lastError="Could not parse the email address: "+rawemailaddress;
				return null;
			} 
			else 
			{
				_lastError=null;
				name=m.Groups["DisplayName"].Value.Trim();
				email=m.Groups["LocalPart"].Value;
				String domain=m.Groups["Domain"].Value;
				if (!domain.Equals("")) 
				{
					email+="@"+domain;
				}
				
			}
			
			return new EmailAddress(email, name);
		}
	}
}
