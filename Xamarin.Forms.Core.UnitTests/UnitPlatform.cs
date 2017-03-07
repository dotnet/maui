using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	public class UnitPlatform : IPlatform
	{
		Func<VisualElement, double, double, SizeRequest> getNativeSizeFunc;
		readonly bool useRealisticLabelMeasure;

		public UnitPlatform (Func<VisualElement, double, double, SizeRequest> getNativeSizeFunc = null, bool useRealisticLabelMeasure = false)
		{
			this.getNativeSizeFunc = getNativeSizeFunc;
			this.useRealisticLabelMeasure = useRealisticLabelMeasure;
		}

		public SizeRequest GetNativeSize (VisualElement view, double widthConstraint, double heightConstraint)
		{
			if (getNativeSizeFunc != null)
				return getNativeSizeFunc (view, widthConstraint, heightConstraint);
			// EVERYTHING IS 100 x 20

			var label = view as Label;
			if (label != null && useRealisticLabelMeasure) {
				var letterSize = new Size (5, 10);
				var w = label.Text.Length * letterSize.Width;
				var h = letterSize.Height;
				if (!double.IsPositiveInfinity (widthConstraint) && w > widthConstraint) {
					h = ((int) w / (int) widthConstraint) * letterSize.Height;
					w = widthConstraint - (widthConstraint % letterSize.Width);

				}
				return new SizeRequest (new Size (w, h), new Size (Math.Min (10, w), h));
			}

			return new SizeRequest(new Size (100, 20));
		}
	}
	
}
