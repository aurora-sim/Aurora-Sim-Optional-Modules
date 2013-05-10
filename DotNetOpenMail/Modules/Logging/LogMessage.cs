using System;

namespace DotNetOpenMail.Logging
{
	/// <summary>
	/// Summary description for LogMessage.
	/// </summary>
	public class LogMessage
	{
		private Object _sender=null;
		private String _message=null;

		/// <summary>
		/// The log message
		/// </summary>
		public String Message 
		{
			get {return _message;}
			set {_message=value;}
		}

		/// <summary>
		/// The sender object
		/// </summary>
		public Object Sender 
		{
			get {return _sender;}
			set {_sender=value;}
		}


		/// <summary>
		/// Log a message
		/// </summary>
		/// <param name="sender">The object logging the message</param>
		/// <param name="message">The message to be logged</param>
		public LogMessage(Object sender, String message)
		{
			this._sender=sender;
			this._message=message;
		}

	}
}
