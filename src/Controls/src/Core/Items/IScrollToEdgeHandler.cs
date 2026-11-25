namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A handler that scrolls its platform view to an edge of its content, for <see cref="StructuredItemsView.ScrollToStart(bool)"/>
	/// and <see cref="StructuredItemsView.ScrollToEnd(bool)"/>.
	/// </summary>
	/// <remarks>
	/// Where the platform view can't scroll to an edge of its content, the handler scrolls to the first or last item it shows,
	/// rather than the one <see cref="ItemsView.ItemsSource"/> gives: a sequence can be read only once, and a group needn't be
	/// a list.
	/// </remarks>
	interface IScrollToEdgeHandler
	{
		/// <summary>Scrolls to the start of the content, and follows it again where the view anchors its start.</summary>
		/// <param name="animate">Whether to animate the scroll.</param>
		/// <returns><see langword="false"/> to scroll to the first item <see cref="ItemsView.ItemsSource"/> gives instead.</returns>
		bool ScrollToStart(bool animate);

		/// <summary>Scrolls to the end of the content, and follows it again where the view anchors its end.</summary>
		/// <param name="animate">Whether to animate the scroll.</param>
		/// <returns><see langword="false"/> to scroll to the last item <see cref="ItemsView.ItemsSource"/> gives instead.</returns>
		bool ScrollToEnd(bool animate);
	}
}
