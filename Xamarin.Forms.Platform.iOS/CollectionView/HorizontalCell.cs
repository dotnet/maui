using CoreGraphics;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal sealed class HorizontalCell : HeightConstrainedTemplatedCell
	{
		public static NSString ReuseId = new NSString("Xamarin.Forms.Platform.iOS.HorizontalCell");

		[Export("initWithFrame:")]
		[Internals.Preserve(Conditional = true)]
		public HorizontalCell(CGRect frame) : base(frame)
		{
		}

		public override CGSize Measure()
		{
			var measure = VisualElementRenderer.Element.Measure(double.PositiveInfinity,
				ConstrainedDimension, MeasureFlags.IncludeMargins);

			return new CGSize(measure.Request.Width, ConstrainedDimension);
		}

		protected override bool AttributesConsistentWithConstrainedDimension(UICollectionViewLayoutAttributes attributes)
		{
			return attributes.Frame.Width == ConstrainedDimension;
		}
	}
}