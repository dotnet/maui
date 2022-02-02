namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a Page that manages two panes of information: A master page that presents data at a high level, 
	/// and a detail page that displays low-level details about information in the master.
	/// </summary>
	public interface IFlyoutView : IView
	{
		/// <summary>
		/// Gets the flyout view.
		/// </summary>
		IView Flyout { get; }

		/// <summary>
		/// Gets the detail view.
		/// </summary>
		IView Detail { get; }

		/// <summary>
		///  Gets a value that indicates if the flyout is presented.
		/// </summary>
		bool IsPresented { get; set; }

		/// <summary>
		/// Gets a value that indicates how detail content is displayed.
		/// </summary>
		FlyoutBehavior FlyoutBehavior { get; }

		/// <summary>
		///  Gets the flyout view width.
		/// </summary>
		double FlyoutWidth { get; }

		/// <summary>
		///  Gets a value that indicates if the swipe gesture is enabled.
		/// </summary>
		bool IsGestureEnabled { get; }
	}
}
