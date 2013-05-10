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
	/// Factory for instantiating the right encoder class,
	/// given an encoding
	/// </summary>
	internal class EncoderFactory
	{

		/// <summary>
		/// Empty Constructor.
		/// </summary>
		internal EncoderFactory()
		{
		}

		/// <summary>
		/// Method for determining the correct encoder for this
		/// encoding type.
		/// </summary>
		/// <param name="encodingtype">Enumerated encoding type</param>
		/// <returns>Returns the proper IEncoder implementation</returns>
		/// <exception cref="ApplicationException">Throws an ApplicationException if the specified encoding is unknown.</exception>
		public static IEncoder GetEncoder(EncodingType encodingtype) 
		{
			if (encodingtype==EncodingType.Base64) 
			{
				return Base64Encoder.GetInstance();
			} 
			else if (encodingtype==EncodingType.QuotedPrintable) 
			{
				return QPEncoder.GetInstance();
			} 
			else if (encodingtype==EncodingType.SevenBit) 
			{
				return SevenBitEncoder.GetInstance();
			} 
			else if (encodingtype==EncodingType.EightBit) 
			{
				return EightBitEncoder.GetInstance();
			} 
			else 
			{
				throw new ApplicationException("This encoder type is not implemented");
			}
		}
		
	}
}
