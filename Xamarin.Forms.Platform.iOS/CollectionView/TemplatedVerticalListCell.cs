using System;
using System.Diagnostics;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal sealed class TemplatedVerticalListCell : TemplatedCell
	{
		public static NSString ReuseId = new NSString("Xamarin.Forms.Platform.iOS.TemplatedVerticalListCell");

		[Export("initWithFrame:")]
		public TemplatedVerticalListCell(CGRect frame) : base(frame)
		{
		}

		public override CGSize Measure()
		{
			var measure = VisualElementRenderer.Element.Measure(ConstrainedDimension, 
				double.PositiveInfinity, MeasureFlags.IncludeMargins);

			var height = VisualElementRenderer.Element.Height > 0 
				? VisualElementRenderer.Element.Height : measure.Request.Height;

			return new CGSize(ConstrainedDimension, height);
		}

		public override void ConstrainTo(CGSize constraint)
		{
			ConstrainedDimension = constraint.Width;
			Layout(constraint);
		}
	}
}