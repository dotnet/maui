using CoreGraphics;
using Foundation;

namespace Xamarin.Forms.Platform.iOS
{
	public class HorizontalTemplatedSupplementalView : TemplatedCell
	{
		public static NSString ReuseId = new NSString("Xamarin.Forms.Platform.iOS.HorizontalTemplatedSupplementalView");

		[Export("initWithFrame:")]
		public HorizontalTemplatedSupplementalView(CGRect frame) : base(frame)
		{
		}

		public override CGSize Measure()
		{
			var measure = VisualElementRenderer.Element.Measure(double.PositiveInfinity, 
				ConstrainedDimension, MeasureFlags.IncludeMargins);

			var width = VisualElementRenderer.Element.Width > 0 
				? VisualElementRenderer.Element.Width : measure.Request.Width;

			return new CGSize(width, ConstrainedDimension);
		}

		public override void ConstrainTo(CGSize constraint)
		{
			ConstrainedDimension = constraint.Height;
			Layout(constraint);
		}
	}
}