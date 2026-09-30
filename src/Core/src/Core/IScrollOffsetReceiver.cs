namespace Microsoft.Maui
{
	/// <summary>
	/// Implemented by scroll views whose reported offsets are derived from platform insets:
	/// when the insets change without any scrolling, the derived offsets move too, and the
	/// handler refreshes them through this path so the update is not mistaken for a scroll.
	/// </summary>
	/// <remarks>
	/// The <see cref="IScrollView.HorizontalOffset"/>/<see cref="IScrollView.VerticalOffset"/>
	/// setters signal "the view scrolled" and views typically raise their scrolled notification
	/// from them; an inset-only change must keep the offset values current without
	/// manufacturing that notification (issue #36801).
	/// </remarks>
	internal interface IScrollOffsetReceiver
	{
		/// <summary>
		/// Updates the reported scroll offsets without treating the change as a scroll.
		/// </summary>
		void UpdateScrollOffsets(double horizontalOffset, double verticalOffset);
	}
}
