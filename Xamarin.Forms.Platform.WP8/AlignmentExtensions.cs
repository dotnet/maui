using System.Windows;

namespace Xamarin.Forms.Platform.WinPhone
{
	internal static class AlignmentExtensions
	{
		internal static System.Windows.TextAlignment ToNativeTextAlignment(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Center:
					return System.Windows.TextAlignment.Center;
				case TextAlignment.End:
					return System.Windows.TextAlignment.Right;
				default:
					return System.Windows.TextAlignment.Left;
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