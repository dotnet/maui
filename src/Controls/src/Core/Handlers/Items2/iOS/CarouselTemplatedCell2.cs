#nullable disable
using System;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal sealed class CarouselTemplatedCell2 : TemplatedCell2
	{
		internal new const string ReuseId = "Microsoft.Maui.Controls.CarouselTemplatedCell2";

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public CarouselTemplatedCell2(CGRect frame) : base(frame)
		{
		}

		private protected override Size GetMeasureConstraints(UICollectionViewLayoutAttributes preferredAttributes)
		{
			var size = preferredAttributes.Size.ToSize();
			
			var virtualView = PlatformHandler?.VirtualView;
			var parent = virtualView?.Parent;
			var handler = parent?.Handler;
			var isUnconstrained = handler is IItemsViewHandler2 { IsHeightConstrained: false };
			
			// Get the CarouselView to check its layout orientation
			var carouselView = parent as CarouselView;
			var isHorizontalCarousel = carouselView?.ItemsLayout?.Orientation == ItemsLayoutOrientation.Horizontal;
			
			// In scene-based apps, the CarouselView may initially measure with 0 height
			// When the parent has unconstrained height (e.g., horizontal CarouselView in StackLayout),
			// we should use infinite height to allow cells to self-size based on content
			if (isHorizontalCarousel && isUnconstrained)
			{
				size.Height = double.PositiveInfinity;
			}
			
			return size;
		}
	}
}