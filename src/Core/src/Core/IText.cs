namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality to be able to customize Text.
	/// </summary>
	public interface IText : ITextStyle
	{
		/// <summary>
		/// Gets the text.
		/// </summary>
		string Text { get; }
	}

	public interface IOverflowableText : IText 
	{
		/// <summary>
		/// Specifies how to handle text which exceeds the available space
		/// </summary>
		TextOverflowMode TextOverflowMode { get; }
	}

	public interface IMultilineText : IText 
	{ 
		/// <summary>
		/// The maximum number of lines which can be displayed by the control. 
		/// </summary>
		/// <remarks>
		/// All values below 1 are treated as "no limit".
		/// </remarks>
		int MaximumLines { get; }

		/// <summary>
		/// Specifies how the control will handle text wrapping
		/// </summary>
		TextWrapMode TextWrapMode { get; }
	}
}