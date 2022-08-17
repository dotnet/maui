namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a view that can contain a context flyout menu, which is usually represented as a right-click menu.
	/// </summary>
	public interface IContextFlyoutElement
	{
		/// <summary>
		/// Gets the <see cref="ContextFlyout"/> for the view. Menu flyouts, menu flyout subitems, and menu flyout separators can be added to the context flyout.
		/// </summary>
		IFlyout? ContextFlyout { get; }
	}
}
