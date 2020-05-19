namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// Enumerates values that describe options for line braking.
	/// </summary>
	public enum LineBreakMode
	{
		/// <summary>
		/// Follow base LineBreakMode.
		/// </summary>
		None,

		/// <summary>
		/// Do not wrap text.
		/// </summary>
		NoWrap,

		/// <summary>
		/// Wrap at character boundaries.
		/// </summary>
		CharacterWrap,

		/// <summary>
		/// Wrap at word boundaries.
		/// </summary>
		WordWrap,

		/// <summary>
		/// Tries to wrap at word boundaries, and then wrap at a character boundary if the word is too long.
		/// </summary>
		MixedWrap,

		/// <summary>
		/// Truncate the head of text.
		/// </summary>
		HeadTruncation,

		/// <summary>
		/// Truncate the middle of text. This may be done, for example, by replacing it with an ellipsis.
		/// </summary>
		MiddleTruncation,

		/// <summary>
		/// Truncate the tail of text.
		/// </summary>
		TailTruncation,
	}
}
