using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal abstract partial class HeightConstrainedTemplatedCell : TemplatedCell
	{
		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public HeightConstrainedTemplatedCell(CGRect frame) : base(frame)
		{
		}

		public override void ConstrainTo(CGSize constraint)
		{
			ClearConstraints();
			ConstrainedDimension = constraint.Height;
		}

		protected override (bool, Size) NeedsContentSizeUpdate(Size currentSize)
		{
			var size = Size.Zero;

			if (PlatformHandler?.VirtualView == null)
			{
				return (false, size);
			}

			var bounds = PlatformHandler.VirtualView.Frame;

			if (bounds.Width <= 0 || bounds.Height <= 0)
			{
				return (false, size);
			}

			var desiredBounds = PlatformHandler.VirtualView.Measure(double.PositiveInfinity, bounds.Height);

			if (desiredBounds.Width == currentSize.Width)
			{
				// Nothing in the cell needs more room, so leave it as it is
				return (false, size);
			}

			return (true, desiredBounds);
		}
	}
}