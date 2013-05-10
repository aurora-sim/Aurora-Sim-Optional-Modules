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
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace DotNetOpenMail.Resources
{
	/// <summary>
	/// ARM= Assembly Resource Manager.  Manage the strings for the
	/// various CultureInfo types.
	/// </summary>
	public sealed class ARM
	{
		/// <summary>
		/// The singleton object
		/// </summary>
		private static ARM _theinstance=new ARM();
		/// <summary>
		/// The namespace of the resource files
		/// </summary>
		private string _pathname="DotNetOpenMail.Resources.DotNetOpenMail";
		/// <summary>
		/// The singleton System.Resources.ResourceManager.
		/// </summary>
		private ResourceManager _resources=null;

		/// <summary>
		/// Create the singleton
		/// </summary>
		private ARM()
		{
			_resources=new ResourceManager(_pathname, typeof(ARM).Assembly);
		}

		/// <summary>
		/// Get the instance of the class
		/// </summary>
		/// <returns>the ARM instance</returns>
		public static ARM GetInstance() 
		{
			return _theinstance;
		}

		/// <summary>
		/// Get an object from the file
		/// </summary>
		/// <param name="name">The key</param>
		/// <returns>The key lookup for the default culture</returns>
		public object GetObject(string name) 
		{
			return GetObject(null, name);
		}

		/// <summary>
		/// Get an object for the specified CultureInfo
		/// </summary>
		/// <param name="culture">The specified culture for the resource</param>
		/// <param name="name">The key for the resource</param>
		/// <returns>returns an object, if one is found, else null</returns>
		public object GetObject(CultureInfo culture, string name) 
		{
			if (_resources != null) 
			{
				return _resources.GetObject(name, culture);
			}
			return null;
		}

		/// <summary>
		/// Get a string for the specified CultureInfo for the
		/// default culture
		/// </summary>
		/// <param name="name">The key for the resource</param>
		/// <returns>returns a string, if one is found, else null</returns>
		public string GetString(string name) 
		{
			return GetString(null, name, null);
		}

		/// <summary>
		/// Get a string for the specified CultureInfo for the
		/// default culture.  If not found, return defaultvalue
		/// </summary>
		/// <param name="name">The key for the resource</param>
		/// <param name="defaultvalue">The value to return if there 
		/// is no value found.</param>
		/// <returns>returns a string, if one is found, else defaultvalue</returns>
		public string GetString(string name, string defaultvalue) 
		{
			return GetString(null, name, defaultvalue);
		}

		/// <summary>
		/// Get a string for the specified CultureInfo for the
		/// current culture.  If not found, return null
		/// </summary>
		/// <param name="culture">The culture of the resulting string</param>
		/// <param name="name">The key for the resource</param>
		/// <returns>returns a string, if one is found, else 
		/// null</returns>
		public string GetString(CultureInfo culture, string name) 
		{
			return GetString(culture, name, null);
		}

		/// <summary>
		/// Get a string for the specified CultureInfo for the
		/// default culture.  If not found, return defaultvalue
		/// </summary>
		/// <param name="culture">The culture of the resulting string</param>
		/// <param name="name">The key for the resource</param>
		/// <param name="defaultvalue">The value to return if there 
		/// is no value found.</param>
		/// <returns>returns a string, if one is found, else defaultvalue</returns>
		public string GetString(CultureInfo culture, string name, string defaultvalue) 
		{
			if (_resources != null) 
			{
				String result=_resources.GetString(name, culture);
				if (result==null) 
				{
					return defaultvalue;
				}
				return result;
			}
			throw new ApplicationException("The Resources are uninitialized");
		}

		/// <summary>
		/// Retrieve the entire resource set for this culture.
		/// </summary>
		/// <param name="culture">The culture to examine</param>
		/// <returns>The resource set, if any.</returns>
		public ResourceSet GetResourceSet(CultureInfo culture) 
		{
			return _resources.GetResourceSet(culture, false, false);
		}

	}
}

