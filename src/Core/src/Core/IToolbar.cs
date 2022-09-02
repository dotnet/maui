namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a bar that may display the page title, navigation affordances, and other interactive items.
	/// </summary>
	public interface IToolbar : IElement
	{
		/// <summary>
		/// Gets or sets a value that indicates whether the back button is enabled or disabled.
		/// </summary>
		bool BackButtonVisible { get; set; }

		/// <summary>
		///  Gets or sets a value that indicates whether the toolbar is visible or not.
		/// </summary>
		bool IsVisible { get; set; }

		/// <summary>
		/// Gets the title for the Toolbar.
		/// </summary>
		string Title { get; }
	}
}
