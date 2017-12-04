namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// Enumerates values that describe alignemnt of text.
	/// </summary>
	public enum TextAlignment
	{
		/// <summary>
		/// Follow base TextAlignment
		/// </summary>
		None,

		/// <summary>
		/// Aligns horizontal text according to language. Top aligned for vertical text.
		/// </summary>
		Auto,
		/// <summary>
		/// Left and top aligned for horizontal and vertical text, respectively.
		/// </summary>
		Start,
		/// <summary>
		/// Right and bottom aligned for horizontal and  vertical text, respectively.
		/// </summary>
		End,
		/// <summary>
		/// Center-aligned text.
		/// </summary>
		Center,
	}
}
