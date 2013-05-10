using System;
using System.IO;

namespace DotNetOpenMail.Utils
{
	/// <summary>
	/// Something take a binary reader's data into a byte array.
	/// </summary>
	public class BinaryReaderUtil
	{
		/// <summary>
		/// Constructor for BinaryReaderUtil
		/// </summary>
		public BinaryReaderUtil()
		{
		}

		/// <summary>
		/// Read a binary stream into a byte array
		/// (Cribbed from posting by Jon Skeet:
		/// http://www.developerfusion.co.uk/show/4696/)
		/// </summary>
		/// <param name="binaryreader">An open Binary Reader</param>
		/// <returns>A byte array with the bytes from the disk</returns>
		public static byte[] ReadIntoByteArray (BinaryReader binaryreader)
		{
			int initialLength = 32768;

			byte[] buffer = new byte[initialLength];
			int read=0;
   
			int chunk;
			while ( (chunk = binaryreader.Read(buffer, read, buffer.Length-read)) > 0)
			{
				read += chunk;
				if (read == buffer.Length)
				{					
					int nextByte = binaryreader.ReadByte();
           
					if (nextByte==-1)
					{
						return buffer;
					}

					byte[] newBuffer = new byte[buffer.Length*2];
					Array.Copy(buffer, newBuffer, buffer.Length);
					newBuffer[read]=(byte)nextByte;
					buffer = newBuffer;
					read++;
				}
			}
			byte[] ret = new byte[read];
			Array.Copy(buffer, ret, read);
			return ret;
		}

	}
}
