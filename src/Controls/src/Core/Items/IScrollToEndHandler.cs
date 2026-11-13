namespace Microsoft.Maui.Controls
{
	/// <summary>A handler that scrolls its platform view to the end of its content, for <see cref="StructuredItemsView.ScrollToEnd(bool)"/>.</summary>
	interface IScrollToEndHandler
	{
		/// <summary>Scrolls to the end of the content, and follows it again where the view anchors its end.</summary>
		/// <param name="animate">Whether to animate the scroll.</param>
		/// <returns><see langword="false"/> when the platform view can't scroll to the end of its content, such as in a horizontal layout.</returns>
		bool ScrollToEnd(bool animate);
	}
}
