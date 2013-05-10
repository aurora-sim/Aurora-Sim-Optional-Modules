using System;
using System.Collections;
using DotNetOpenMail.SmtpAuth;

namespace DotNetOpenMail
{
	/// <summary>
	/// The server response from EHLO
	/// </summary>
	public class EhloSmtpResponse
	{
		private int _responseCode=0;
		private String _message;

		ArrayList smtpAuthTypes=new ArrayList();

		/// <summary>
		/// Create an instance of the EhloSmtpResponse class
		/// </summary>
		public EhloSmtpResponse()
		{
		}

		/// <summary>
		/// Get the available Auth Types strings
		/// as reported by the server.
		/// (e.g. "login", etc.)
		/// </summary>
		/// <returns></returns>
		public String[] GetAvailableAuthTypes() 
		{
			String[] stringArray=new String[smtpAuthTypes.Count];
			smtpAuthTypes.CopyTo(stringArray);
			
			return stringArray;
			//return (SmtpAuthType[]) smtpAuthTypes.ToArray(typeof (SmtpAuthType[]));
		}

		/// <summary>
		/// Add an Auth type (by string as contained in the
		/// EHLO authentication.  These are converted to lower
		/// case.
		/// </summary>
		/// <param name="authType">The auth type from EHLO</param>
		public void AddAvailableAuthType(String authType) 
		{
			smtpAuthTypes.Add(authType.Trim().ToLower());
		}

		/*
		/// <summary>
		/// Add an Auth type 
		/// </summary>
		/// <param name="smtpAuthType"></param>
		public void AddAvailableAuthType(SmtpAuthType smtpAuthType) 
		{
			smtpAuthTypes.Add(smtpAuthType);
		}
		*/
		
		/// <summary>
		/// The SMTP Reponse code
		/// </summary>
		public int ResponseCode
		{
			get { return _responseCode; }
			set { _responseCode=value; }
		}

		/// <summary>
		/// The SMTP Message.  This will be "OK" if successful,
		/// otherwise it will be the part that failed.
		/// </summary>
		public String Message
		{
			get { return _message; }
			set { _message=value; }
		}
	}
}
