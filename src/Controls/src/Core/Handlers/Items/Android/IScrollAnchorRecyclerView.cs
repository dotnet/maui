namespace Microsoft.Maui.Controls.Handlers.Items
{
	/// <summary>A recycler view that can keep an edge of its content in view (<see cref="ScrollAnchorMode"/>).</summary>
	interface IScrollAnchorRecyclerView
	{
		/// <summary>Starts or stops anchoring, for the items view's current <see cref="StructuredItemsView.AnchorMode"/>.</summary>
		void UpdateAnchorMode();

		/// <summary>
		/// Stops a scroll the app requested (<c>ScrollTo</c>, <c>ScrollToStart</c> or <c>ScrollToEnd</c>): what one left for
		/// the next layout, and an animated one, also while RecyclerView settles the rest of the way. The smooth scroller that
		/// animates a request stops once it finds its target, and leaves the rest of the way to RecyclerView, which settles it
		/// as it settles a fling. A fling of the user's carries on.
		/// </summary>
		void StopRequestedScroll();

		/// <summary>
		/// Starts an animated scroll to the start or the end of the content once the view is laid out, as an animated
		/// <c>ScrollToStart</c> or <c>ScrollToEnd</c> does: a jump an earlier request left for that layout lands first, and a
		/// newer request replaces it.
		/// </summary>
		/// <param name="end">Whether to scroll to the end, rather than the start.</param>
		void AnimateScrollToEdgeOnceLaidOut(bool end);

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
