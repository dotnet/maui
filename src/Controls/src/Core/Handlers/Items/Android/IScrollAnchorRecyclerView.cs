namespace Microsoft.Maui.Controls.Handlers.Items
{
	/// <summary>A recycler view that can keep the end of its content in view (<see cref="ScrollAnchorMode.End"/>).</summary>
	interface IScrollAnchorRecyclerView
	{
		/// <summary>Starts or stops anchoring, for the items view's current <see cref="StructuredItemsView.AnchorMode"/>.</summary>
		void UpdateAnchorMode();

		/// <summary>Scrolls to the end of the content, and follows it again while anchoring.</summary>
		/// <param name="animate">Whether to animate the scroll.</param>
		/// <returns><see langword="false"/> when the layout doesn't scroll vertically.</returns>
		bool ScrollToEnd(bool animate);
	}
}
