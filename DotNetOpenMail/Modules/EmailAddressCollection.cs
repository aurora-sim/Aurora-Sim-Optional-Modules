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

using DotNetOpenMail.Encoding;

namespace DotNetOpenMail
{
	/// <summary>
	/// A collection of EmailAddress objects.  Implements
	/// CollectionBase
	/// </summary>
	public class EmailAddressCollection : CollectionBase
	{
		/// <summary>
		/// Create a new instance of the EmailAddressCollection
		/// </summary>
		public EmailAddressCollection()
		{
		}

		#region Add
		/// <summary>
		/// Add an object to the collection
		/// </summary>
		/// <param name="emailaddress">The EmailAddress object to add</param>
		/// <returns></returns>
		public int Add( EmailAddress emailaddress )  
		{
			return( List.Add( emailaddress ) );
		}
		#endregion

		#region Add
		/// <summary>
		/// Append a collection of objects to the existing collection
		/// </summary>
		/// <param name="emailaddresses">The EmailAddressCollection to append to the existing collection.</param>
		public void AddCollection( EmailAddressCollection emailaddresses )  
		{
			foreach (EmailAddress emailaddress in emailaddresses) 
			{
				List.Add(emailaddress);
			}
		}
		#endregion

		#region IndexOf
		/// <summary>
		/// Find the index of the EmailAddress in the collection
		/// </summary>
		/// <param name="value">The object to find.</param>
		/// <returns>The index, from 0, or -1 if not found.</returns>
		public int IndexOf( EmailAddress value )  
		{
			return( List.IndexOf( value ) );
		}
		#endregion

		#region Insert
		/// <summary>
		/// Insert an EmailAddress into the collection at the
		/// specified index.
		/// </summary>
		/// <param name="index">The index, starting at zero.</param>
		/// <param name="emailaddress">The email address to add to the collection</param>
		public void Insert( int index, EmailAddress emailaddress )  
		{
			List.Insert( index, emailaddress );
		}
		#endregion

		#region Remove
		/// <summary>
		/// Remove an address, if found, from the collection.
		/// </summary>
		/// <param name="emailaddress">The EmailAddress to remove</param>
		public void Remove( EmailAddress emailaddress )  
		{
			List.Remove( emailaddress );
		}
		#endregion

		#region ToDataString
		/// <summary>
		/// Render the email addresses in a comma-separated list,
		/// suitable for being rendered in an email.
		/// </summary>
		/// <returns></returns>
		public String ToDataString() 
		{
			StringBuilder sb=new StringBuilder();
			bool doneone=false;
			foreach (EmailAddress emailaddress in List) 
			{
				if (!doneone) 
				{
					doneone=true;
				} 
				else 
				{
					sb.Append(", ");
				}
				sb.Append(emailaddress.ToDataString());
			}
			return sb.ToString();
		}
		#endregion

		#region ToString
		/// <summary>
		/// Calls ToDataString().
		/// </summary>
		/// <returns></returns>
		public override String ToString() 
		{
			return ToDataString();
		}
		#endregion


	}
}
