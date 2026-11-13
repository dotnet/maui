namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Implemented by scroll view handlers whose platform reduces the scrollable viewport
	/// below the view's frame — on iOS/MacCatalyst the adjusted content insets obscure part
	/// of the frame, so the visible viewport is smaller and (0,0) in cross-platform scroll
	/// coordinates is the inset rest position.
	/// </summary>
	/// <remarks>
	/// The handler owns that coordinate convention; this is how the cross-platform layer and
	/// the platform view consume it instead of re-deriving inset knowledge of their own.
	/// Handlers on platforms where the viewport always equals the frame do not implement it.
	/// </remarks>
	internal interface IScrollViewportProvider
	{
		/// <summary>
		/// The total insets obscuring the scrollable viewport, in cross-platform units:
		/// the platform-applied content insets plus any safe area the platform view baked
		/// into the content itself (see <see cref="ContentCoordinateInsets"/>).
		/// </summary>
		Thickness ViewportInsets { get; }

		/// <summary>
		/// The portion of <see cref="ViewportInsets"/> the platform view baked into the
		/// content as padding instead of applying it as a platform inset. Element positions
		/// read from the content tree already include it, so element-relative targets must
		/// shift by it; platform insets instead sit outside the content coordinate space and
		/// are compensated when the request is translated to a native offset.
		/// </summary>
		Thickness ContentCoordinateInsets { get; }

		/// <summary>
		/// Notifies the handler that the platform view's insets changed. The reported scroll
		/// offsets are derived from them, and insets can change without the offset moving —
		/// which produces no scroll notification of its own.
		/// </summary>
		void NotifyInsetsChanged();
	}
}
