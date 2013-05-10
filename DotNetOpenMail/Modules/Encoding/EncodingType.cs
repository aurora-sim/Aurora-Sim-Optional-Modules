using System;

namespace DotNetOpenMail.Encoding
{
	/// <summary>
	/// Enumeration of encoding types.
	/// </summary>
	public enum EncodingType
	{
		/// <summary>
		/// The Quoted-Printable encoding
		/// </summary>
		QuotedPrintable,
		/// <summary>
		/// The Base-65 encoding
		/// </summary>
		Base64,
		/// <summary>
		/// The 7Bit encoding marker
		/// </summary>
		SevenBit,
		/// <summary>
		/// The 8Bit encoding marker
		/// </summary>
		EightBit
	}
}
