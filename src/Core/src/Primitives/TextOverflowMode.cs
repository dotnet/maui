namespace Microsoft.Maui
{
	public enum TextOverflowMode 
	{
		None,			// Text just keeps laying out
		Truncate,		// Text is cut off
		EllipsizeEnd,	// Text is cut off at the end with ...
		EllipsizeStart,	// Text is cut off at the start with ...
		EllipsizeMiddle	// Text is spliced in the middle with ...
	}
}