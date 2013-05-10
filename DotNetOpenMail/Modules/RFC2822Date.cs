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

namespace DotNetOpenMail
{
	/// <summary>
	/// Create a RFC 2822-compliant date.
	/// 
	/// See section 3.3 in
	/// http://ftp.rfc-editor.org/in-notes/rfc2822.txt
	/// </summary>
	public class RFC2822Date
	{
		private DateTime _datetime;
		private TimeZone _timezone;

		/// <summary>
		/// Create an instance of the RFC 2822 date object.
		/// </summary>
		/// <param name="datetime">The date to convert</param>
		/// <param name="timezone">The timezone to use in the conversion</param>
		public RFC2822Date(DateTime datetime, TimeZone timezone)
		{
			this._datetime=datetime;
			this._timezone=timezone;
		}
		
		/// <summary>
		/// Generate the date string
		/// </summary>
		/// <returns>A RFC-2822 error code</returns>
		public override String ToString() 
		{
			TimeZone current=TimeZone.CurrentTimeZone;
			
			TimeSpan timespan=current.GetUtcOffset(_datetime);
			String tz=String.Format("{0:00}{1:00}", timespan.Hours, Math.Abs(timespan.Minutes %60));
			if (timespan.TotalHours >= 0) 
			{
				tz="+"+tz;
			}
			// Note: fixed for non-English default cultures; thanks, Angel Marin
			return _datetime.ToString("ddd, d MMM yyyy HH:mm:ss "+tz, System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
			
		}

	}
}
