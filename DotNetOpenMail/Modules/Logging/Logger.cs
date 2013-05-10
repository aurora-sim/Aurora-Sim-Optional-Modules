using System;

namespace DotNetOpenMail.Logging
{
	/// <summary>
	/// Summary description for Logger.
	/// </summary>
	public class Logger
	{
		/// <summary>
		/// Log handler delegate
		/// </summary>
		public delegate void LogHandler(LogMessage logMessage);

		/// <summary>
		/// Log event
		/// </summary>
		//public event LogHandler LogEvent;

		private static Logger _logger=new Logger();

		private Logger()
		{
		}

		/// <summary>
		/// Return the singleton object
		/// </summary>
		/// <returns></returns>
		public static Logger GetInstance() 
		{
			return _logger;
		}

		/// <summary>
		/// Log a message with level "Info"
		/// </summary>
		/// <param name="message"></param>
		public void LogInfo(String message) 
		{
			
		}

		/// <summary>
		/// Log a message with level "Debug"
		/// </summary>
		public void LogDebug(String message) 
		{
			
		}

		/// <summary>
		/// Log a message with level "Error"
		/// </summary>
		public void LogError(String message) 
		{
			
		}

		/// <summary>
		/// Log the output of sending smtp
		/// </summary>
		public void LogWriteSMTP(String message)
		{
		}

		/// <summary>
		/// Log the reply via smtp
		/// </summary>
		/// <param name="message"></param>
		public void LogReceiveSMTP(String message) 
		{
			
		}

	}
}
