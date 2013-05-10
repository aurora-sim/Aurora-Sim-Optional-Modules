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

namespace DotNetOpenMail.Utils
{
	/// <summary>
	/// The DotNetOpenMail Version.
	/// </summary>
	public class VersionInfo
	{
		private static VersionInfo _theobject=new VersionInfo();
		private int majorid=0;
		private int minorid=5;
		private int incidentalid=8;
		private String status="b";  // "a" is for alpha, "b" for beta, "rcX" for release candidate,
		//"" for release

		/// <summary>
		/// Empty Constructor
		/// </summary>
		private VersionInfo()
		{
		}

		/// <summary>
		/// Get the singleton of this class
		/// </summary>
		/// <returns></returns>
		public static VersionInfo GetInstance() 
		{
			return new VersionInfo();
		}

		/// <summary>
		/// Get the current version info as a printable string.
		/// This will be of the form A.B.CD, where A is the
		/// majorid, B is the minor id, C is the incidental revision, 
		/// and D is either "a", "b", "rcX" for "alpha", "beta" and
		/// "release candidate X", respectively.  If it is the
		/// release version, D is blank.
		/// </summary>
		/// <returns></returns>
		public override String ToString() 
		{
			return String.Format("{0}.{1}.{2}{3}", majorid, minorid, incidentalid, status);
		}
	}
}
