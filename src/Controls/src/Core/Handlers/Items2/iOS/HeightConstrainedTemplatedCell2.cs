#nullable disable
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal abstract partial class HeightConstrainedTemplatedCell2 : TemplatedCell2
	{
		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public HeightConstrainedTemplatedCell2(CGRect frame) : base(frame)
		{
		}

		// public override void ConstrainTo(CGSize constraint)
		// {
		// 	ClearConstraints();
		// 	ConstrainedDimension = constraint.Height;
		// }

		// protected override (bool, Size) NeedsContentSizeUpdate(Size currentSize)
		// {
		// 	if (PlatformHandler?.VirtualView == null)
		// 	{
		// 		return (false, currentSize);
		// 	}
		//
		// 	var bounds = PlatformHandler.VirtualView.Frame;
		//
		// 	if (bounds.Width <= 0 || bounds.Height <= 0)
		// 	{
		// 		return (false, currentSize);
		// 	}
		//
		// 	var desiredBounds = PlatformHandler.VirtualView.Measure(double.PositiveInfinity, bounds.Height);
		//
		// 	if (desiredBounds.Width == currentSize.Width)
		// 	{
		// 		// Nothing in the cell needs more room, so leave it as it is
		// 		return (false, currentSize);
		// 	}
		//
		// 	// Keep the current height in the updated content size
		// 	desiredBounds.Height = bounds.Height;
		//
		// 	return (true, desiredBounds);
		// }
	}
}