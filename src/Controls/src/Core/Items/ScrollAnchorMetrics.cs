using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Where a scrolling view is along its scrolling axis, in device-independent units.</summary>
	readonly struct ScrollAnchorMetrics
	{
		/// <summary>Initializes a new instance of the <see cref="ScrollAnchorMetrics"/> struct.</summary>
		/// <param name="offset">How far the content is scrolled: 0 shows its start.</param>
		/// <param name="extent">The length of the scrollable content, including any content insets.</param>
		/// <param name="viewport">The length of the visible area.</param>
		public ScrollAnchorMetrics(double offset, double extent, double viewport)
		{
			Offset = offset;
			Extent = extent;
			Viewport = viewport;
		}

		/// <summary>Gets how far the content is scrolled: 0 shows its start.</summary>
		public double Offset { get; }

		/// <summary>Gets the length of the scrollable content, including any content insets.</summary>
		public double Extent { get; }

		/// <summary>Gets the length of the visible area.</summary>
		public double Viewport { get; }

		/// <summary>Gets how far the end of the content is past the end of the viewport: zero or less while it is in view.</summary>
		public double DistanceToEnd => Extent - Offset - Viewport;

		/// <summary>Gets the offset that shows the end of the content.</summary>
		public double EndOffset => Math.Max(0, Extent - Viewport);

		/// <summary>Gets whether every value is a finite number and the lengths aren't negative.</summary>
		public bool IsValid => IsFinite(Offset) && IsFinite(Extent) && IsFinite(Viewport) && Extent >= 0 && Viewport >= 0;

		/// <summary>Returns these metrics scrolled to <paramref name="offset"/>.</summary>
		public ScrollAnchorMetrics WithOffset(double offset) => new(offset, Extent, Viewport);

		static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
	}
}
