using UIKit;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.iOS
{
	internal static class AlignmentExtensions
	{
		internal static UITextAlignment ToNativeTextAlignment(this TextAlignment alignment, EffectiveFlowDirection flowDirection)
		{
			var isLtr = flowDirection.IsLeftToRight();
			switch (alignment)
			{
				case TextAlignment.Center:
					return UITextAlignment.Center;
				case TextAlignment.End:
					if (isLtr)
						return UITextAlignment.Right;
					else
						return UITextAlignment.Left;
				default:
					if (isLtr)
						return UITextAlignment.Left;
					else
						return UITextAlignment.Natural;
			}
		}
	}
}