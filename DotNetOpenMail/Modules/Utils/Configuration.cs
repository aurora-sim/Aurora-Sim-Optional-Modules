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
	/// An interface to the configuration settings for the library.
	/// This includes an interface to the .config file.
	/// This is a singleton.
	/// </summary>
	public class Configuration
	{
		/// <summary>
		/// The singleton
		/// </summary>
		private static Configuration _theinstance=new Configuration();

		/// <summary>
		/// Empty constructor
		/// </summary>
		private Configuration()
		{
			
		}
		
		/// <summary>
		/// Instantiate the instance of the object (if not created)
		/// and return it.
		/// </summary>
		/// <returns>the Configuration singleton object</returns>
		public static Configuration GetInstance() 
		{
			return _theinstance;
		}

		/// <summary>
		/// Get the X-Sender header, if any is specified in the .config
		/// file with the key dnom.headers.xsender.
		/// </summary>
		/// <returns></returns>
		public String GetXSender() 
		{
			return System.Configuration.ConfigurationSettings.AppSettings["dnom.headers.xsender"];
		}

		/// <summary>
		/// Get the default encoding, if any is specified in the .config
		/// file with the key dnom.encoding.
		/// </summary>
		public String GetDefaultEncoding(String def) 
		{
			return GetAppSetting("dnom.encoding", def);			
		}

		/// <summary>
		/// Get the default charset, if any is specified in the .config
		/// file with the key dnom.charset.
		/// </summary>
		public System.Text.Encoding GetDefaultCharset() 
		{
			String encodingstr=GetAppSetting("dnom.encoding", "iso-8859-1");
			return System.Text.Encoding.GetEncoding(encodingstr);
		}

		/// <summary>
		/// Get an app setting from the .config file.  If none is found
		/// or it is empty or only whitespace, use the defaultvalue.
		/// </summary>
		/// <param name="key">The key to search for in the .config file</param>
		/// <param name="defaultvalue">The default value if the value
		/// is null or only whitespace.</param>
		private String GetAppSetting(String key, String defaultvalue) 
		{
			String tmp=System.Configuration.ConfigurationSettings.AppSettings[key];
			if (tmp==null || tmp.Trim()=="") 
			{
				return defaultvalue;
			} 
			else 
			{
				return tmp;
			}
		}
	}
}
