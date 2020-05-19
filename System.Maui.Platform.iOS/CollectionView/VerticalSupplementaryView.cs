using CoreGraphics;
using Foundation;

namespace System.Maui.Platform.iOS
{
	internal sealed class VerticalSupplementaryView : WidthConstrainedTemplatedCell
	{
		public static NSString ReuseId = new NSString("System.Maui.Platform.iOS.VerticalSupplementaryView");

		[Export("initWithFrame:")]
		[Internals.Preserve(Conditional = true)]
		public VerticalSupplementaryView(CGRect frame) : base(frame)
		{
		}

		public override CGSize Measure()
		{
			if (VisualElementRenderer?.Element == null)
			{
				return CGSize.Empty;
			}

			var measure = VisualElementRenderer.Element.Measure(ConstrainedDimension, 
				double.PositiveInfinity, MeasureFlags.IncludeMargins);

			var height = VisualElementRenderer.Element.Height > 0 
				? VisualElementRenderer.Element.Height : measure.Request.Height;

			return new CGSize(ConstrainedDimension, height);
		}
	}
}