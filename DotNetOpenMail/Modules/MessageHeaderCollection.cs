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
using System.Collections;

namespace DotNetOpenMail
{
	/// <summary>
	/// A collection of MessageHeader objects
	/// Implements the CollectionBase interface.
	/// </summary>
	public class MessageHeaderCollection : CollectionBase, IEmailStringable
	{
		/// <summary>
		/// Initializes a new instance of the ArrayList class.
		/// </summary>
		public MessageHeaderCollection()
		{
		}

		#region Add
		/// <summary>
		/// Adds an object to the end of the MessageHeaderCollection
		/// </summary>
		/// <param name="MessageHeader">The Object to be added to the end of the MessageHeaderCollection. 
		/// The value can be a null reference (Nothing in Visual Basic). </param>
		/// <returns>The MessageHeaderCollection index at which the value has been added.</returns>
		public int Add( MessageHeader MessageHeader )  
		{
			return( List.Add( MessageHeader ) );
		}
		#endregion

		#region IndexOf
		/// <summary>
		/// Returns the zero-based index of the first occurrence of a
		/// value in the MessageHeaderCollection or in a portion of it.
		/// </summary>
		/// <param name="value">The Object to locate in the MessageHeaderCollection. The value 
		/// can be a null reference (Nothing in Visual Basic). </param>
		/// <returns></returns>
		public int IndexOf( MessageHeader value )  
		{
			return( List.IndexOf( value ) );
		}
		#endregion

		#region Insert
		/// <summary>
		/// Inserts an element into the MessageHeaderCollection at 
		/// the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which value should be inserted</param>
		/// <param name="MessageHeader">The MessageHeader to insert. 
		/// The value can be a null reference (Nothing in Visual Basic). </param>
		public void Insert( int index, MessageHeader MessageHeader )  
		{
			List.Insert( index, MessageHeader );
		}
		#endregion

		#region Remove
		/// <summary>
		/// Removes the first occurrence of a 
		/// specific MessageHeader from the MessageHeaderCollection.
		/// </summary>
		/// <param name="MessageHeader">The object to remove from the MessageHeaderCollection</param>
		public void Remove( MessageHeader MessageHeader )  
		{
			List.Remove( MessageHeader );
		}
		#endregion

		#region ToDataString
		/// <summary>
		/// Write the MessageHeaderCollection to a string that can be 
		/// included in an email.
		/// </summary>
		/// <returns>The String as it will appear in the resulting email.</returns>
		public String ToDataString() 
		{
			StringBuilder sb=new StringBuilder("");			
			foreach (MessageHeader messageheader in List) 
			{
				sb.Append(messageheader.ToDataString());
			}
			return sb.ToString();
		}
		#endregion

	}
}
