using CoreGraphics;
using Foundation;

namespace Xamarin.Forms.Platform.iOS
{
	internal sealed class HorizontalSupplementaryView : HeightConstrainedTemplatedCell
	{
		public static NSString ReuseId = new NSString("Xamarin.Forms.Platform.iOS.HorizontalSupplementaryView");

		[Export("initWithFrame:")]
		[Internals.Preserve(Conditional = true)]
		public HorizontalSupplementaryView(CGRect frame) : base(frame)
		{
		}

		public override CGSize Measure()
		{
			if (VisualElementRenderer?.Element == null)
			{
				return CGSize.Empty;
			}

			var measure = VisualElementRenderer.Element.Measure(double.PositiveInfinity, 
				ConstrainedDimension, MeasureFlags.IncludeMargins);

			var width = VisualElementRenderer.Element.Width > 0 
				? VisualElementRenderer.Element.Width : measure.Request.Width;

			return new CGSize(width, ConstrainedDimension);
		}
	}
}