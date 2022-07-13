namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a view that can contain a context flyout menu, which is usually represented as a right-click menu.
	/// </summary>
	public interface IContextFlyoutContainer
	{
		/// <summary>
		/// Gets the <see cref="IContextFlyout"/> for the view. Menu flyouts, menu flyout subitems, and menu flyout separators can be added to the context flyout.
		/// </summary>
		IContextFlyout? ContextFlyout { get; }
	}
}
