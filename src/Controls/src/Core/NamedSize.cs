#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Represents pre-defined font sizes.</summary>
	[Obsolete]
	public enum NamedSize
	{
		/// <summary>The default font size.</summary>
		Default = 0,
		/// <summary>The smallest readable font size for the device. Think about this like legal footnotes.</summary>
		Micro = 1,
		/// <summary>A small but readable font size. Use this for block of text.</summary>
		Small = 2,
		/// <summary>A default font size, to be used in stand alone labels or buttons.</summary>
		Medium = 3,
		/// <summary>A Large font size, for titles or other important text elements.</summary>
		Large = 4,
		/// <summary>Body.</summary>
		Body = 5,
		/// <summary>Header.</summary>
		Header = 6,
		/// <summary>Title.</summary>
		Title = 7,
		/// <summary>Subtitle.</summary>
		Subtitle = 8,
		/// <summary>Caption.</summary>
		Caption = 9
	}
}