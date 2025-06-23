#nullable disable
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
			=> preferredAttributes.Size.ToSize();
	}
}