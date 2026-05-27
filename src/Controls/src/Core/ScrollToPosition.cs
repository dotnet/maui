namespace Microsoft.Maui.Controls
{
	/// <summary>Specifies the position to scroll an item to within a list or scroll view.</summary>
	public enum ScrollToPosition
	{
		/// <summary>Scroll the minimum amount to make the item visible.</summary>
		MakeVisible = 0,
		/// <summary>Scroll to position the item at the start of the viewport.</summary>
		Start = 1,
		/// <summary>Scroll to position the item at the center of the viewport.</summary>
		Center = 2,
		/// <summary>Scroll to position the item at the end of the viewport.</summary>
		End = 3
	}
}