#nullable disable
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal abstract class WidthConstrainedTemplatedCell : TemplatedCell
	{
		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public WidthConstrainedTemplatedCell(CGRect frame) : base(frame)
		{
		}

		public override void ConstrainTo(CGSize constraint)
		{
			ClearConstraints();
			ConstrainedDimension = constraint.Width;
		}

		protected override (bool, Size) NeedsContentSizeUpdate(Size currentSize)
		{
			if (PlatformHandler?.VirtualView == null)
			{
				return (false, currentSize);
			}

			var bounds = PlatformHandler.VirtualView.Frame;

			if (bounds.Width <= 0 || bounds.Height <= 0)
			{
				return (false, currentSize);
			}

			var desiredBounds = PlatformHandler.VirtualView.Measure(bounds.Width, double.PositiveInfinity);

			if (desiredBounds.Height == currentSize.Height)
			{
				// Nothing in the cell needs more room, so leave it as it is
				return (false, currentSize);
			}

			// Keep the current width in the updated content size
			desiredBounds.Width = bounds.Width;

			return (true, desiredBounds);
		}
	}
}