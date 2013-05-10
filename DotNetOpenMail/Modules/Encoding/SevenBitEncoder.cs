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

namespace DotNetOpenMail.Encoding
{
	/// <summary>
	/// Denotes 7bit encoding.  Note that this doesn't really
	/// encode anything; it just specifies that this email is already
	/// in 7-bit ascii encoding.
	/// </summary>
	public class SevenBitEncoder : DoNothingEncoder
	{
		/// <summary>
		/// Empty constructor
		/// </summary>
		private SevenBitEncoder()
		{
		}

		/// <summary>
		/// Create an instance of this class.
		/// </summary>
		/// <returns></returns>
		public static SevenBitEncoder GetInstance() 
		{
			return new SevenBitEncoder();
		}


		#region ContentTransferEncodingString
		/// <summary>
		/// The String that goes in the content transfer encoding header
		/// </summary>
		public override String ContentTransferEncodingString
		{
			get {return "7bit";}
		}
		#endregion


	}
}
