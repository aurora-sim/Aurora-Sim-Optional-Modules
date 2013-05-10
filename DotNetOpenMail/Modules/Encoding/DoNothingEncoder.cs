using System;
using System.Text;
using System.IO;

namespace DotNetOpenMail.Encoding
{
	/// <summary>
	/// An encoder that does nothing.  (This is essentially
	/// what the 7bit and 8bit encodings are; they are just
	/// markers, not actual encodings)
	/// </summary>
	public abstract class DoNothingEncoder : IEncoder
	{
		/// <summary>
		/// Empty constructor
		/// </summary>
		public DoNothingEncoder()
		{
		}

		#region Encode
		/// <summary>
		/// Echo the text back unchanged.
		/// </summary>
		/// <param name="stringreader">Reader for the incoming string</param>
		/// <param name="stringwriter">Writer for the outgoing string</param>
		/// <param name="encoding">The encodigng for the outgoing string (ignored)</param>
		public void Encode(StringReader stringreader, StringWriter stringwriter, System.Text.Encoding encoding) 
		{
			try
			{
				stringwriter.Write(stringreader.ReadToEnd());
			}
			catch(Exception e)
			{
				throw new Exception("Error writing during Encoding" + e.Message);
			}

		}
		#endregion

		#region Encode
		/// <summary>
		/// Echo the text back unchanged
		/// </summary>
		/// <param name="filestream">The incoming filestream</param>
		/// <param name="stringwriter">The outgoing filestream</param>
		/// <param name="charset">Charset (ignored)</param>
		public void Encode(FileStream filestream, StringWriter stringwriter, System.Text.Encoding charset) 
		{
			byte[] buffer = new byte[filestream.Length];
			filestream.Read(buffer, 0, buffer.Length);
			filestream.Close();	
			stringwriter.Write(buffer);
		}
		#endregion

		#region Encode
		/// <summary>
		/// Echo the text back unchanged (good for 7bit text only).
		/// </summary>
		/// <param name="binaryreader">The incoming binaryreader</param>
		/// <param name="stringwriter">The outgoing stream</param>
		public void Encode(BinaryReader binaryreader, StringWriter stringwriter) 
		{
			throw new Exception("Binary reader doesn't do anything for this reader");
			//stringwriter.Write(binaryreader.ReadString());
		}
		#endregion

		/// <summary>
		/// Do nothing to the header string
		/// </summary>
		/// <param name="name">The header key</param>
		/// <param name="val">The header value</param>
		/// <param name="charset">The charset to encode in (ignored)</param>
		/// <param name="forceencoding">(ignored)</param>
		/// <returns></returns>
		public String EncodeHeaderString(String name, String val, System.Text.Encoding charset, bool forceencoding) 
		{
			return val;
		}

		#region ContentTransferEncodingString
		/// <summary>
		/// The String that goes in the content transfer encoding header
		/// </summary>
		public virtual String ContentTransferEncodingString
		{
			get {throw new ApplicationException("Implement this");}
		}
		#endregion


	}
}
