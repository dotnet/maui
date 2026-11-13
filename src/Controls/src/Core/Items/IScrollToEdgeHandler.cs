namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A handler that scrolls its platform view to an edge of its content, for <see cref="StructuredItemsView.ScrollToStart(bool)"/>
	/// and <see cref="StructuredItemsView.ScrollToEnd(bool)"/>.
	/// </summary>
	interface IScrollToEdgeHandler
	{
		/// <summary>Scrolls to the start of the content, and follows it again where the view anchors its start.</summary>
		/// <param name="animate">Whether to animate the scroll.</param>
		/// <returns><see langword="false"/> when the platform view can't scroll to the start of its content, such as in a horizontal layout.</returns>
		bool ScrollToStart(bool animate);

		/// <summary>Scrolls to the end of the content, and follows it again where the view anchors its end.</summary>
		/// <param name="animate">Whether to animate the scroll.</param>
		/// <returns><see langword="false"/> when the platform view can't scroll to the end of its content, such as in a horizontal layout.</returns>
		bool ScrollToEnd(bool animate);
	}
}
