using Windows.UI.Xaml;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	internal static class AlignmentExtensions
	{
		internal static Windows.UI.Xaml.TextAlignment ToNativeTextAlignment(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Center:
					return Windows.UI.Xaml.TextAlignment.Center;
				case TextAlignment.End:
					return Windows.UI.Xaml.TextAlignment.Right;
				default:
					return Windows.UI.Xaml.TextAlignment.Left;
			}
		}

		internal static VerticalAlignment ToNativeVerticalAlignment(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Center:
					return VerticalAlignment.Center;
				case TextAlignment.End:
					return VerticalAlignment.Bottom;
				default:
					return VerticalAlignment.Top;
			}
		}
	}
}