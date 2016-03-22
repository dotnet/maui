#if __UNIFIED__
using UIKit;

#else
using MonoTouch.UIKit;
#endif

namespace Xamarin.Forms.Platform.iOS
{
	internal static class AlignmentExtensions
	{
		internal static UITextAlignment ToNativeTextAlignment(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Center:
					return UITextAlignment.Center;
				case TextAlignment.End:
					return UITextAlignment.Right;
				default:
					return UITextAlignment.Left;
			}
		}
	}
}