namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Specifies the selection mode for selectable items views.
	/// </summary>
	public enum SelectionMode
	{
		/// <summary>
		/// No items can be selected. Selection is disabled.
		/// </summary>
		None,
		
		/// <summary>
		/// Only one item can be selected at a time.
		/// Selecting a new item automatically deselects the previously selected item.
		/// </summary>
		Single,
		
		/// <summary>
		/// Multiple items can be selected simultaneously.
		/// Users can select and deselect items independently.
		/// </summary>
		Multiple
	}
}