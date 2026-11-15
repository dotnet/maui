namespace Microsoft.Maui.Controls.Handlers.Items
{
	/// <summary>A recycler view that can keep an edge of its content in view (<see cref="ScrollAnchorMode"/>).</summary>
	interface IScrollAnchorRecyclerView
	{
		/// <summary>Starts or stops anchoring, for the items view's current <see cref="StructuredItemsView.AnchorMode"/>.</summary>
		void UpdateAnchorMode();

		/// <summary>
		/// Scrolls to the start of the content, and follows it again while anchoring its start. A layout that doesn't scroll
		/// vertically scrolls to the first item the adapter shows.
		/// </summary>
		/// <param name="animate">Whether to animate the scroll.</param>
		/// <returns><see langword="false"/> when the view has no layout manager yet.</returns>
		bool ScrollToStart(bool animate);

		/// <summary>
		/// Scrolls to the end of the content, and follows it again while anchoring its end. A layout that doesn't scroll
		/// vertically scrolls to the last item the adapter shows.
		/// </summary>
		/// <param name="animate">Whether to animate the scroll.</param>
		/// <returns><see langword="false"/> when the view has no layout manager yet.</returns>
		bool ScrollToEnd(bool animate);
	}
}
