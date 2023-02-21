namespace Microsoft.Maui
{
	/// <summary>
	/// Enumeration specifying various options for line breaking.
	/// </summary>
	/// <remarks>How lines are broken or text is truncated might be different depending on the platform.</remarks>
	public enum LineBreakMode
	{
		/// <summary>
		/// Do not wrap text.
		/// </summary>
		NoWrap,

		/// <summary>
		/// Wrap at word boundaries.
		/// </summary>
		WordWrap,

		/// <summary>
		/// Wrap at character boundaries.
		/// </summary>
		CharacterWrap,

		/// <summary>
		/// Truncate the head of text.
		/// </summary>
		HeadTruncation,

		/// <summary>
		/// Truncate the tail of text.
		/// </summary>
		TailTruncation,

		/// <summary>
		/// Truncate the middle of text. This may be done, for example, by replacing it with an ellipsis.
		/// </summary>
		MiddleTruncation
	}
}